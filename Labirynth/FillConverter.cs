using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Labirynth
{
    public class FillConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Square)value).Filling;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}