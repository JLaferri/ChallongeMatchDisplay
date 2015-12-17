using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Fizzi.Applications.ChallongeVisualization.View
{
    public class MultiplyDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val, multiplier;

            if (value == null || parameter == null) return null;

            //Get val
            if (value is string)
            {
				if (!double.TryParse((string)value, NumberStyles.AllowLeadingSign, null, out val)) return null;
            }
            else
            {
                try { val = (double)value; }
                catch (InvalidCastException) { return null; }
            }

            //Get multiplier
            if (parameter is string)
            {
                if (!double.TryParse((string)parameter, NumberStyles.AllowLeadingSign, null, out multiplier)) return null;
            }
            else
            {
                try { multiplier = (double)parameter; }
                catch (InvalidCastException) { return null; }
            }

            return val * multiplier;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
