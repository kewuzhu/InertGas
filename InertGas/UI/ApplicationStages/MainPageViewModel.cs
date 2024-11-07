using CommunityToolkit.Mvvm.ComponentModel;
using InertGas.Application.Model;
using InertGas.Application.Themes;

namespace InertGas.Application.UI.ApplicationStages
{
    internal partial class MainPageViewModel : ApplicationStageViewModel
    {
        public List<WorkingPhases> WorkingPhaseList { get; } = new List<WorkingPhases>() { WorkingPhases.CollectionStart, WorkingPhases.CollectionEnd, WorkingPhases.Purification, WorkingPhases.Excitation};

        [ObservableProperty]
        private WorkingPhases selectedWorkingPhase;

        public MainPageViewModel() : base(ApplicationStage.MainPage)
        {
            Title = Theme.GetString(Strings.MainPage);
        }

    }
}
