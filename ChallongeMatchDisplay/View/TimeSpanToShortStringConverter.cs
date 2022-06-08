using System;
using System.Globalization;
using System.Windows.Data;

namespace Fizzi.Applications.ChallongeVisualization.View;

internal class TimeSpanToShortStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		TimeSpan? timeSpan = value as TimeSpan?;
		if (!timeSpan.HasValue)
		{
			return null;
		}
		TimeSpan value2 = timeSpan.Value;
		if (value2 < TimeSpan.Zero)
		{
			return "0:00";
		}
		int minutes = value2.Minutes;
		int seconds = value2.Seconds;
		string timeSeparator = DateTimeFormatInfo.CurrentInfo.TimeSeparator;
		return string.Format("{1}{2}{0:00}", seconds, minutes, timeSeparator);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
