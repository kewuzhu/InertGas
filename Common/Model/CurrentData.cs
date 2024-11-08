using CommunityToolkit.Mvvm.ComponentModel;

namespace InertGas.Common.Model
{
    public partial class CurrentData : ObservableObject
    {
        [ObservableProperty]
        private double volumeFlowA;

        [ObservableProperty]
        public double volumeFlowB;

        [ObservableProperty]
        public double charcoalColumnTemperature;

        [ObservableProperty]
        public double column4A5ATemperature;

        [ObservableProperty]
        public double pressure;
    }
}
