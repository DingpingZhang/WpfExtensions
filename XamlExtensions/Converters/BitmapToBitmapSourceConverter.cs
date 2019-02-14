using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XamlExtensions.ExtensionMethods;

namespace XamlExtensions.Converters
{
    public class BitmapToBitmapSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Bitmap bitmap &&
                typeof(ImageSource).IsAssignableFrom(targetType))
            {
                return bitmap.ToBitmapSource();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BitmapSource bitmapSource &&
                typeof(Bitmap) == targetType)
            {
                return bitmapSource.ToBitmap();
            }

            return value;
        }
    }
}
