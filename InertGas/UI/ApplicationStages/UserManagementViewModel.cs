using InertGas.Application.Model;
using InertGas.Application.Themes;

namespace InertGas.Application.UI.ApplicationStages
{
    class UserManagementViewModel : ApplicationStageViewModel
    {
        public UserManagementViewModel() : base(ApplicationStage.UserManagement)
        {
            Title = Theme.GetString(Strings.UserManagement);
        }
    }
}
