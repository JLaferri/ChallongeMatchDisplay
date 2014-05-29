using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Fizzi.Applications.ChallongeVisualization.View
{
    class TimeSpanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var timeSpan = value as TimeSpan?;
            if (!timeSpan.HasValue) return new SolidColorBrush(Colors.Gray);

            var time = timeSpan.Value;

            if (time.TotalMinutes > 30) return new SolidColorBrush(Color.FromRgb(0xFF, 0xA0, 0x00));
            else return new SolidColorBrush(Color.FromRgb(0x50, 0xC0, 0x50));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
