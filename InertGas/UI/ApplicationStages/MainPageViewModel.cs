using InertGas.Application.Model;
using InertGas.Application.Themes;

namespace InertGas.Application.UI.ApplicationStages
{
    class MainPageViewModel : ApplicationStageViewModel
    {
        public MainPageViewModel() : base(ApplicationStage.MainPage)
        {
            Title = Theme.GetString(Strings.MainPage);
        }


    }
}
