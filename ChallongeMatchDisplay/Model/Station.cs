using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Fizzi.Applications.ChallongeVisualization.Common;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    class Stations
    {
        #region Singleton Pattern Region
        private static volatile Stations instance;
        private static object syncRoot = new Object();

        private Stations() { }

        public static Stations Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null) instance = new Stations();
                    }
                }

                return instance;
            }
        }
        #endregion

        public Dictionary<string, Station> Dict { get; private set; }

        public void LoadNew(IEnumerable<Station> stations)
        {
            Dict = stations.ToDictionary(s => s.Name, s => s);
        }

        public void AttemptFreeStation(string stationName)
        {
            if (Dict != null && stationName != null)
            {
                Station s;

                if (Dict.TryGetValue(stationName, out s))
                {
                    s.Status = StationStatus.Open;
                }
            }
        }

        public void AttemptClaimStation(string stationName)
        {
            if (Dict != null && stationName != null)
            {
                Station s;

                if (Dict.TryGetValue(stationName, out s))
                {
                    s.Status = StationStatus.InUse;
                }
            }
        }
    }

    class Station : INotifyPropertyChanged
    {
        public string Name { get; private set; }

        public int Order { get; private set; }

        private StationStatus _status;
        public StationStatus Status { get { return _status; } 
            set 
            { 
                this.RaiseAndSetIfChanged("Status", ref _status, value, PropertyChanged); 
            } 
        }

        public Station(string name, int order)
        {
            Name = name;
            Order = order;
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
