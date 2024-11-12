namespace InertGas.Common.Model
{
    public partial class CollectedData : PersistableModel
    {
        public override string Id { get; }

        public DateTime CreatedDate { get; set; }

        public string VolumeFlowA { get; set; }

        public string VolumeFlowB { get; set; }
        
        public string TotalFlowB { get; set; }

        public double CharcoalColumnTemperature { get; set; }

        public double Column4A5ATemperature { get; set; }

        public string Pressure { get; set; }
    }
}
