using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.FlowMeter;
using InertGas.HeatingBox;
using NLog;
using static InertGas.Application.ApplicationConstants;

namespace InertGas.Application.UI.ApplicationStages
{
    internal partial class ParameterSettingViewModel : ApplicationStageViewModel
    {
        [RelayCommand]
        private static async Task SetCharcoalColumnTemperatureThreshold() 
        { 
            var heatingBox = AppModel.HardwareControls.FirstOrDefault(x => x.Hardware.Id == "CharcoalColumnTemperature").Hardware as HeatingBoxControl;
            heatingBox.StopGetTemperatureTimer();
            while (!await heatingBox.WriteCommand(HeatingBox.CommandTypes.SetTemperatureThreshold, AppModel.CharcoalColumnTemperatureThreshold))
            {
                await Task.Delay(RESEND_COMMAND_INTERVAL);
                logger_.Info("Set CharcoalColumnTemperature threshold failed. Trying again.");
            };
            heatingBox.StartGetTemperatureTimer();
            logger_.Info("Set CharcoalColumnTemperature threshold succeeded.");
        }

        [RelayCommand]
        private static async Task SetColumn4A5ATemperatureThreshold()
        {
            var heatingBox = AppModel.HardwareControls.FirstOrDefault(x => x.Hardware.Id == "Column4A5ATemperature").Hardware as HeatingBoxControl;
            heatingBox.StopGetTemperatureTimer();
            while (!await heatingBox.WriteCommand(HeatingBox.CommandTypes.SetTemperatureThreshold, AppModel.Column4A5ATemperatureThreshold))
            {
                await Task.Delay(RESEND_COMMAND_INTERVAL);
                logger_.Info("Set Column4A5ATemperature threshold failed. Trying again.");
            };
            heatingBox.StartGetTemperatureTimer();
            logger_.Info("Set Column4A5ATemperature threshold succeeded.");
        }

        [RelayCommand]
        private static async Task SetVolumeFlowAThreshold()
        {
            var flowMeter = AppModel.HardwareControls.FirstOrDefault(x => x.Hardware.Id == "VolumeFlowA").Hardware as FlowMeterControl;
            flowMeter.StopGetFlowDataTimer();
            while (!await flowMeter.SetVolumeFlowCommand(AppModel.VolumeFlowAThreshold))
            {
                await Task.Delay(RESEND_COMMAND_INTERVAL);
                logger_.Info("Set VolumeFlowA threshold failed. Trying again.");
            };
            flowMeter.StartGetFlowDataTimer();
            logger_.Info("Set VolumeFlowA threshold succeeded.");
        }

        [RelayCommand]
        private static async Task SetVolumeFlowBThreshold()
        {
            var flowMeter = AppModel.HardwareControls.FirstOrDefault(x => x.Hardware.Id == "VolumeFlowB").Hardware as FlowMeterControl;
            flowMeter.StopGetFlowDataTimer();
            while (!await flowMeter.SetVolumeFlowCommand(AppModel.VolumeFlowBThreshold))
            {
                await Task.Delay(RESEND_COMMAND_INTERVAL);
                logger_.Info("Set VolumeFlowB threshold failed. Trying again.");
            };
            flowMeter.StartGetFlowDataTimer();
            logger_.Info("Set VolumeFlowB threshold succeeded.");
        }

        public ParameterSettingViewModel() : base(ApplicationStage.ParameterSetting)
        {
            Title = Theme.GetString(Strings.ParameterSetting);
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
    }
}
