using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Common.DataAccess;
using InertGas.Common.Model;
using InertGas.Common.Utility;
using InertGas.HeatingBox;
using InertGas.Plc;
using NLog;
using System.ComponentModel;
using static InertGas.Application.ApplicationConstants;

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

            AppModel.HardwareControls.ForEach(HardwareControl =>
            {
                HardwareControl.PropertyChanged += OnHardwareControlPropertyChangedAsync;
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

        private async void OnHardwareControlPropertyChangedAsync(object sender, PropertyChangedEventArgs e)
        {
            var HardwareControl = sender as HardwareControl;

            switch (e.PropertyName)
            {
                case (nameof(HardwareControl.IsOn)):
                    switch (HardwareControl.HardwareType)
                    {
                        case HardwareTypes.HeatingBox:
                            var heatingBox = HardwareControl.Hardware as HeatingBoxControl;
                            if (HardwareControl.IsOn)
                            {
                                heatingBox.StopGetTemperatureTimer();
                                while (!await heatingBox.WriteCommand(HeatingBox.CommandTypes.StartHeating))
                                {
                                    await Task.Delay(RESEND_COMMAND_INTERVAL);
                                    logger_.Warn("Start heating failed. Trying again.");
                                };
                                heatingBox.StartGetTemperatureTimer();
                                logger_.Info("Start heating succeeded.");
                            }
                            else if (!HardwareControl.IsOn)
                            {
                                heatingBox.StopGetTemperatureTimer();
                                while (!await heatingBox.WriteCommand(HeatingBox.CommandTypes.StopHeating))
                                {
                                    await Task.Delay(RESEND_COMMAND_INTERVAL);
                                    logger_.Warn("Stop heating failed. Trying again.");
                                }
                                heatingBox.StartGetTemperatureTimer();
                                logger_.Info("Stop heating succeeded.");
                            }
                            break;
                        case HardwareTypes.Plc:
                            var plcValveAddress = HardwareControl.PlcValve.Address;
                            var plcValveType = HardwareControl.PlcValve.ControlType;
                            var plcValveNumber = HardwareControl.PlcValve.Number;

                            var plc = HardwareControl.Hardware as PlcControl;
                            if (HardwareControl.IsOn)
                            {
                                plc.WriteCoil(plcValveAddress, true);
                                logger_.Info($"PlcValveType: {plcValveType}, Number:{plcValveNumber}, Address{plcValveAddress} is on.");
                            }
                            else if (!HardwareControl.IsOn)
                            {
                                plc.WriteCoil(plcValveAddress, false);
                                logger_.Info($"PlcValveType: {plcValveType}, Number:{plcValveNumber}, Address{plcValveAddress} is off.");
                            }
                            break;
                    }
                    break;
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e) 
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(SelectedWorkingPhase):
                    AppModel.HardwareControls.ForEach(x => x.IsEnabled = false);
                    var plcControls = AppModel.HardwareControls.Where(x => x.HardwareType == HardwareTypes.Plc).ToList();
                    var heatingBoxes = AppModel.HardwareControls.Where(x => x.HardwareType == HardwareTypes.HeatingBox).ToList();
                    switch (SelectedWorkingPhase) 
                    {
                        case WorkingPhases.CollectionStart:
                            plcControls.Where(x => x.PlcValve.ControlType == PlcControlTypes.FiveWayValve && x.PlcValve.Number == 1).FirstOrDefault().IsEnabled = true;
                            plcControls.Where(x => x.PlcValve.ControlType == PlcControlTypes.ElectricalValve && x.PlcValve.Number == 1).FirstOrDefault().IsEnabled = true;
                            plcControls.Where(x => x.PlcValve.ControlType == PlcControlTypes.ElectricalValve && x.PlcValve.Number == 3).FirstOrDefault().IsEnabled = true;
                            plcControls.Where(x => x.PlcValve.ControlType == PlcControlTypes.PneumaticPump).ToList().ForEach(x => x.IsEnabled = true);
                            break;
                        case WorkingPhases.CollectionEnd:
                            plcControls.Where(x => x.PlcValve.ControlType == PlcControlTypes.ElectricalValve && x.PlcValve.Number == 1).FirstOrDefault().IsEnabled = true;
                            plcControls.Where(x => x.PlcValve.ControlType == PlcControlTypes.ElectricalValve && x.PlcValve.Number == 3).FirstOrDefault().IsEnabled = true;
                            plcControls.Where(x => x.PlcValve.ControlType == PlcControlTypes.PneumaticPump).ToList().ForEach(x => x.IsEnabled = true);
                            break;
                        case WorkingPhases.Purification:
                            heatingBoxes.Where(x => x.Number == 1).FirstOrDefault().IsEnabled = true;
                            plcControls.Where(x => x.PlcValve.ControlType == PlcControlTypes.FiveWayValve).ToList().ForEach(x => x.IsEnabled = true);
                            break;
                        case WorkingPhases.Excitation:
                            heatingBoxes.ForEach(x => x.IsEnabled = true);
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

        private void OnDataSavingTimerElapsed(object state, System.Timers.ElapsedEventArgs e)
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
