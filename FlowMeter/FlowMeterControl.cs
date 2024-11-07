using InertGas.Common.Model;
using NLog;
using System.IO.Ports;

namespace InertGas.FlowMeter
{
    public class FlowMeterControl
    {
        public bool IsInitialized { get; private set; }

        public string Id { get; set; }

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

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
        private readonly List<byte> readBuffer_ = new();
        private readonly AutoResetEvent replyReceived_ = new(false);
        private static readonly SemaphoreSlim commandLock_ = new(1);

        private SerialPort serialPort_;
        private string comPort_;
    }
}
