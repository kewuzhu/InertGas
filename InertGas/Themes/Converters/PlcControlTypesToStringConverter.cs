using InertGas.Common.Model;
using System.Globalization;
using System.Windows.Data;

namespace InertGas.Application.Themes.Converters
{
    internal class PlcControlTypesToStringConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not PlcControlTypes controlType)
                return null;

            return controlType switch
            {
                PlcControlTypes.ElectricalValve => Theme.GetString(Strings.ElectricalValve),
                PlcControlTypes.FiveWayValve => Theme.GetString(Strings.FiveWayValve),
                PlcControlTypes.PneumaticPump => Theme.GetString(Strings.PneumaticPump),
                PlcControlTypes.DoubleSolenoidValve => Theme.GetString(Strings.DoubleSolenoidValve),
                _ => value.ToString(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not string s)
                return null;

            if (s == Theme.GetString(Strings.ElectricalValve))
                return PlcControlTypes.ElectricalValve;
            if (s == Theme.GetString(Strings.FiveWayValve))
                return PlcControlTypes.FiveWayValve;
            if (s == Theme.GetString(Strings.PneumaticPump))
                return PlcControlTypes.PneumaticPump;
            if (s == Theme.GetString(Strings.DoubleSolenoidValve))
                return PlcControlTypes.DoubleSolenoidValve;

            return Enum.Parse(typeof(PlcControlTypes), value.ToString());
        }
    }
}
