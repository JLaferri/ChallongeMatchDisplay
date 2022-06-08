using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Fizzi.Applications.ChallongeVisualization.View;

internal class ProgressToSizeConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length < 1)
		{
			return null;
		}
		double num = 0.0;
		double num2 = 100.0;
		double num3 = (double)values[0];
		if (values.Length >= 2)
		{
			num = (double)values[1];
		}
		if (values.Length >= 3)
		{
			num2 = (double)values[2];
		}
		double width = (num3 - num) / (num2 - num);
		return new RectangleGeometry(new Rect(0.0, 0.0, width, 1.0));
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
