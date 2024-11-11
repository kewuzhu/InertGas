using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Common.Model;
using InertGas.HeatingBox;
using NLog;
using System.ComponentModel;

namespace InertGas.Application.UI.ApplicationStages
{
    internal partial class MainPageViewModel : ApplicationStageViewModel
    {
        public List<WorkingPhases> WorkingPhaseList { get; } = new List<WorkingPhases>() { WorkingPhases.CollectionStart, WorkingPhases.CollectionEnd, WorkingPhases.Purification, WorkingPhases.Excitation };

        [ObservableProperty]
        private WorkingPhases selectedWorkingPhase;

        [ObservableProperty]
        private int charcoalColumnTemperature;

        [ObservableProperty]
        private int column4A5ATemperature;

        [RelayCommand]
        private void SaveData()
        {

        }

        public MainPageViewModel() : base(ApplicationStage.MainPage)
        {
            Title = Theme.GetString(Strings.MainPage);

            AppModel.ValveControls.ToList().ForEach(valveControl =>
            {
                valveControl.PropertyChanged += OnValveControlPropertyChangedAsync;
            });

            AppModel.CurrentData.PropertyChanged += OnAppModelCurrentDataPropertyChanged;
        }

        private void OnAppModelCurrentDataPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case (nameof(CurrentData.CharcoalColumnTemperature)):
                case (nameof(CurrentData.Column4A5ATemperature)):
                    if (AppModel.CurrentData.Column4A5ATemperature >= 200 || AppModel.CurrentData.Column4A5ATemperature >= 200)
                    {
                        var heatingBox = AppModel.ValveControls.FirstOrDefault(x => x.Hardware.Id == e.PropertyName).Hardware as HeatingBoxControl;
                        heatingBox.SetTemperatureTo300AfterDelay();
                        return;
                    }
                    break;

            }
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
                            var heatingBox = valveControl.Hardware as HeatingBoxControl;
                            if (valveControl.IsOn)
                            {
                                while (!await heatingBox.WriteCommand(HeatingBox.CommandTypes.StartHeating))
                                {
                                    await Task.Delay(100);
                                    logger_.Info("Start heating failed. Trying again.");
                                };
                                await heatingBox.SetTemperatureTo200();
                                logger_.Info("Start heating and set temperature threshold succeeded.");
                            }
                            else
                            {
                                while (!await heatingBox.WriteCommand(HeatingBox.CommandTypes.StopHeating))
                                {
                                    await Task.Delay(100);
                                }
                                logger_.Info("Stop heating succeeded.");
                            }
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
