using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Fizzi.Applications.ChallongeVisualization.Common
{
    static class NotifyPropertyChangedEx
    {
        public static void RaiseAndSetIfChanged<T>(this INotifyPropertyChanged notifier, string property, ref T backing, T newVal,
            PropertyChangedEventHandler handler)
        {
            if ((backing != null && !backing.Equals(newVal)) || (newVal != null && !newVal.Equals(backing)))
            {
                backing = newVal;

                if (handler != null)
                {
                    handler(notifier, new PropertyChangedEventArgs(property));
                }
            }
        }

        public static void Raise(this INotifyPropertyChanged notifier, string property, PropertyChangedEventHandler handler)
        {
            if (handler != null)
            {
                handler(notifier, new PropertyChangedEventArgs(property));
            }
        }
    }
}
