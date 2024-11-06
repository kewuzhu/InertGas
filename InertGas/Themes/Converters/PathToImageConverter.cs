using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InertGas.Application.Themes.Converters
{
    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class PathToImageConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string path) return null;

            var fileExtension = Path.GetExtension(path).ToLower();
            return fileExtension switch
            {
                ".xaml" => LoadDrawingImageFromFile(path),
                _ => LoadBitmapImageFromFile(path),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static DrawingImage LoadDrawingImageFromFile(string file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            return XamlReader.Load(fileStream) as DrawingImage;
        }

        private static BitmapImage LoadBitmapImageFromFile(string file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var image = new BitmapImage();
            using (var stream = File.OpenRead(file))
            {
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
            }
            return image;
        }
    }
}
