using CommunityToolkit.Mvvm.ComponentModel;
using InertGas.Common.Model;

namespace InertGas.Application.Model
{
    public partial class HardwareControl : ObservableObject
    {
        [ObservableProperty]
        private HardwareTypes hardwareType;

        [ObservableProperty]
        private int number;

        [ObservableProperty]
        private bool isEnabled;

        [ObservableProperty]
        private bool isOn;

        [ObservableProperty]
        private ISystemHardware hardware;

        [ObservableProperty]
        private PlcValve plcValve;
    }
}
