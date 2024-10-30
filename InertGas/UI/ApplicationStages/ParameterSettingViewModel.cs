using InertGas.Application.Model;
using InertGas.Application.Themes;

namespace InertGas.Application.UI.ApplicationStages
{
    class ParameterSettingViewModel : ApplicationStageViewModel
    {
        public ParameterSettingViewModel() : base(ApplicationStage.ParameterSetting)
        {
            Title = Theme.GetString(Strings.ParameterSetting);
        }


    }
}
