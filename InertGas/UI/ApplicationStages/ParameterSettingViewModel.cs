using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.HeatingBox;
using NLog;
using static InertGas.Application.ApplicationConstants;

namespace InertGas.Application.UI.ApplicationStages
{
    internal partial class ParameterSettingViewModel : ApplicationStageViewModel
    {
        [ObservableProperty]
        private int charcoalColumnTemperatureThreshold = 200;

        [ObservableProperty]
        private int column4A5ATemperatureThreshold = 200;

        [RelayCommand]
        private async Task SetCharcoalColumnTemperatureThreshold() 
        { 
            var heatingBox = AppModel.HardwareControls.FirstOrDefault(x => x.Hardware.Id == "CharcoalColumnTemperature").Hardware as HeatingBoxControl;
            heatingBox.StopGetTemperatureTimer();
            while (!await heatingBox.WriteCommand(HeatingBox.CommandTypes.SetTemperatureThreshold, CharcoalColumnTemperatureThreshold))
            {
                await Task.Delay(RESEND_COMMAND_INTERVAL);
                logger_.Info("Set CharcoalColumnTemperature threshold failed. Trying again.");
            };
            heatingBox.StartGetTemperatureTimer();
            logger_.Info("Set CharcoalColumnTemperature threshold succeeded.");
        }

        [RelayCommand]
        private async Task SetColumn4A5ATemperatureThreshold()
        {
            var heatingBox = AppModel.HardwareControls.FirstOrDefault(x => x.Hardware.Id == "Column4A5ATemperature").Hardware as HeatingBoxControl;
            heatingBox.StopGetTemperatureTimer();
            while (!await heatingBox.WriteCommand(HeatingBox.CommandTypes.SetTemperatureThreshold, Column4A5ATemperatureThreshold))
            {
                await Task.Delay(RESEND_COMMAND_INTERVAL);
                logger_.Info("Set Column4A5ATemperature threshold failed. Trying again.");
            };
            heatingBox.StartGetTemperatureTimer();
            logger_.Info("Set Column4A5ATemperature threshold succeeded.");
        }

        public ParameterSettingViewModel() : base(ApplicationStage.ParameterSetting)
        {
            Title = Theme.GetString(Strings.ParameterSetting);
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
    }
}
