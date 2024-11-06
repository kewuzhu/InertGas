namespace InertGas.Common.Model
{
    public partial class CollectedData : PersistableModel
    {
        public override string Id { get; }

        public DateTime CreatedDate { get; set; }

        public double VolumeFlowA { get; set; }

        public double VolumeFlowB { get; set; }

        public double CharcoalColumnTemperature { get; set; }

        public double Column4A5ATemperature { get; set; }

        public double Pressure { get; set; }
    }
}
