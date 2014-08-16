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

        public Station GetHighestPriorityOpenStation()
        {
            if (Dict == null) return null;

            return Dict.Values.Where(s => s.Status == StationStatus.Open && s.Priority != null).OrderBy(s => s.Priority).FirstOrDefault();
        }

        public void AssignOpenMatchesToStations(ObservableMatch[] matches)
        {
            if (Dict == null) return;

            var orderedPending = matches.Where(m => !m.IsMatchInProgress).OrderBy(m => m.Player1.Seed + m.Player2.Seed)
                    .ThenBy(m => (new[] { m.Player1.Seed, m.Player2.Seed }).Min()).ToArray();
            var orderedStations = Dict.Values.Where(s => s.Status == StationStatus.Open && s.Priority != null).OrderBy(s => s.Priority).ToArray();

            var zipped = orderedPending.Zip(orderedStations, (m, s) => new { Match = m, Station = s }).ToArray();

            foreach (var pair in zipped) pair.Match.AssignPlayersToStation(pair.Station.Name);
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
        public int? Priority { get; private set; }

        public int Order { get; private set; }

        private StationStatus _status;
        public StationStatus Status { get { return _status; } 
            set 
            { 
                this.RaiseAndSetIfChanged("Status", ref _status, value, PropertyChanged); 
            } 
        }

        public Station(string name, int order) : this(name, order, null) { }

        public Station(string name, int order, int? priority)
        {
            Name = name;
            Order = order;
            Priority = priority;
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
