using InertGas.Common.Model;
using System.Globalization;
using System.Windows.Data;

namespace InertGas.Application.Themes.Converters
{
    public class UserRoleToStringConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not UserRole userRole)
                return null;

            return userRole switch
            {
                UserRole.Administrator => Theme.GetString(Strings.Administrator) + Theme.GetString(Strings.User),
                UserRole.Normal => Theme.GetString(Strings.Normal) + Theme.GetString(Strings.User),
                _ => value.ToString(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not string s)
                return null;

            if (s == Theme.GetString(Strings.Administrator) + Theme.GetString(Strings.User))
                return UserRole.Administrator;
            if (s == Theme.GetString(Strings.Normal) + Theme.GetString(Strings.User))
                return UserRole.Normal;

            return Enum.Parse(typeof(UserRole), value.ToString());
        }
    }
}
