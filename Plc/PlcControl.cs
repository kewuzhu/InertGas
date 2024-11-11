using InertGas.Common.Model;
using InertGas.Common.Utility;
using NLog;

namespace InertGas.Plc
{
    public class PlcControl : SyncContextAwareObject, ISystemHardware
    {
        private const int HEARTBEAT_INTERVAL = 5000;

        public EventHandler<string> PressureDataReceived;

        public string Id { get; private set; }

        public bool IsInitialized { get; private set; }

        public void ReadPlcPressure()
        {
            int address = 3;
            int count = 1;
            ushort[] data = new ushort[2] { 0, 0 };

            if (modbusManager_.ReadPLCRegister((ushort)address, (ushort)count, ref data))
            {
                string plc = data[0].ToString();
                logger_.Info($"Succeeded in reading data from plc: {data[0]}");
                PressureDataReceived?.Invoke(this, plc);
            }
            else
            {
                logger_.Warn("Failed to read from PLC.");
            }
        }

        public void WritePump(int pumpAddress, bool turnOn)
        {
            int count = 1;

            if (modbusManager_.WritePLCCoils((ushort)pumpAddress, (ushort)count, turnOn))
            {
                logger_.Info($"Succeeded in controling the pump£¬address£º{pumpAddress} {(turnOn ? "open" : "close")}");
            }
            else
            {
                logger_.Warn($"Failed in controling the pump£¬address£º{pumpAddress}");
            }
        }

        public void Initialize(PLCConfiguration plcConfig)
        {
            try
            {
                modbusManager_ = new ModbusManager();

                bool isConn = modbusManager_.InitPLCConnection(plcConfig.IpAddress, plcConfig.Port);
                if (!isConn)
                {
                    throw new Exception("Plc Connection failed");
                }

                modbusManager_.Heartbeat(HEARTBEAT_INTERVAL);
                IsInitialized = true;
            }
            catch (Exception e)
            {
                logger_.Warn($"{e.Message}");
            }
        }

        public void Uninitialize()
        {
            try
            {
                if (modbusManager_ != null)
                {
                    modbusManager_.Uninitialize();
                    modbusManager_ = null;
                    IsInitialized = false;

                    logger_.Info("Successfully disconnected from PLC.");
                }
                else
                {
                    logger_.Warn("ModbusManager is not initialized, nothing to disconnect.");
                }
            }
            catch (Exception e)
            {
                logger_.Error($"Error while disconnecting from PLC: {e.Message}");
            }
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();

        private ModbusManager modbusManager_;
    }
}
