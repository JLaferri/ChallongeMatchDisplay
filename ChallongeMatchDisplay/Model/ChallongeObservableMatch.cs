using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.Properties;
using Fizzi.Libraries.ChallongeApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class ChallongeObservableMatch : INotifyPropertyChanged
{
	private static PropertyInfo[] matchProperties = typeof(ChallongeMatch).GetProperties();

	private ChallongeMatch source;

	private bool _isMatchInProgress;

	private string _stationAssignment;

	private Queue<ChallongeObservableParticipant> player1Queue = new Queue<ChallongeObservableParticipant>();

	public int Id => source.Id;

	public int? Player1Id => source.Player1Id;

	public int? Player2Id => source.Player2Id;

	public int? WinnerId => source.WinnerId;

	public int? LoserId => source.LoserId;

	public bool Player1IsPrereqMatchLoser => source.Player1IsPrereqMatchLoser;

	public int? Player1PrereqMatchId => source.Player1PrereqMatchId;

	public bool Player2IsPrereqMatchLoser => source.Player2IsPrereqMatchLoser;

	public int? Player2PrereqMatchId => source.Player2PrereqMatchId;

	public string State => source.State;

	public string Identifier => source.Identifier;

	public DateTime? StartedAt => source.StartedAt;

	public ChallongeTournamentContext OwningContext { get; private set; }

	public ChallongeObservableParticipant Player1
	{
		get
		{
			if (!Player1Id.HasValue || !((ChallongeObservableTournament)OwningContext.Tournament).Participants.ContainsKey(Player1Id.Value))
			{
				return null;
			}
			return ((ChallongeObservableTournament)OwningContext.Tournament).Participants[Player1Id.Value];
		}
	}

	public ChallongeObservableParticipant Player2
	{
		get
		{
			if (!Player2Id.HasValue || !((ChallongeObservableTournament)OwningContext.Tournament).Participants.ContainsKey(Player2Id.Value))
			{
				return null;
			}
			return ((ChallongeObservableTournament)OwningContext.Tournament).Participants[Player2Id.Value];
		}
	}

	public ChallongeObservableParticipant Winner
	{
		get
		{
			if (!WinnerId.HasValue || !((ChallongeObservableTournament)OwningContext.Tournament).Participants.ContainsKey(WinnerId.Value))
			{
				return null;
			}
			return ((ChallongeObservableTournament)OwningContext.Tournament).Participants[WinnerId.Value];
		}
	}

	public ChallongeObservableParticipant Loser
	{
		get
		{
			if (!LoserId.HasValue || !((ChallongeObservableTournament)OwningContext.Tournament).Participants.ContainsKey(LoserId.Value))
			{
				return null;
			}
			return ((ChallongeObservableTournament)OwningContext.Tournament).Participants[LoserId.Value];
		}
	}

	public ChallongeObservableMatch Player1PreviousMatch
	{
		get
		{
			if (!Player1PrereqMatchId.HasValue)
			{
				return null;
			}
			return (ChallongeObservableMatch)((ChallongeObservableTournament)OwningContext.Tournament).Matches[Player1PrereqMatchId.Value];
		}
	}

	public ChallongeObservableMatch Player2PreviousMatch
	{
		get
		{
			if (!Player2PrereqMatchId.HasValue)
			{
				return null;
			}
			return (ChallongeObservableMatch)((ChallongeObservableTournament)OwningContext.Tournament).Matches[Player2PrereqMatchId.Value];
		}
	}

	public int PlayerCount => new bool[2] { Player1Id.HasValue, Player2Id.HasValue }.Where((bool b) => b).Count();

	public int Round { get; private set; }

	public bool isWinnersGrandFinal
	{
		get
		{
			if (Round == ((ChallongeObservableTournament)OwningContext.Tournament).MaxRoundNumber && !Player1IsPrereqMatchLoser && !Player2IsPrereqMatchLoser)
			{
				return true;
			}
			return false;
		}
	}

	public string RoundNamePreferred => Settings.Default.roundDisplayType switch
	{
		1 => RoundNameFull, 
		2 => RoundName, 
		_ => RoundNameCondensed, 
	};

	public string RoundNameFull
	{
		get
		{
			ChallongeObservableTournament challongeObservableTournament = (ChallongeObservableTournament)OwningContext.Tournament;
			string tournamentType = challongeObservableTournament.TournamentType;
			if (tournamentType != null && tournamentType == "double elimination")
			{
				if (Round < 0)
				{
					if (Round == challongeObservableTournament.MinRoundNumber)
					{
						return "Losers Finals";
					}
					if (Round == challongeObservableTournament.MinRoundNumber + 1)
					{
						return "Losers Semi Finals";
					}
					if (Round == challongeObservableTournament.MinRoundNumber + 2)
					{
						return "Losers Quarter Finals";
					}
					return "Losers Round " + Math.Abs(Round);
				}
				if (Round == challongeObservableTournament.MaxRoundNumber)
				{
					if (isWinnersGrandFinal)
					{
						return "Winners Grand Finals";
					}
					return "Losers Grand Finals";
				}
				if (Round == challongeObservableTournament.MaxRoundNumber - 1)
				{
					return "Winners Finals";
				}
				if (Round == challongeObservableTournament.MaxRoundNumber - 2)
				{
					return "Winners Semi Finals";
				}
				if (Round == challongeObservableTournament.MaxRoundNumber - 3)
				{
					return "Winners Quarter Finals";
				}
				return "Winners Round " + Round;
			}
			return Round.ToString();
		}
	}

	public string RoundNameCondensed
	{
		get
		{
			ChallongeObservableTournament challongeObservableTournament = (ChallongeObservableTournament)OwningContext.Tournament;
			string tournamentType = challongeObservableTournament.TournamentType;
			if (tournamentType != null && tournamentType == "double elimination")
			{
				if (Round < 0)
				{
					if (Round == challongeObservableTournament.MinRoundNumber)
					{
						return "Losers Finals";
					}
					if (Round == challongeObservableTournament.MinRoundNumber + 1)
					{
						return "L Semi Finals";
					}
					if (Round == challongeObservableTournament.MinRoundNumber + 2)
					{
						return "L Quarter Finals";
					}
					return "L Round " + Math.Abs(Round);
				}
				if (Round == challongeObservableTournament.MaxRoundNumber)
				{
					if (isWinnersGrandFinal)
					{
						return "W Grand Finals";
					}
					return "L Grand Finals";
				}
				if (Round == challongeObservableTournament.MaxRoundNumber - 1)
				{
					return "W Finals";
				}
				if (Round == challongeObservableTournament.MaxRoundNumber - 2)
				{
					return "W Semi Finals";
				}
				if (Round == challongeObservableTournament.MaxRoundNumber - 3)
				{
					return "W Quarter Finals";
				}
				return "W Round " + Round;
			}
			return Round.ToString();
		}
	}

	public string RoundName
	{
		get
		{
			ChallongeObservableTournament challongeObservableTournament = (ChallongeObservableTournament)OwningContext.Tournament;
			string tournamentType = challongeObservableTournament.TournamentType;
			if (tournamentType != null && tournamentType == "double elimination")
			{
				if (Round < 0)
				{
					if (Round == challongeObservableTournament.MinRoundNumber)
					{
						return "LF";
					}
					if (Round == challongeObservableTournament.MinRoundNumber + 1)
					{
						return "LSF";
					}
					if (Round == challongeObservableTournament.MinRoundNumber + 2)
					{
						return "LQF";
					}
					return "L" + Math.Abs(Round);
				}
				if (Round == challongeObservableTournament.MaxRoundNumber)
				{
					return "GF";
				}
				if (Round == challongeObservableTournament.MaxRoundNumber - 1)
				{
					return "WF";
				}
				if (Round == challongeObservableTournament.MaxRoundNumber - 2)
				{
					return "WSF";
				}
				if (Round == challongeObservableTournament.MaxRoundNumber - 3)
				{
					return "WQF";
				}
				return "W" + Round;
			}
			return Round.ToString();
		}
	}

	public int RoundOrder
	{
		get
		{
			string tournamentType = ((ChallongeObservableTournament)OwningContext.Tournament).TournamentType;
			if (tournamentType != null && tournamentType == "double elimination")
			{
				if (Round >= 0)
				{
					return Round;
				}
				return Math.Abs(Round) / 2 + 1;
			}
			return Round;
		}
	}

	public bool IsWinners => Round < 0;

	public string Player1SourceString
	{
		get
		{
			if (Player1PreviousMatch == null)
			{
				return "N/A";
			}
			string identifier = Player1PreviousMatch.Identifier;
			if (Player1IsPrereqMatchLoser)
			{
				return "Loser of " + identifier;
			}
			return "Winner of " + identifier;
		}
	}

	public string Player2SourceString
	{
		get
		{
			if (Player2PreviousMatch == null)
			{
				return "N/A";
			}
			string identifier = Player2PreviousMatch.Identifier;
			if (Player2IsPrereqMatchLoser)
			{
				return "Loser of " + identifier;
			}
			return "Winner of " + identifier;
		}
	}

	public TimeSpan? TimeSinceAvailable
	{
		get
		{
			if (!StartedAt.HasValue)
			{
				return null;
			}
			return DateTime.Now - StartedAt.Value;
		}
	}

	public bool IsMatchInProgress
	{
		get
		{
			return _isMatchInProgress;
		}
		set
		{
			this.RaiseAndSetIfChanged("IsMatchInProgress", ref _isMatchInProgress, value, this.PropertyChanged);
		}
	}

	public string StationAssignment
	{
		get
		{
			return _stationAssignment;
		}
		set
		{
			this.RaiseAndSetIfChanged("StationAssignment", ref _stationAssignment, value, this.PropertyChanged);
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public ChallongeObservableMatch(ChallongeMatch match, ChallongeTournamentContext context)
	{
		source = match;
		OwningContext = context;
		int participantsCount = ((ChallongeTournament)context.Tournament).ParticipantsCount;
		double y = Math.Floor(Math.Log(participantsCount, 2.0));
		double num = Math.Pow(2.0, y);
		if (match.Round < 0 && (double)participantsCount > num && (double)participantsCount <= num + num / 2.0)
		{
			Round = match.Round - 1;
		}
		else
		{
			Round = match.Round;
		}
		string text = ((Player1 != null) ? Player1.StationAssignment : null);
		string text2 = ((Player2 != null) ? Player2.StationAssignment : null);
		if (State != "complete" && text != text2)
		{
			ClearStationAssignment();
		}
		PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
			case "Player1Id":
				this.Raise("Player1", this.PropertyChanged);
				this.Raise("PlayerCount", this.PropertyChanged);
				if (Player1 != null)
				{
					Player1.IsMissing = false;
				}
				break;
			case "Player2Id":
				this.Raise("Player2", this.PropertyChanged);
				this.Raise("PlayerCount", this.PropertyChanged);
				if (Player2 != null)
				{
					Player2.IsMissing = false;
				}
				break;
			case "Player1PrereqMatchId":
				this.Raise("Player1PreviousMatch", this.PropertyChanged);
				break;
			case "Player2PrereqMatchId":
				this.Raise("Player2PreviousMatch", this.PropertyChanged);
				break;
			case "StartedAt":
				this.Raise("TimeSinceAvailable", this.PropertyChanged);
				break;
			case "State":
				if (Player1 != null)
				{
					Player1.ClearStationAssignment();
				}
				if (Player2 != null)
				{
					Player2.ClearStationAssignment();
				}
				if (State == "open")
				{
					switch (GlobalSettings.Instance.SelectedNewMatchAction)
					{
					case NewMatchAction.AutoAssign:
					{
						ChallongeStationModel bestNormalStation = ChallongeStations.Instance.GetBestNormalStation();
						if (bestNormalStation != null)
						{
							AssignPlayersToStation(bestNormalStation.Name);
						}
						break;
					}
					case NewMatchAction.Anywhere:
						AssignPlayersToStation("Any");
						break;
					}
				}
				break;
			}
		};
		(from a in Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(delegate(PropertyChangedEventHandler h)
			{
				PropertyChanged += h;
			}, delegate(PropertyChangedEventHandler h)
			{
				PropertyChanged -= h;
			})
			where a.EventArgs.PropertyName == "Player1"
			select a into _
			select (Player1 != null) ? (from a in Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(delegate(PropertyChangedEventHandler h)
				{
					player1Queue.Enqueue(Player1);
					Player1.PropertyChanged += h;
				}, delegate(PropertyChangedEventHandler h)
				{
					player1Queue.Dequeue().PropertyChanged -= h;
				})
				where a.EventArgs.PropertyName == "IsAssignedToStation" || a.EventArgs.PropertyName == "StationAssignment"
				select a into _2
				select EventArgs.Empty).StartWith(EventArgs.Empty) : Observable.Return(EventArgs.Empty)).Switch().Subscribe(delegate
		{
			IsMatchInProgress = Player1 != null && Player1.IsAssignedToStation;
			StationAssignment = ((Player1 == null) ? null : Player1.StationAssignment);
		});
		this.Raise("Player1", this.PropertyChanged);
	}

	public void AssignPlayersToStation(string stationName)
	{
		if (PlayerCount == 2)
		{
			Player1.AssignStation(stationName);
			Player2.AssignStation(stationName);
			ChallongeStations.Instance.NewAssignment(stationName, Player1, Player2, this);
		}
	}

	public void ClearStationAssignment()
	{
		if (Player1 != null)
		{
			Player1.ClearStationAssignment();
		}
		if (Player2 != null)
		{
			Player2.ClearStationAssignment();
		}
	}

	public void ReportPlayer1Victory(params SetScore[] setCounts)
	{
		if (Player1Id.HasValue)
		{
			ChallongeTournamentContext owningContext = OwningContext;
			owningContext.Portal.ReportMatchWinner(((ChallongeObservableTournament)owningContext.Tournament).Id, Id, Player1Id.Value, setCounts);
			owningContext.Refresh();
		}
	}

	public void ReportPlayer2Victory(params SetScore[] setCounts)
	{
		if (Player2Id.HasValue)
		{
			ChallongeTournamentContext owningContext = OwningContext;
			owningContext.Portal.ReportMatchWinner(((ChallongeObservableTournament)owningContext.Tournament).Id, Id, Player2Id.Value, setCounts);
			owningContext.Refresh();
		}
	}

	public void Update(ChallongeMatch newData)
	{
		ChallongeMatch obj = source;
		source = newData;
		PropertyInfo[] array = matchProperties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!object.Equals(propertyInfo.GetValue(obj, null), propertyInfo.GetValue(newData, null)))
			{
				this.Raise(propertyInfo.Name, this.PropertyChanged);
			}
		}
		if (StartedAt.HasValue)
		{
			this.Raise("TimeSinceAvailable", this.PropertyChanged);
		}
	}
}
