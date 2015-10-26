using System;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.View;
using Fizzi.Applications.ChallongeVisualization.ViewModel;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    class Stations
	{
		private SynchronizationContext _uiContext = SynchronizationContext.Current;
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

        public Dictionary<string, Station> Dict { get; set; }

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

		public void MoveDown(string name)
		{
			int newOrder = Dict[name].Order + 1;

			if (newOrder < Dict.Count)
			{ 
				foreach (KeyValuePair<string, Station> entry in Dict)
				{
					Station station = entry.Value;
					if (station.Order == newOrder)
					{
						station.Order = Dict[name].Order;
						break;
					}
				}

				Dict[name].Order++;
			}
        }

		public void MoveUp(string name)
		{
			int newOrder = Dict[name].Order - 1;

			if (newOrder >= 0)
			{
				foreach (KeyValuePair<string, Station> entry in Dict)
				{
					Station station = entry.Value;
					if (station.Order == newOrder)
					{
						station.Order = Dict[name].Order;
						break;
					}
				}

				Dict[name].Order--;
			}
		}

		public void Add(string name, string type)
		{
			if (name.Trim() != "")
			{
				if (!Dict.ContainsKey(name))
				{
					Station station = new Station(name, Dict.Count + 1);
					station.SetType(type);
					Dict.Add(name, station);
					Save();
				}
			}
        }

		public void Delete(string name)
		{
			if (Dict.ContainsKey(name))
			{
				Dict.Remove(name);
				Save();
			}
        }

		public void Save()
		{
			List<Station> stations = new List<Station>();
            Properties.Settings.Default.stationNames = new System.Collections.Specialized.StringCollection();
			Properties.Settings.Default.stationTypes = new System.Collections.Specialized.StringCollection();

			foreach (KeyValuePair<string, Station> entry in Dict)
			{
				stations.Add(entry.Value);
			}

			stations.Sort((a, b) => a.Order.CompareTo(b.Order));

			foreach (Station station in stations)
			{
				Properties.Settings.Default.stationNames.Add(station.Name);
				Properties.Settings.Default.stationTypes.Add(station.Type.ToString());
			}

			Properties.Settings.Default.Save();
        }

		public void NewAssignment(string stationName, ObservableParticipant player1, ObservableParticipant player2, ObservableMatch match)
		{
			_uiContext.Post(new SendOrPostCallback(new Action<object>(o => {
				if (Application.Current.Windows.OfType<OrganizerWindow>().Count() > 0)
				{
					var view = Application.Current.Windows.OfType<OrganizerWindow>().First() as OrganizerWindow;

					Station station;
					if (Stations.Instance.Dict.TryGetValue(stationName, out station))
					{
						if (station.isPrimaryStream())
						{
							if (view.playersSwapped)
								view.swapPlayers();

							view.p1Score.Text = "0";
							view.p2Score.Text = "0";
							view.p1Name.Text = player1.OverlayName;
							view.p2Name.Text = player2.OverlayName;
							view.round.Text = match.RoundNamePreferred;

							if (match.isWinnersGrandFinal)
							{
								if (match.Player1IsPrereqMatchLoser)
								{
									view.p1Name.Text += " (L)";
									view.p2Name.Text += " (W)";
								}
								else
								{
									view.p1Name.Text += " (W)";
									view.p2Name.Text += " (L)";
								}
                            }
						}
					}
				}
			})), null);
        }
    }

    class Station : INotifyPropertyChanged
	{
		public string Name { get; set; }
        public int Order { get; set; }

        private StationStatus _status;
        public StationStatus Status { get { return _status; } 
            set 
            { 
                this.RaiseAndSetIfChanged("Status", ref _status, value, PropertyChanged); 
            } 
        }

		public void SetType(string stationText)
		{
			stationText = stationText.ToLower().Trim();
			StationType type = StationType.Standard;

			if (stationText == "stream") type = StationType.Stream;
			else if (stationText == "recording") type = StationType.Recording;
			else if (stationText == "premium") type = StationType.Premium;
			else if (stationText == "backup") type = StationType.Backup;
			else if (stationText == "noassign") type = StationType.NoAssign;

			this.Type = type;
		}

        public StationType Type { get; set; }

        public Station(string name, int order) : this(name, order, StationType.Standard) { }

        public Station(string name, int order, StationType type)
        {
            Name = name;
            Order = order;
            Type = type;
            Status = StationStatus.Open;
        }

		public bool isPrimaryStream()
		{
			if (this.Type == StationType.Stream)
			{
				foreach (KeyValuePair<string, Station> entry in Stations.Instance.Dict)
				{
					Station station = entry.Value;
					if (station.Type == StationType.Stream)
					{
						if (entry.Key == this.Name)
							return true;

						return false;
					}
				}
			}

			return false;
		}

        public event PropertyChangedEventHandler PropertyChanged;
    }

    enum StationStatus
    {
        Open,
        InUse
    }

    public enum StationType
    {
        Stream,
        Recording,
        Premium,
        Standard,
        Backup,
        NoAssign
    }
}
