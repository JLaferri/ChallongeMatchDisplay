using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Fizzi.Applications.ChallongeVisualization.View;

internal class TimeSpanToBrushConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		TimeSpan? timeSpan = value as TimeSpan?;
		if (!timeSpan.HasValue)
		{
			return new SolidColorBrush(Colors.Gray);
		}
		if (timeSpan.Value.TotalMinutes > 30.0)
		{
			return new SolidColorBrush(Color.FromRgb(byte.MaxValue, 136, 0));
		}
		return new SolidColorBrush(Color.FromRgb(80, 192, 80));
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
