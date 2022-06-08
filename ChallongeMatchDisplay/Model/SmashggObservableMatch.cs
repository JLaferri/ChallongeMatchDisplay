using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.Properties;
using Fizzi.Libraries.SmashggApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class SmashggObservableMatch : INotifyPropertyChanged, IObservableMatch
{
	private static PropertyInfo[] matchProperties = typeof(SmashggMatch).GetProperties();

	private SmashggMatch source;

	private Queue<SmashggObservableEntrant> player1Queue = new Queue<SmashggObservableEntrant>();

	public string Id => source.Id;

	public int? WinnerId => source.WinnerId;

	public SmashggMatchState State => source.State;

	public string Identifier => source.Identifier;

	public DateTime? StartedAt => source.StartedAt;

	public DateTime? CreatedAt => source.CreatedAt;

	public SmashggStation Station => source.Station;

	public SmashggEventPhaseGroupContext OwningContext { get; private set; }

	public string StationAssignment
	{
		get
		{
			if (source.Station != null)
			{
				return source.Station.Number.ToString();
			}
			return "";
		}
	}

	public SmashggObservableEntrant Entrant1
	{
		get
		{
			if (source.Slots[0].Entrant == null)
			{
				return null;
			}
			return new SmashggObservableEntrant(source.Slots[0].Entrant, OwningContext);
		}
	}

	public SmashggObservableEntrant Entrant2
	{
		get
		{
			if (source.Slots[1].Entrant == null)
			{
				return null;
			}
			return new SmashggObservableEntrant(source.Slots[1].Entrant, OwningContext);
		}
	}

	public IObservableParticipant Winner
	{
		get
		{
			if (!WinnerId.HasValue)
			{
				return null;
			}
			if (Entrant1.Id != WinnerId.Value)
			{
				return Entrant2;
			}
			return Entrant1;
		}
	}

	public IObservableParticipant Loser
	{
		get
		{
			if (!LoserId.HasValue)
			{
				return null;
			}
			if (Entrant1.Id != LoserId.Value)
			{
				return Entrant2;
			}
			return Entrant1;
		}
	}

	public bool IsMatchInProgress => source.State == SmashggMatchState.ACTIVE;

	public bool IsMatchComplete => source.State == SmashggMatchState.COMPLETED;

	public int? LoserId
	{
		get
		{
			if (!WinnerId.HasValue)
			{
				return WinnerId;
			}
			return (source.Slots[0].Entrant.Id == WinnerId.Value) ? source.Slots[1].Entrant.Id : source.Slots[0].Entrant.Id;
		}
	}

	public int EntrantCount => new bool[2]
	{
		Entrant1 != null,
		Entrant2 != null
	}.Where((bool b) => b).Count();

	public int Round => source.Round;

	public string RoundNamePreferred => Settings.Default.roundDisplayType switch
	{
		1 => RoundNameFull, 
		2 => RoundName, 
		_ => RoundNameCondensed, 
	};

	public string RoundNameFull => source.FullRoundText;

	public string RoundNameCondensed
	{
		get
		{
			if (((SmashggObservablePhaseGroup)OwningContext.Tournament).BracketType == SmashggBracketType.DOUBLE_ELIMINATION)
			{
				if (Round < 0)
				{
					if (source.FullRoundText == "Losers Final")
					{
						return "Losers Finals";
					}
					if (source.FullRoundText == "Losers Semi-Final")
					{
						return "L Semi Finals";
					}
					if (source.FullRoundText == "Losers Quarter-Final")
					{
						return "L Quarter Finals";
					}
					return "L Round " + Math.Abs(Round);
				}
				if (source.FullRoundText == "Grand Final")
				{
					return "W Grand Finals";
				}
				if (source.FullRoundText == "Grand Final Reset")
				{
					return "L Grand Finals";
				}
				if (source.FullRoundText == "Winners Final")
				{
					return "Winners Finals";
				}
				if (source.FullRoundText == "Winners Semi-Final")
				{
					return "W Semi Finals";
				}
				if (source.FullRoundText == "Winners Quarter-Final")
				{
					return "W Quarter Finals";
				}
				return "W Round " + Round;
			}
			return "Round " + Math.Abs(Round);
		}
	}

	public string RoundName
	{
		get
		{
			if (((SmashggObservablePhaseGroup)OwningContext.Tournament).BracketType == SmashggBracketType.DOUBLE_ELIMINATION)
			{
				if (Round < 0)
				{
					if (source.FullRoundText == "Losers Final")
					{
						return "LF";
					}
					if (source.FullRoundText == "Losers Semi-Final")
					{
						return "LSF";
					}
					if (source.FullRoundText == "Losers Quarter-Final")
					{
						return "LQF";
					}
					return "L" + Math.Abs(Round);
				}
				if (source.FullRoundText == "Grand Final" || source.FullRoundText == "Grand Final Reset")
				{
					return "GF";
				}
				if (source.FullRoundText == "Winners Final")
				{
					return "WF";
				}
				if (source.FullRoundText == "Winners Semi-Final")
				{
					return "WSF";
				}
				if (source.FullRoundText == "Winners Quarter-Final")
				{
					return "WQF";
				}
				return "W" + Math.Abs(Round);
			}
			return "R" + Math.Abs(Round);
		}
	}

	public int RoundOrder
	{
		get
		{
			_ = ((SmashggObservablePhaseGroup)OwningContext.Tournament).BracketType;
			if (Round >= 0)
			{
				return Round;
			}
			return Math.Abs(Round) / 2 + 1;
		}
	}

	public bool IsWinners => Round > 0;

	public string Entrant1SourceString
	{
		get
		{
			if (source.Slots[0].PrereqType != "set")
			{
				return "N/A";
			}
			string text = "";
			if (((SmashggObservablePhaseGroup)OwningContext.Tournament).Matches.TryGetValue(source.Slots[0].PrereqId, out var value))
			{
				text = ((SmashggObservableMatch)value).Identifier;
			}
			if (text.Length == 0)
			{
				return "";
			}
			if (source.Slots[0].PrereqPlacement.Value == 2)
			{
				return "Loser of " + text;
			}
			return "Winner of " + text;
		}
	}

	public string Entrant2SourceString
	{
		get
		{
			if (source.Slots[1].PrereqType != "set")
			{
				return "N/A";
			}
			string text = "";
			if (((SmashggObservablePhaseGroup)OwningContext.Tournament).Matches.TryGetValue(source.Slots[1].PrereqId, out var value))
			{
				text = ((SmashggObservableMatch)value).Identifier;
			}
			if (text.Length == 0)
			{
				return "";
			}
			if (source.Slots[1].PrereqPlacement.Value == 2)
			{
				return "Loser of " + text;
			}
			return "Winner of " + text;
		}
	}

	public TimeSpan? TimeSinceStarted
	{
		get
		{
			if (!StartedAt.HasValue)
			{
				return null;
			}
			return DateTime.Now.ToUniversalTime() - StartedAt.Value;
		}
	}

	public TimeSpan? TimeSinceCreated
	{
		get
		{
			if (!CreatedAt.HasValue)
			{
				return null;
			}
			return DateTime.Now.ToUniversalTime() - CreatedAt.Value;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public SmashggObservableMatch(SmashggMatch match, SmashggEventPhaseGroupContext context)
	{
		source = match;
		OwningContext = context;
		PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
			case "Slots":
				this.Raise("Entrant1", this.PropertyChanged);
				this.Raise("Entrant2", this.PropertyChanged);
				this.Raise("EntrantCount", this.PropertyChanged);
				break;
			case "StartedAt":
				this.Raise("TimeSinceStarted", this.PropertyChanged);
				break;
			case "Station":
				this.Raise("StationAssignment", this.PropertyChanged);
				break;
			case "State":
				this.Raise("IsMatchInProgress", this.PropertyChanged);
				this.Raise("IsMatchComplete", this.PropertyChanged);
				break;
			}
		};
	}

	public void ReportEntrant1Victory(params SetScore[] setCounts)
	{
		if (Entrant1 != null)
		{
			SmashggEventPhaseGroupContext owningContext = OwningContext;
			owningContext.Portal.ReportMatchWinner(((SmashggObservablePhaseGroup)owningContext.Tournament).Id, Id, Entrant1.Id, setCounts);
			owningContext.Refresh();
		}
	}

	public void ReportEntrant2Victory(params SetScore[] setCounts)
	{
		if (Entrant2 != null)
		{
			SmashggEventPhaseGroupContext owningContext = OwningContext;
			owningContext.Portal.ReportMatchWinner(((SmashggObservablePhaseGroup)owningContext.Tournament).Id, Id, Entrant2.Id, setCounts);
			owningContext.Refresh();
		}
	}

	public void Update(SmashggMatch newData)
	{
		SmashggMatch obj = source;
		source = newData;
		PropertyInfo[] array = matchProperties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!object.Equals(propertyInfo.GetValue(obj, null), propertyInfo.GetValue(newData, null)))
			{
				this.Raise(propertyInfo.Name, this.PropertyChanged);
			}
		}
		this.Raise("TimeSinceCreated", this.PropertyChanged);
		this.Raise("TimeSinceStarted", this.PropertyChanged);
	}
}
