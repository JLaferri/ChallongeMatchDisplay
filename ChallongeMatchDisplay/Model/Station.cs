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

        public Station GetBestNormalStation()
        {
            if (Dict == null) return null;

            return Dict.Values.Where(s => s.Status == StationStatus.Open && s.Type != StationType.Stream && s.Type != StationType.Recording &&
                s.Type != StationType.NoAssign).OrderBy(s => s.Type).ThenBy(s => s.Order).FirstOrDefault();
        }

        public void AssignOpenMatchesToStations(ObservableMatch[] matches)
        {
            if (Dict == null) return;

            var allOpenMatches = matches.Where(m => !m.IsMatchInProgress).ToArray();
            var allOpenStations = Dict.Values.Where(s => s.Status == StationStatus.Open && s.Type != StationType.NoAssign).OrderBy(s => s.Type).ThenBy(s => s.Order).ToArray();

            var openStationCount = allOpenStations.Length;

            //If there are no stations or no matches, return
            if (openStationCount == 0 || allOpenMatches.Length == 0) return;

            //Get list of matches that should be considered for assignment
            var orderedMatches = allOpenMatches.OrderBy(m => m.RoundOrder).ThenBy(m => m.IsWinners).ThenBy(m => m.Identifier).ToArray();
            var lastMatch = orderedMatches.Select((m, i) => new { Match = m, Index = i }).Take(openStationCount).Last();

            //Get matches that will be considered for assignment. This will prioritize earlier rounds but still allow the most recent round there are stations for to consider all matches in that round
            var matchesToConsider = orderedMatches.TakeWhile((m, i) => i <= lastMatch.Index || (m.IsWinners == lastMatch.Match.IsWinners && m.RoundOrder == lastMatch.Match.RoundOrder)).ToList();

            var streamStations = allOpenStations.Where(s => s.Type == StationType.Stream || s.Type == StationType.Recording).ToArray();
            var streamStationCount = streamStations.Length;

            //Organize matches by seed to put best matches on stream
            var seedPrioritizedMatches = matchesToConsider.OrderBy(m => m.Player1.Seed + m.Player2.Seed).ThenBy(m => (new[] { m.Player1.Seed, m.Player2.Seed }).Min()).ToArray();

            //Combine stream stations with highest priority seed matches
            var streamAssignments = seedPrioritizedMatches.Zip(streamStations, (m, s) => new { Match = m, Station = s }).ToArray();

            //Remove assigned matches from matches to assign
            foreach (var sa in streamAssignments) matchesToConsider.Remove(sa.Match);

            //Assign remaining matches to remaining stations
            var normalStations = allOpenStations.Where(s => s.Type == StationType.Premium || s.Type == StationType.Standard || s.Type == StationType.Backup).ToArray();
            var normalAssignments = matchesToConsider.Zip(normalStations, (m, s) => new { Match = m, Station = s }).ToArray();

            //Commit assignments to challonge
            foreach (var pair in streamAssignments.Concat(normalAssignments)) pair.Match.AssignPlayersToStation(pair.Station.Name);
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

        public StationType Type { get; private set; }

        public Station(string name, int order) : this(name, order, StationType.Standard) { }

        public Station(string name, int order, StationType type)
        {
            Name = name;
            Order = order;
            Type = type;
            Status = StationStatus.Open;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    enum StationStatus
    {
        Open,
        InUse
    }

    enum StationType
    {
        Stream,
        Recording,
        Premium,
        Standard,
        Backup,
        NoAssign
    }
}
