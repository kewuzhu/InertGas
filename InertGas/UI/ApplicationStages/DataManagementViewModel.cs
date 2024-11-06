using CommunityToolkit.Mvvm.ComponentModel;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Common.Model;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace InertGas.Application.UI.ApplicationStages
{
    partial class DataManagementViewModel : ApplicationStageViewModel
    {
        [ObservableProperty]
        private DateTime selectedStartDate = DateTime.Now;

        [ObservableProperty]
        private DateTime selectedEndDate = DateTime.Now;

        [ObservableProperty]
        private CollectedData selectedData;

        public DataManagementViewModel() : base(ApplicationStage.DataManagement)
        {
            Title = Theme.GetString(Strings.DataManagement);
        }

    }
}
