using InertGas.Application.Model;
using System.Windows.Data;

namespace InertGas.Application.Themes.Converters
{
    [ValueConversionAttribute(typeof(ApplicationMode), typeof(bool))]
    internal class ApplicationModeToBooleanConverter : BaseConverter, IValueConverter
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
