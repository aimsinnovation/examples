using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AIMS.ApiExample
{
    public class StatusColorSelector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int status = (int)value;
            return status == 82 || status == 83 ? Brushes.LawnGreen : Brushes.DarkRed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}