using NLog;
using NModbus;
using System.Net.Sockets;
using System.Timers;

namespace InertGas.Plc
{
    internal class ModbusManager
    {

        public void Uninitialize()
        {
            if (client_ != null)
            {
                client_.Close();
                client_ = null;
            }
            if (heartbeatTimer_ != null)
            {
                heartbeatTimer_.Elapsed -= OnHeartbeatTimerElapsed;
                heartbeatTimer_.Stop();
                heartbeatTimer_.Dispose();
                heartbeatTimer_ = null;
            }
        }

        public bool InitPLCConnection(string ip, int port)
        {
            try
            {
                client_ = new TcpClient(ip, port);
                var factory = new ModbusFactory();
                master_ = factory.CreateMaster(client_);
                return true;
            }
            catch (Exception ex)
            {
                logger_.Warn($"Connect to Plc failed：{ex.Message}");
                return false;
            }
        }

        public bool ReadPLCRegister(ushort address, ushort count, ref ushort[] data)
        {
            try
            {
                data = master_.ReadHoldingRegisters(1, address, count);
                return true;
            }
            catch (Exception ex)
            {
                logger_.Warn($"Read from register failed：{ex.Message}");
                return false;
            }
        }

        public bool WritePLCRegister(ushort address, ushort count, ushort[] data)
        {
            try
            {
                master_.WriteMultipleRegisters(1, address, data);
                return true;
            }
            catch (Exception ex)
            {
                logger_.Warn($"Write to register failed：{ex.Message}");
                return false;
            }
        }

        public bool WritePLCCoils(ushort address, ushort count, bool isOn)
        {
            try
            {
                master_.WriteMultipleCoils((byte)count, address, new[] { isOn });
                return true;
            }
            catch (Exception ex)
            {
                logger_.Warn($"Write to coils failed：{ex.Message}");
                return false;
            }
        }

        public bool Heartbeat(int heartTime)
        {
            heartbeatTimer_ = new();
            heartbeatTimer_.Elapsed += OnHeartbeatTimerElapsed;
            heartbeatTimer_.Start();
            return heartbeatTimer_.Enabled;
        }

        private void OnHeartbeatTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                if (master_ != null)
                {
                    ushort[] tab_reg = master_.ReadHoldingRegisters(1, 0, 1);
                }
            }
            catch (Exception ex)
            {
                logger_.Warn($"Heat beat sending failed：{ex.Message}");
            }
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();

        private TcpClient client_;
        private IModbusMaster master_;
        private System.Timers.Timer heartbeatTimer_;
    }
}
