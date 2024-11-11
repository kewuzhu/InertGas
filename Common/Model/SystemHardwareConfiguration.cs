namespace InertGas.Common.Model
{
    public class SystemHardwareConfiguration
    {
        public List<HeatingBoxConfiguration> HeatingBoxConfigs { get; set; }

        public List<HeatingBoxConfiguration> FlowMeterConfigs { get; set; }

        public PLCConfiguration PLCConfig { get; set; }
    }
}
