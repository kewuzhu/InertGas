using CommunityToolkit.Mvvm.ComponentModel;

namespace InertGas.Common.Model
{
    public partial class CurrentData : ObservableObject
    {
        [ObservableProperty]
        private string volumeFlowA;

        [ObservableProperty]
        private string volumeFlowB;

        [ObservableProperty]
        private string totalFlowB;

        [ObservableProperty]
        private double charcoalColumnTemperature;

        [ObservableProperty]
        private double column4A5ATemperature;

        [ObservableProperty]
        private string pressureA;

        [ObservableProperty]
        private string pressureB;
    }
}
