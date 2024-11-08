using CommunityToolkit.Mvvm.ComponentModel;

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
    }
}
