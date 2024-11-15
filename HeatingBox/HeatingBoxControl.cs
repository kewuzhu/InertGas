using InertGas.Common.Model;
using InertGas.Common.Utility;
using NLog;
using RJCP.IO.Ports;

namespace InertGas.HeatingBox
{
    public class HeatingBoxControl : SyncContextAwareObject, ISystemHardware
    {
        public event EventHandler<int> TemperatureDataReceived;

        private const int EVENT_WAIT_TIME = 200; //ms
        private const int READ_TEMPERATRUE_TIMER_INTERVAL = 500; //ms
        private const int SET_TEMPERATURE_TO_300_DELAY = 20; //min

        private static readonly byte[] SetTemperatureThresholdCommandHeader = new byte[]
            {
                0x01, 0x10,
                0x21, 0x03,
                0x00, 0x01
            };

        private static readonly byte[] SetTemperatureSuccessMsg = new byte[]
            {
                0x01, 0x10,
                0x21, 0x03,
                0x00, 0x01,
                0xFB, 0xF5
            };

        private static readonly byte[] StartHeatingCommand = new byte[]
            {
                0x01, 0x06,
                0x00, 0x00,
                0x01, 0x00
            };

        private static readonly byte[] StopHeatingCommand = new byte[]
            {
                0x01, 0x06,
                0x00, 0x00,
                0x01, 0x01
            };

        private static readonly byte[] ReadTemperatureCommand = new byte[]
            {
                0x01, 0x03,
                0x00, 0x00,
                0x00, 0x02
            };

        private static readonly byte[] ReadTemperatureReplyCommandHeader = new byte[]
            {
                0x01, 0x03,
                0x04
            };

        public bool IsInitialized { get; private set; }

        public string Id { get; private set; }

        public async Task Initialize(HeatingBoxConfiguration heatingBoxConfig)
        {
            if (IsInitialized)
            {
                if (heatingBoxConfig.SerialConfiguration.SerialPort != comPort_)
                    throw new InvalidOperationException("Already initialized with a different port.");

                return;
            }

            Id = heatingBoxConfig.Id;

            await EnableSerialPort(heatingBoxConfig);

            IsInitialized = true;
        }

        private async Task EnableSerialPort(HeatingBoxConfiguration serialconfig)
        {
            comPort_ = serialconfig.SerialConfiguration.SerialPort;
            serialPort_ = new SerialPortStream(comPort_, 9600, 8, Parity.None, StopBits.One);

            await Task.Run(() =>
            {
                serialPort_.Open();
                serialPort_.DiscardInBuffer();
                serialPort_.DiscardOutBuffer();
                serialPort_.DataReceived += OnDataReceived;
            });
        }

        public async Task Uninitialize()
        {
            if (!IsInitialized) return;
            await Task.Run(() =>
            {
                serialPort_.DataReceived -= OnDataReceived;
                IsInitialized = false;
                serialPort_.Close();
            });
        }

        public async Task<bool> WriteCommand(CommandTypes cmd, int parameter = 0)
        {
            try
            {
                await commandLock_.WaitAsync();

                var isCommandSuccessful = false;
                switch (cmd)
                {
                    case CommandTypes.ReadTemperature:
                        isCommandSuccessful = await WriteReadTemperatureCommand(cmd);
                        break;
                    case CommandTypes.SetTemperatureThreshold:
                        isCommandSuccessful = await WriteSetTemperatrueThresholdCommand(cmd, parameter);
                        break;
                    case CommandTypes.StopHeating:
                        isCommandSuccessful = await WriteStopHeatingCommand(cmd);
                        isHeating_ = !isCommandSuccessful;
                        break;
                    case CommandTypes.StartHeating:
                        isCommandSuccessful = await WriteStartHeatingCommand(cmd);
                        isHeating_ = isCommandSuccessful;
                        break;
                    default:
                        isCommandSuccessful = false;
                        break;
                }
                if (isCommandSuccessful)
                {
                    logger_.Info($"{cmd} successfully.");
                }
                else
                {
                    logger_.Warn($"{cmd} failed.");
                }
                return isCommandSuccessful;
            }
            finally
            {
                commandLock_.Release();
            }
        }

        private async Task<bool> WriteReadTemperatureCommand(CommandTypes cmd)
        {
            var command = ConcatCommandWithCRC(ReadTemperatureCommand);
            serialPort_.Write(command, 0, command.Length);
            logger_.Info($"{cmd} is sent.");
            return await GetResponse(cmd);
        }

        private async Task<bool> WriteSetTemperatrueThresholdCommand(CommandTypes cmd, int parameter)
        {
            var command = BuildCommand(parameter);
            serialPort_.Write(command, 0, command.Length);
            logger_.Info($"{cmd} is sent.");
            return await GetResponse(cmd);
        }

        private async Task<bool> WriteStopHeatingCommand(CommandTypes cmd)
        {
            if (!isHeating_)
                return true;

            var command = ConcatCommandWithCRC(StopHeatingCommand);
            serialPort_.Write(command, 0, command.Length);
            logger_.Info($"{cmd} is sent.");
            return await GetResponse(cmd);
        }

        private async Task<bool> WriteStartHeatingCommand(CommandTypes cmd)
        {
            if (isHeating_)
                return false;

            var command = ConcatCommandWithCRC(StartHeatingCommand);
            serialPort_.Write(command, 0, command.Length);
            logger_.Info($"{cmd} is sent.");
            return await GetResponse(cmd);
        }

