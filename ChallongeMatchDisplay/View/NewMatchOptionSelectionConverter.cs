using System;
using System.Globalization;
using System.Windows.Data;
using Fizzi.Applications.ChallongeVisualization.Model;

namespace Fizzi.Applications.ChallongeVisualization.View;

internal class NewMatchOptionSelectionConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (!(value is NewMatchAction))
		{
			return null;
		}
		if (!int.TryParse(parameter as string, out var result))
		{
			return null;
		}
		return (NewMatchAction)value == (NewMatchAction)result;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (!(value is bool))
		{
			return null;
		}
		if (!int.TryParse(parameter as string, out var result))
		{
			return null;
		}
		return (NewMatchAction)result;
	}
}
