using CommunityToolkit.Mvvm.ComponentModel;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using NLog;
using System.ComponentModel;

namespace InertGas.Application.UI.ApplicationStages
{
    internal partial class MainPageViewModel : ApplicationStageViewModel
    {
        public List<ValveControl> ValveControls { get; } = new() {
            new ValveControl() { ValveType = ValveTypes.HeatingBox, Number = 0, IsEnabled = true, IsOn = false},
            new ValveControl() { ValveType = ValveTypes.HeatingBox, Number = 1, IsEnabled = true, IsOn = false}
        };

        public List<WorkingPhases> WorkingPhaseList { get; } = new List<WorkingPhases>() { WorkingPhases.CollectionStart, WorkingPhases.CollectionEnd, WorkingPhases.Purification, WorkingPhases.Excitation };

        [ObservableProperty]
        private WorkingPhases selectedWorkingPhase;

        public MainPageViewModel() : base(ApplicationStage.MainPage)
        {
            Title = Theme.GetString(Strings.MainPage);

            ValveControls.ToList().ForEach(valveControl =>
            {
                valveControl.PropertyChanged += OnValveControlPropertyChangedAsync;
            });
        }

        private async void OnValveControlPropertyChangedAsync(object sender, PropertyChangedEventArgs e)
        {
            var valveControl = sender as ValveControl;

            switch (e.PropertyName) 
            {
                case (nameof(ValveControl.IsOn)):
                    switch (valveControl.ValveType) 
                    {
                        case ValveTypes.HeatingBox:
                            if (valveControl.IsOn)
                                await AppModel.HeatingBoxControls[valveControl.Number].WriteCommand(HeatingBox.CommandTypes.StartHeating, 200);
                            else
                                await AppModel.HeatingBoxControls[valveControl.Number].WriteCommand(HeatingBox.CommandTypes.StopHeating);
                            break;
                    }
                    break;
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }



        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
    }
}
