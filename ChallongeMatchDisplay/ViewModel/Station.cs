using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Fizzi.Applications.ChallongeVisualization.Common;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel
{
    class Station : INotifyPropertyChanged
    {
        public string Name { get; private set; }

        private StationStatus _status;
        public StationStatus Status { get { return _status; } set { this.RaiseAndSetIfChanged("Status", ref _status, value, PropertyChanged); } }

        public Station(string name)
        {
            Name = name;
            Status = StationStatus.Open;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    enum StationStatus
    {
        Open,
        InUse
    }
}
