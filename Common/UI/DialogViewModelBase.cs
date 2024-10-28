using CommunityToolkit.Mvvm.ComponentModel;

namespace InertGas.Common.UI
{
    public partial class DialogViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        private bool isActive;
    }
}
