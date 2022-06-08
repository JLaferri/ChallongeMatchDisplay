using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Fizzi.Applications.ChallongeVisualization.View;

internal class ObjectEqualityConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length <= 1)
		{
			return true;
		}
		object firstItem = values[0];
		return values.Skip(1).All((object o) => object.Equals(o, firstItem));
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
