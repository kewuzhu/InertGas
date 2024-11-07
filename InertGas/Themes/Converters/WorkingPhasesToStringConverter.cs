using InertGas.Application.Model;
using InertGas.Common.Model;
using System.Globalization;
using System.Windows.Data;

namespace InertGas.Application.Themes.Converters
{
    public class WorkingPhasesToStringConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not WorkingPhases workingPhases)
                return null;

            return workingPhases switch
            {
                WorkingPhases.CollectionStart => Theme.GetString(Strings.CollectionStart),
                WorkingPhases.CollectionEnd => Theme.GetString(Strings.CollectionEnd),
                WorkingPhases.Purification => Theme.GetString(Strings.PurificationPhase),
                WorkingPhases.Excitation => Theme.GetString(Strings.ExcitationPhase),
                _ => value.ToString(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not string s)
                return null;

            if (s == Theme.GetString(Strings.CollectionStart))
                return WorkingPhases.CollectionStart;
            if (s == Theme.GetString(Strings.CollectionEnd))
                return WorkingPhases.CollectionEnd;
            if (s == Theme.GetString(Strings.PurificationPhase))
                return WorkingPhases.Purification;
            if (s == Theme.GetString(Strings.ExcitationPhase))
                return WorkingPhases.Excitation;

            return Enum.Parse(typeof(WorkingPhases), value.ToString());
        }
    }
}