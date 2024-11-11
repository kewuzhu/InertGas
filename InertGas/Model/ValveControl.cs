using CommunityToolkit.Mvvm.ComponentModel;
using InertGas.Common.Model;

namespace InertGas.Application.Model
{
    public partial class ValveControl : ObservableObject
    {
        [ObservableProperty]
        private ValveTypes valveType;

        [ObservableProperty]
        private int number;

        [ObservableProperty]
        private bool isEnabled;

        [ObservableProperty]
        private bool isOn;

        [ObservableProperty]
        private ISystemHardware hardware;
    }
}
