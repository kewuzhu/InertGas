using InertGas.Common.Model;
using InertGas.Common.Utility;
using NLog;
using System.IO.Ports;
using System.Text;

namespace InertGas.FlowMeter
{
    public class FlowMeterControl : SyncContextAwareObject, ISystemHardware
    {
        public EventHandler<List<string>> VolumeFlowReceived;

        private const int EVENT_WAIT_TIME = 200; //ms
        private const int READ_FLOW_DATA_TIMER_INTERVAL = 500; //ms
        private const string READ_FLOW_METER_COMMAND = "A";

        private readonly string CommandTail = ((char)0x0D).ToString();

        public bool IsInitialized { get; private set; }

        public string Id { get; private set; }

        public async Task Initialize(FlowMeterConfiguration flowMeterConfig)
        {
            if (IsInitialized)
            {
                if (flowMeterConfig.SerialConfiguration.SerialPort != comPort_)
                    throw new InvalidOperationException("Already initialized with a different port.");

                return;
            }

            Id = flowMeterConfig.Id;

            await EnableSerialPort(flowMeterConfig);

            IsInitialized = true;
        }

        private async Task EnableSerialPort(FlowMeterConfiguration serialconfig)
        {
            comPort_ = serialconfig.SerialConfiguration.SerialPort;
            serialPort_ = new SerialPort(comPort_, 19200, Parity.None, 8, StopBits.One);

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

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (readBuffer_)
            {
                int bytesToRead = serialPort_.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                serialPort_.Read(buffer, 0, bytesToRead);
                if (buffer.Length != 0)
                {
                    readBuffer_.AddRange(buffer);
                }
                replyReceived_.Set();
            }
        }

        public async Task<bool> WriteCommand()
        {
            logger_.Info($"Flow meter command writing.");
            try
            {
                await commandLock_.WaitAsync();

                serialPort_.Write(READ_FLOW_METER_COMMAND + CommandTail);
                return await GetResponse();
            }
            finally
            {
                commandLock_.Release();
            }
        }

        private async Task<bool> GetResponse()
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

                return IsResponseValid(response);
            });
        }

        private bool IsResponseValid(List<byte> response)
        {
            string data = Encoding.UTF8.GetString(response.ToArray());
            List<string> dataList = new List<string>(data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            if (dataList.Count >= 7)
            {
                string pressure = dataList[1];
                string volumeFlow = dataList[3];
                string totalFlow = dataList[6];

                logger_.Info($"Extracted value: Pressure{pressure} VolumeFlow {volumeFlow}, TotalFlow {totalFlow}");
                VolumeFlowReceived?.Invoke(this, new List<string>() { pressure, volumeFlow, totalFlow });
                return true;
            }
            else
            {
                logger_.Info("Data format is not as expected");
                return false;
            }
        }

        public void StartGetFlowDataTimer()
        {
            if (!IsInitialized)
                return;

            flowDataReadingTimer_ = new(READ_FLOW_DATA_TIMER_INTERVAL) { Enabled = true };
            flowDataReadingTimer_.Elapsed += OnFlowDataReadingTimerElapsed;
            logger_.Info($"{nameof(flowDataReadingTimer_)} started.");
        }

        public void StopGetFlowDataTimer()
        {
            if (flowDataReadingTimer_ != null)
            {
                flowDataReadingTimer_.Elapsed -= OnFlowDataReadingTimerElapsed;
                flowDataReadingTimer_.Stop();
                flowDataReadingTimer_.Dispose();
                flowDataReadingTimer_ = null;
                logger_.Info($"{nameof(flowDataReadingTimer_)} stopped.");
            }
        }

        private async void OnFlowDataReadingTimerElapsed(object state, System.Timers.ElapsedEventArgs e)
        {
            await WriteCommand();
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
        private readonly List<byte> readBuffer_ = new();
        private readonly AutoResetEvent replyReceived_ = new(false);
        private static readonly SemaphoreSlim commandLock_ = new(1);

        private System.Timers.Timer flowDataReadingTimer_;
        private SerialPort serialPort_;
        private string comPort_;
    }
}
