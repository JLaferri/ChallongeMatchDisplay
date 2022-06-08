using System;
using System.ComponentModel;
using System.Reflection;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Libraries.ChallongeApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class ChallongeObservableParticipant : IObservableParticipant, INotifyPropertyChanged
{
	private static PropertyInfo[] participantProperties = typeof(ChallongeParticipant).GetProperties();

	private ChallongeParticipant source;

	public int Id => source.Id;

	public string Name => source.NameOrUsername;

	public int Seed => source.Seed;

	public string Misc => source.Misc;

	public ChallongeTournamentContext OwningContext { get; private set; }

	public Dirtyable<ParticipantMiscProperties> MiscProperties { get; private set; }

	public bool IsMissing
	{
		get
		{
			return UtcTimeMissing.HasValue;
		}
		set
		{
			SetMissing(value);
		}
	}

	public string OverlayName
	{
		get
		{
			string text = Name;
			if (text.LastIndexOf("(") > 0 && text.LastIndexOf(")") > text.LastIndexOf("("))
			{
				text = text.Substring(0, text.LastIndexOf("(")).Trim();
			}
			return text;
		}
	}

	public DateTime? UtcTimeMissing => MiscProperties.Value.UtcTimeMissing;

	public TimeSpan? TimeSinceMissing
	{
		get
		{
			if (!UtcTimeMissing.HasValue)
			{
				return null;
			}
			return DateTime.UtcNow - UtcTimeMissing.Value;
		}
	}

	public DateTime? UtcTimeMatchAssigned => MiscProperties.Value.UtcTimeMatchAssigned;

	public TimeSpan? TimeSinceAssigned
	{
		get
		{
			if (!UtcTimeMatchAssigned.HasValue)
			{
				return null;
			}
			return DateTime.UtcNow - UtcTimeMatchAssigned.Value;
		}
	}

	public string StationAssignment => MiscProperties.Value.StationAssignment;

	public bool IsAssignedToStation => UtcTimeMatchAssigned.HasValue;

	public event PropertyChangedEventHandler PropertyChanged;

	private void miscPropertySetter<T>(string property, T newValue, T currentValue, Action<T> setProperty)
	{
		if (!object.Equals(newValue, currentValue))
		{
			setProperty(newValue);
			this.Raise(property, this.PropertyChanged);
		}
	}

	public ChallongeObservableParticipant(ChallongeParticipant participant, ChallongeTournamentContext context)
	{
		source = participant;
		MiscProperties = new Dirtyable<ParticipantMiscProperties>(ParticipantMiscProperties.Parse(Misc));
		OwningContext = context;
		DateTime? startedAt = ((ChallongeObservableTournament)context.Tournament).StartedAt;
		if (UtcTimeMissing.HasValue && startedAt.HasValue && UtcTimeMissing.Value.ToLocalTime() < startedAt.Value)
		{
			SetMissing(isMissing: false);
		}
		if (UtcTimeMatchAssigned.HasValue && startedAt.HasValue && UtcTimeMatchAssigned.Value.ToLocalTime() < startedAt.Value)
		{
			ClearStationAssignment();
		}
		PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
			case "Misc":
			{
				ParticipantMiscProperties value = MiscProperties.Value;
				MiscProperties.SuggestValue(ParticipantMiscProperties.Parse(Misc));
				ParticipantMiscProperties value2 = MiscProperties.Value;
				if (!object.Equals(value.UtcTimeMissing, value2.UtcTimeMissing))
				{
					this.Raise("UtcTimeMissing", this.PropertyChanged);
				}
				if (!object.Equals(value.UtcTimeMatchAssigned, value2.UtcTimeMatchAssigned))
				{
					this.Raise("UtcTimeMatchAssigned", this.PropertyChanged);
				}
				if (!object.Equals(value.StationAssignment, value2.StationAssignment))
				{
					this.Raise("StationAssignment", this.PropertyChanged);
				}
				break;
			}
			case "UtcTimeMissing":
				this.Raise("IsMissing", this.PropertyChanged);
				this.Raise("TimeSinceMissing", this.PropertyChanged);
				break;
			case "UtcTimeMatchAssigned":
				this.Raise("TimeSinceAssigned", this.PropertyChanged);
				this.Raise("IsAssignedToStation", this.PropertyChanged);
				break;
			}
		};
	}

	public void SetMissing(bool isMissing)
	{
		DateTime? utcTimeMissing = (isMissing ? new DateTime?(DateTime.UtcNow) : null);
		MiscProperties.Value = new ParticipantMiscProperties(utcTimeMissing, MiscProperties.Value.UtcTimeMatchAssigned, MiscProperties.Value.StationAssignment);
		this.Raise("UtcTimeMissing", this.PropertyChanged);
	}

	public void AssignStation(string stationName)
	{
		if (string.IsNullOrWhiteSpace(stationName))
		{
			throw new ArgumentException("Station name must contain characters.");
		}
		ChallongeStations.Instance.AttemptFreeStation(MiscProperties.Value.StationAssignment);
		MiscProperties.Value = new ParticipantMiscProperties(MiscProperties.Value.UtcTimeMissing, DateTime.UtcNow, stationName);
		ChallongeStations.Instance.AttemptClaimStation(stationName);
		this.Raise("UtcTimeMatchAssigned", this.PropertyChanged);
		this.Raise("StationAssignment", this.PropertyChanged);
	}

	public void ClearStationAssignment()
	{
		ChallongeStations.Instance.AttemptFreeStation(MiscProperties.Value.StationAssignment);
		MiscProperties.Value = new ParticipantMiscProperties(MiscProperties.Value.UtcTimeMissing, null, null);
		this.Raise("UtcTimeMatchAssigned", this.PropertyChanged);
		this.Raise("StationAssignment", this.PropertyChanged);
	}

	public void Update(ChallongeParticipant newData)
	{
		ChallongeParticipant obj = source;
		source = newData;
		PropertyInfo[] array = participantProperties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!object.Equals(propertyInfo.GetValue(obj, null), propertyInfo.GetValue(newData, null)))
			{
				this.Raise(propertyInfo.Name, this.PropertyChanged);
			}
		}
		if (UtcTimeMissing.HasValue)
		{
			this.Raise("TimeSinceMissing", this.PropertyChanged);
		}
		if (UtcTimeMatchAssigned.HasValue)
		{
			this.Raise("TimeSinceAssigned", this.PropertyChanged);
		}
	}
}
