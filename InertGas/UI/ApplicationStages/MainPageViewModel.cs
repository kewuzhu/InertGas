using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Common.DataAccess;
using InertGas.Common.Model;
using InertGas.Common.Utility;
using InertGas.HeatingBox;
using NLog;
using System.ComponentModel;

namespace InertGas.Application.UI.ApplicationStages
{
    internal partial class MainPageViewModel : ApplicationStageViewModel
    {
        private const int DATA_SAVING_INTERVAL = 1;//min

        public List<WorkingPhases> WorkingPhaseList { get; } = new List<WorkingPhases>() { WorkingPhases.CollectionStart, WorkingPhases.CollectionEnd, WorkingPhases.Purification, WorkingPhases.Excitation };

        [ObservableProperty]
        private WorkingPhases selectedWorkingPhase;

        [ObservableProperty]
        private int charcoalColumnTemperature;

        [ObservableProperty]
        private int column4A5ATemperature;

        [ObservableProperty]
        private bool isSavingData;

        [RelayCommand]
        private void ToggleSaveData()
        {
            if (!IsSavingData)
                StartDataSavingTimer();
            else
                StopDataSavingTimer();
        }

        public MainPageViewModel(IDataRepository dataRepository) : base(ApplicationStage.MainPage)
        {
            Title = Theme.GetString(Strings.MainPage);

            dataRepository_ = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));

            AppModel.ValveControls.ForEach(valveControl =>
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
                    logger_.Info($"Current {e.PropertyName} : {AppModel.CurrentData.CharcoalColumnTemperature}");
                    break;
                case (nameof(CurrentData.Column4A5ATemperature)):
                    logger_.Info($"Current {e.PropertyName} : {AppModel.CurrentData.Column4A5ATemperature}");
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
                        case HardwareTypes.HeatingBox:
                            var heatingBox = valveControl.Hardware as HeatingBoxControl;
                            if (valveControl.IsOn)
                            {
                                while (!await heatingBox.WriteCommand(HeatingBox.CommandTypes.StartHeating))
                                {
                                    await Task.Delay(100);
                                    logger_.Info("Start heating failed. Trying again.");
                                };
                                logger_.Info("Start heating succeeded.");
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

        public void StartDataSavingTimer()
        {
            dataSavingTimer_ = new(DATA_SAVING_INTERVAL * 1000) { Enabled = true };
            dataSavingTimer_.Elapsed += OnDataSavingTimerElapsed;
            IsSavingData = true;
            logger_.Info($"{nameof(dataSavingTimer_)} started.");
        }

        public void StopDataSavingTimer()
        {
            if (dataSavingTimer_ != null)
            {
                dataSavingTimer_.Elapsed -= OnDataSavingTimerElapsed;
                dataSavingTimer_.Stop();
                dataSavingTimer_.Dispose();
                dataSavingTimer_ = null;
                IsSavingData = false;
                logger_.Info($"{nameof(dataSavingTimer_)} stopped.");
            }
        }

        private async void OnDataSavingTimerElapsed(object state, System.Timers.ElapsedEventArgs e)
        {
            var collectedData = new CollectedData()
            {
                CreatedDate = DateTime.Now,
                VolumeFlowA = AppModel.CurrentData.VolumeFlowA,
                VolumeFlowB = AppModel.CurrentData.VolumeFlowB,
                CharcoalColumnTemperature = AppModel.CurrentData.CharcoalColumnTemperature,
                Column4A5ATemperature = AppModel.CurrentData.Column4A5ATemperature,
                PressureA = AppModel.CurrentData.PressureA,
                PressureB = AppModel.CurrentData.PressureB
            };

            dataRepository_.UpsertData(collectedData);
            syncContextProxy_.ExecuteInSyncContext(() => AppModel.CollectedDataSet.Add(collectedData));
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
        private readonly IDataRepository dataRepository_;
        private readonly SyncContextProxy syncContextProxy_ = new();

        private System.Timers.Timer dataSavingTimer_;
    }
}
