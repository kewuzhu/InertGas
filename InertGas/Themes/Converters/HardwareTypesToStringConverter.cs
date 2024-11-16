using InertGas.Common.Model;
using System.Globalization;
using System.Windows.Data;

namespace InertGas.Application.Themes.Converters
{
    internal class HardwareTypesToStringConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not HardwareTypes hardwareType)
                return null;

            return hardwareType switch
            {
                HardwareTypes.HeatingBox => Theme.GetString(Strings.HeatingBox),
                HardwareTypes.FlowMeter => Theme.GetString(Strings.FlowMeter),
                HardwareTypes.Plc => "Plc",
                _ => value.ToString(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not string s)
                return null;

            if (s == Theme.GetString(Strings.HeatingBox))
                return HardwareTypes.HeatingBox;
            if (s == Theme.GetString(Strings.FlowMeter))
                return HardwareTypes.FlowMeter;
            if (s == "Plc")
                return HardwareTypes.Plc;

            return Enum.Parse(typeof(HardwareTypes), value.ToString());
        }
    }
}
