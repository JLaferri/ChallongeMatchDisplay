using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;

namespace Fizzi.Applications.ChallongeVisualization.View
{
    class ProgressToSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 1) return null;

            double current, minimum = 0, maximum = 100;
            current = (double)values[0];
            if (values.Length >= 2) minimum = (double)values[1];
            if (values.Length >= 3) maximum = (double)values[2];

            double ratio = (double)(current - minimum) / (maximum - minimum);
            return new RectangleGeometry(new System.Windows.Rect(0, 0, ratio, 1));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
