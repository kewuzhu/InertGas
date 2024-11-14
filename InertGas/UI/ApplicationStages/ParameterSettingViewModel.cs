using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.HeatingBox;
using NLog;

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
            var heatingBox = AppModel.ValveControls.FirstOrDefault(x => x.Hardware.Id == "CharcoalColumnTemperature").Hardware as HeatingBoxControl;

            while (!await heatingBox.WriteCommand(HeatingBox.CommandTypes.SetTemperatureThreshold, CharcoalColumnTemperatureThreshold))
            {
                await Task.Delay(100);
                logger_.Info("Set CharcoalColumnTemperature threshold failed. Trying again.");
            };
            logger_.Info("Set CharcoalColumnTemperature threshold succeeded.");
        }

        [RelayCommand]
        private async Task SetColumn4A5ATemperatureThreshold()
        {
            var heatingBox = AppModel.ValveControls.FirstOrDefault(x => x.Hardware.Id == "Column4A5ATemperature").Hardware as HeatingBoxControl;

            while (!await heatingBox.WriteCommand(HeatingBox.CommandTypes.SetTemperatureThreshold, Column4A5ATemperatureThreshold))
            {
                await Task.Delay(100);
                logger_.Info("Set Column4A5ATemperature threshold failed. Trying again.");
            };
            logger_.Info("Set Column4A5ATemperature threshold succeeded.");
        }

        public ParameterSettingViewModel() : base(ApplicationStage.ParameterSetting)
        {
            Title = Theme.GetString(Strings.ParameterSetting);
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
    }
}
