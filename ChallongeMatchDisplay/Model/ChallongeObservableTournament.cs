using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Libraries.ChallongeApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class ChallongeObservableTournament : IObservableTournament, INotifyPropertyChanged
{
	private static PropertyInfo[] tournamentProperties = typeof(ChallongeTournament).GetProperties();

	private ChallongeTournament source;

	private Dictionary<int, ChallongeObservableParticipant> _participants;

	private Dictionary<int, IObservableMatch> _matches;

	public DateTime? CreatedAt => source.CreatedAt;

	public DateTime? StartedAt => source.StartedAt;

	public DateTime? CompletedAt => source.CompletedAt;

	public string Name => source.Name;

	public string Description => source.Description;

	public int Id => source.Id;

	public int ParticipantsCount => source.ParticipantsCount;

	public int ProgressMeter => source.ProgressMeter;

	public string State => source.State;

	public string TournamentType => source.TournamentType;

	public string Url => source.Url;

	public string FullChallongeUrl => source.FullChallongeUrl;

	public string LiveImageUrl => source.LiveImageUrl;

	public Dictionary<int, ChallongeObservableParticipant> Participants
	{
		get
		{
			return _participants;
		}
		set
		{
			this.RaiseAndSetIfChanged("Participants", ref _participants, value, this.PropertyChanged);
		}
	}

	public Dictionary<int, IObservableMatch> Matches
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

	public ChallongeTournamentContext OwningContext { get; private set; }

	public int? MaxRoundNumber { get; private set; }

	public int? MinRoundNumber { get; private set; }

	public event PropertyChangedEventHandler PropertyChanged;

	public ChallongeObservableTournament(ChallongeTournament tournament, ChallongeTournamentContext context)
	{
		source = tournament;
		OwningContext = context;
	}

	public void Initialize(IEnumerable<ChallongeParticipant> playerList, IEnumerable<ChallongeMatch> matchList)
	{
		Participants = playerList.ToDictionary((ChallongeParticipant p) => p.Id, (ChallongeParticipant p) => new ChallongeObservableParticipant(p, OwningContext));
		Matches = matchList.ToDictionary((ChallongeMatch m) => m.Id, (ChallongeMatch m) => (IObservableMatch)new ChallongeObservableMatch(m, OwningContext));
		MaxRoundNumber = ((IEnumerable<KeyValuePair<int, IObservableMatch>>)Matches).Select((Func<KeyValuePair<int, IObservableMatch>, int?>)((KeyValuePair<int, IObservableMatch> m) => m.Value.Round)).DefaultIfEmpty().Max();
		MinRoundNumber = ((IEnumerable<KeyValuePair<int, IObservableMatch>>)Matches).Select((Func<KeyValuePair<int, IObservableMatch>, int?>)((KeyValuePair<int, IObservableMatch> m) => m.Value.Round)).DefaultIfEmpty().Min();
	}

	public void Update(ChallongeTournament newData, IEnumerable<ChallongeParticipant> playerList, IEnumerable<ChallongeMatch> matchList)
	{
		ChallongeTournament challongeTournament = source;
		source = newData;
		PropertyInfo[] array = tournamentProperties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!object.Equals(propertyInfo.GetValue(challongeTournament, null), propertyInfo.GetValue(newData, null)))
			{
				this.Raise(propertyInfo.Name, this.PropertyChanged);
			}
		}
		if (challongeTournament.ProgressMeter != newData.ProgressMeter)
		{
			ChallongeStations.Instance.ProgressChange(newData.ProgressMeter);
		}
		if (challongeTournament.CompletedAt != newData.CompletedAt)
		{
			ChallongeStations.Instance.CompletionChange(newData.CompletedAt.HasValue, TopFourParticipants());
		}
		IEnumerable<int> second = Participants.Select((KeyValuePair<int, ChallongeObservableParticipant> kvp) => kvp.Key).Intersect(playerList.Select((ChallongeParticipant p) => p.Id));
		bool num = Participants.Select((KeyValuePair<int, ChallongeObservableParticipant> kvp) => kvp.Key).Union(playerList.Select((ChallongeParticipant p) => p.Id)).Except(second)
			.Any();
		bool flag = Matches.Count != matchList.Count();
		if (num || flag)
		{
			Initialize(playerList, matchList);
			return;
		}
		foreach (ChallongeParticipant player in playerList)
		{
			Participants[player.Id].Update(player);
		}
		foreach (ChallongeMatch match in matchList)
		{
			((ChallongeObservableMatch)Matches[match.Id]).Update(match);
		}
	}

	public List<ChallongeObservableParticipant> TopFourParticipants()
	{
		List<ChallongeObservableParticipant> list = new List<ChallongeObservableParticipant>();
		if (Matches.Count > 3 && CompletedAt.HasValue)
		{
			int num = Matches.Count - 1;
			list.Add((ChallongeObservableParticipant)Matches.ElementAt(num).Value.Winner);
			list.Add((ChallongeObservableParticipant)Matches.ElementAt(num).Value.Loser);
			if (!((ChallongeObservableMatch)Matches.ElementAt(num).Value).isWinnersGrandFinal)
			{
				num--;
			}
			list.Add((ChallongeObservableParticipant)Matches.ElementAt(num - 1).Value.Loser);
			list.Add((ChallongeObservableParticipant)Matches.ElementAt(num - 2).Value.Loser);
		}
		return list;
	}

	public void EndTournament()
	{
		OwningContext.EndTournament();
	}
}
