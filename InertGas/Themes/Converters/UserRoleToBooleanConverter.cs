using InertGas.Application.Themes.Converters;
using InertGas.Common.Model;
using System.Windows.Data;

namespace InertGas.Application.Themes.Converters
{
    [ValueConversionAttribute(typeof(UserRole), typeof(bool))]
    internal class UserRoleToBooleanConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : System.Windows.Data.Binding.DoNothing;
        }
    }
}
