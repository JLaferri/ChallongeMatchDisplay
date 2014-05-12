using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fizzi.Applications.ChallongeVisualization.Model;
using Fizzi.Applications.ChallongeVisualization.Common;
using System.ComponentModel;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel
{
    class MatchDisplay : INotifyPropertyChanged
    {
        public ObservableMatch Match { get; private set; }

        private string _timeString;
        public string TimeString { get { return _timeString; } set { this.RaiseAndSetIfChanged("TimeString", ref _timeString, value, PropertyChanged); } }

        public MatchDisplay(ObservableMatch match)
        {
            Match = match;


        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
