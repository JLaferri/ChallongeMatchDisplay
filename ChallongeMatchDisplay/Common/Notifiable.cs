using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Fizzi.Applications.ChallongeVisualization.Common
{
    class Notifiable<T> : INotifyPropertyChanged
    {
        private T _value;
        public T Value { get { return _value; } set { this.RaiseAndSetIfChanged("Value", ref _value, value, PropertyChanged); } }

        public Notifiable() { }
        public Notifiable(T value)
        {
            _value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
