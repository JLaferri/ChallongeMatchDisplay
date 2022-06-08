using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Libraries.SmashggApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class SmashggObservablePhaseGroup : IObservableTournament, INotifyPropertyChanged
{
	private static PropertyInfo[] phaseGroupProperties = typeof(SmashggPhaseGroup).GetProperties();

	private SmashggPhaseGroup source;

	private SmashggPhase sourcePhase;

	private SmashggTournament sourceTournament;

	private Dictionary<string, IObservableMatch> _matches;

	public string Name => sourceTournament.Name + " " + sourcePhase.Name + " (G" + source.DisplayIdentifier + ")";

	public long Id => source.Id;

	public SmashggPhaseGroupState PhaseGroupState => source.State;

	public Dictionary<string, IObservableMatch> Matches
	{
		get
		{
			return _matches;
		}
		set
		{
			this.RaiseAndSetIfChanged("Matches", ref _matches, value, this.PropertyChanged);
		}
	}

	public SmashggEventPhaseGroupContext OwningContext { get; private set; }

	public SmashggBracketType BracketType => source.BracketType;

	public event PropertyChangedEventHandler PropertyChanged;

	public SmashggObservablePhaseGroup(SmashggPhaseGroup phaseGroup, SmashggPhase phase, SmashggTournament tournament, SmashggEventPhaseGroupContext context)
	{
		source = phaseGroup;
		sourcePhase = phase;
		sourceTournament = tournament;
		OwningContext = context;
	}

	public void Initialize(IEnumerable<SmashggMatch> matchList)
	{
		Matches = matchList.ToDictionary((Func<SmashggMatch, string>)((SmashggMatch m) => m.Id), (Func<SmashggMatch, IObservableMatch>)((SmashggMatch m) => new SmashggObservableMatch(m, OwningContext)));
	}

	public void Update(SmashggPhaseGroup newData, SmashggPhase newPData, SmashggTournament newTData, IEnumerable<SmashggMatch> matchList)
	{
		SmashggPhaseGroup smashggPhaseGroup = source;
		source = newData;
		SmashggPhase smashggPhase = sourcePhase;
		sourcePhase = newPData;
		SmashggTournament smashggTournament = sourceTournament;
		sourceTournament = newTData;
		if (smashggPhase.Name != newPData.Name || smashggTournament.Name != newTData.Name)
		{
			this.Raise("Name", this.PropertyChanged);
		}
		if (smashggPhaseGroup.State != newData.State)
		{
			SmashggStations.Instance.CompletionChange(newData.State == SmashggPhaseGroupState.COMPLETED, TopTwoParticipants());
		}
		PropertyInfo[] array = phaseGroupProperties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!object.Equals(propertyInfo.GetValue(smashggPhaseGroup, null), propertyInfo.GetValue(newData, null)))
			{
				this.Raise(propertyInfo.Name, this.PropertyChanged);
			}
		}
		if (Matches.Count != matchList.Count())
		{
			Initialize(matchList);
			return;
		}
		foreach (SmashggMatch match in matchList)
		{
			((SmashggObservableMatch)Matches[match.Id]).Update(match);
		}
	}

	public List<SmashggObservableEntrant> TopTwoParticipants()
	{
		List<SmashggObservableEntrant> list = new List<SmashggObservableEntrant>();
		if (PhaseGroupState == SmashggPhaseGroupState.COMPLETED)
		{
			int index = Matches.Count - 1;
			List<IObservableMatch> list2 = Matches.Values.ToList();
			list2.Sort((IObservableMatch a, IObservableMatch b) => ((SmashggObservableMatch)a).Round.CompareTo(((SmashggObservableMatch)b).Round));
			list.Add((SmashggObservableEntrant)list2.ElementAt(index).Winner);
			list.Add((SmashggObservableEntrant)list2.ElementAt(index).Loser);
		}
		return list;
	}
}