        private async Task<bool> GetResponse(CommandTypes cmd)
        {
            return await Task.Run(() =>
            {
                var response = new List<byte>();

                lock (readBuffer_)
                {
                    replyReceived_.WaitOne(EVENT_WAIT_TIME);

                    response = new List<byte>(readBuffer_);
                    readBuffer_.Clear();
                }

                return IsResponseValid(cmd, response);
            });
        }

        private bool IsResponseValid(CommandTypes cmd, List<byte> response)
        {
            return cmd switch
            {
                CommandTypes.StartHeating => IsStartHeatingSuccessful(response),
                CommandTypes.SetTemperatureThreshold => IsTwoArraySame(response.ToArray(), SetTemperatureSuccessMsg),
                CommandTypes.StopHeating => IsStopHeatingSuccessful(response),
                CommandTypes.ReadTemperature => ParseBytesToInt(response.ToArray()),
                _ => false,
            };
        }

        private static byte[] BuildCommand(int parameter)
        {
            byte[] data = BitConverter.GetBytes((short)parameter).Reverse().ToArray();
            byte dataLength = (byte)data.Length;

            byte[] commandWithoutCRC = SetTemperatureThresholdCommandHeader
                .Concat(new[] { dataLength })
                .Concat(data)
                .ToArray();

            return ConcatCommandWithCRC(commandWithoutCRC);
        }

        private static byte[] ConcatCommandWithCRC(byte[] commandWithoutCRC)
        {
            ushort crc16 = CalculateCRC16(commandWithoutCRC);

            byte[] crcBytes = BitConverter.GetBytes(crc16).ToArray();

            byte[] fullCommand = commandWithoutCRC
                .Concat(crcBytes)
                .ToArray();

            logger_.Info($"Command is built: {BitConverter.ToString(fullCommand).Replace("-", " ")}");

            return fullCommand;
        }

        private static ushort CalculateCRC16(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in data)
            {
                crc ^= b;
                for (int i = 0; i < 8; ++i)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }

        private bool IsStartHeatingSuccessful(List<byte> response)
        {
            var theoretical = ConcatCommandWithCRC(StartHeatingCommand);
            var actual = response.ToArray();

            return IsTwoArraySame(actual, theoretical);
        }

        private bool IsStopHeatingSuccessful(List<byte> response)
        {
            var theoretical = ConcatCommandWithCRC(StopHeatingCommand);
            var actual = response.ToArray();

            return IsTwoArraySame(actual, theoretical);
        }

        private bool IsTwoArraySame(byte[] actual, byte[] theoretical)
        {
            if (actual.Length != theoretical.Length)
                return false;

            for (int i = 0; i < theoretical.Length; i++)
            {
                if (actual[i] != theoretical[i])
                    return false;
            }

            return true;
        }

        private bool ParseBytesToInt(byte[] bytes)
        {
            if (bytes.Length < ReadTemperatureReplyCommandHeader.Length)
                return false;

            for (int i = 0; i < ReadTemperatureReplyCommandHeader.Length; i++)
            {
                if (bytes[i] != ReadTemperatureReplyCommandHeader[i])
                {
                    logger_.Info($"Not a valid temperature readout");
                    return false;
                }
            }

            var crcReceived = bytes.TakeLast(2).ToArray();
            var replyWithoutCRC = bytes.SkipLast(2).ToArray();

            ushort crc16 = CalculateCRC16(replyWithoutCRC);

            byte[] crcBytes = BitConverter.GetBytes(crc16).ToArray();

            if (!crcReceived.SequenceEqual(crcBytes))
            {
                logger_.Info("Temperature readout CRC verification failed");
                return false;
            }

            var temperatureBytes = replyWithoutCRC.Skip(3).ToArray();

            int temperature = 0;

            for (int i = 0; i < temperatureBytes.Length; i++)
            {
                temperature = (temperature << 8) | temperatureBytes[i];
            }

            TemperatureDataReceived?.Invoke(this, temperature);

            return true;
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (readBuffer_)
            {
                int bytesToRead = serialPort_.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                serialPort_.Read(buffer, 0, bytesToRead);
                readBuffer_.AddRange(buffer);
                replyReceived_.Set();
            }
        }

        public void StartGetTemperatureTimer()
        {
            if (!IsInitialized)
                return;

            temperatureReadingTimer_ = new(READ_TEMPERATRUE_TIMER_INTERVAL) { Enabled = true };
            temperatureReadingTimer_.Elapsed += OnTemperatureReadingTimerElapsed;
            logger_.Info($"{nameof(temperatureReadingTimer_)} started.");
        }

        public void StopGetTemperatureTimer()
        {
            if (temperatureReadingTimer_ != null)
            {
                temperatureReadingTimer_.Elapsed -= OnTemperatureReadingTimerElapsed;
                temperatureReadingTimer_.Stop();
                temperatureReadingTimer_.Dispose();
                temperatureReadingTimer_ = null;
                logger_.Info($"{nameof(temperatureReadingTimer_)} stopped.");
            }
        }

        private async void OnTemperatureReadingTimerElapsed(object state, System.Timers.ElapsedEventArgs e)
        {
            await WriteCommand(CommandTypes.ReadTemperature);
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
        private readonly List<byte> readBuffer_ = new();
        private readonly AutoResetEvent replyReceived_ = new(false);
        private static readonly SemaphoreSlim commandLock_ = new(1);

        private System.Timers.Timer temperatureReadingTimer_;
        private SerialPortStream serialPort_;
        private string comPort_;
        private bool isHeating_;
    }
}
