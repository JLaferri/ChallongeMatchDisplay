using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Fizzi.Applications.ChallongeVisualization.Model;

namespace Fizzi.Applications.ChallongeVisualization.View
{
    class NewMatchOptionSelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is NewMatchAction)) return null;

            int num;

            //Try to parse parameter for int (target enum value)
            var intString = parameter as string;
            if (!int.TryParse(intString, out num)) return null;

            //Cast value to NewMatchAction
            var selectedAction = (NewMatchAction)value;

            //Return true if the two values match (checked)
            return selectedAction == (NewMatchAction)num;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool)) return null;

            int num;

            //Try to parse parameter for int (target enum value)
            var intString = parameter as string;
            if (!int.TryParse(intString, out num)) return null;

            //Return the casted version of the parameter to be committed to the binding
            return (NewMatchAction)num;
        }
    }
}
