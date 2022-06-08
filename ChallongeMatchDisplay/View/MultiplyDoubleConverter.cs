using System;
using System.Globalization;
using System.Windows.Data;

namespace Fizzi.Applications.ChallongeVisualization.View;

public class MultiplyDoubleConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null || parameter == null)
		{
			return null;
		}
		double result;
		if (value is string)
		{
			if (!double.TryParse((string)value, NumberStyles.AllowLeadingSign, null, out result))
			{
				return null;
			}
		}
		else
		{
			try
			{
				result = (double)value;
			}
			catch (InvalidCastException)
			{
				return null;
			}
		}
		double result2;
		if (parameter is string)
		{
			if (!double.TryParse((string)parameter, NumberStyles.AllowLeadingSign, null, out result2))
			{
				return null;
			}
		}
		else
		{
			try
			{
				result2 = (double)parameter;
			}
			catch (InvalidCastException)
			{
				return null;
			}
		}
		return result * result2;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
