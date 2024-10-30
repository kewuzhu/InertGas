using InertGas.Application.Model;
using InertGas.Application.Themes;

namespace InertGas.Application.UI.ApplicationStages
{
    class DataManagementViewModel : ApplicationStageViewModel
    {
        public DataManagementViewModel() : base(ApplicationStage.DataManagement)
        {
            Title = Theme.GetString(Strings.DataManagement);
        }


    }
}
