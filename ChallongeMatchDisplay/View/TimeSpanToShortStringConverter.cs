using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Fizzi.Applications.ChallongeVisualization.View
{
    class TimeSpanToShortStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var timeSpan = value as TimeSpan?;
            if (!timeSpan.HasValue) return null;

            var time = timeSpan.Value;

            int hours = (int)Math.Floor(time.TotalHours);
            int minutes = time.Minutes;

            var ccTimeSeparator = System.Globalization.DateTimeFormatInfo.CurrentInfo.TimeSeparator;

            return string.Format("{1}{2}{0:00}", minutes, hours, ccTimeSeparator);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
