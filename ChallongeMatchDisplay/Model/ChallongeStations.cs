using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using Fizzi.Applications.ChallongeVisualization.Properties;
using Fizzi.Applications.ChallongeVisualization.View;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class ChallongeStations
{
	private SynchronizationContext _uiContext = SynchronizationContext.Current;

	private static volatile ChallongeStations instance;

	private static object syncRoot = new object();

	public static ChallongeStations Instance
	{
		get
		{
			if (instance == null)
			{
				lock (syncRoot)
				{
					if (instance == null)
					{
						instance = new ChallongeStations();
					}
				}
			}
			return instance;
		}
	}

	public Dictionary<string, ChallongeStationModel> Dict { get; set; }

	private ChallongeStations()
	{
	}

	public void LoadNew(IEnumerable<ChallongeStationModel> stations)
	{
		Dict = stations.ToDictionary((ChallongeStationModel s) => s.Name, (ChallongeStationModel s) => s);
	}

	public ChallongeStationModel GetBestNormalStation()
	{
		if (Dict == null)
		{
			return null;
		}
		return (from s in Dict.Values
			where s.Status == ChallongeStationStatus.Open && s.Type != 0 && s.Type != ChallongeStationType.Recording && s.Type != ChallongeStationType.NoAssign
			orderby s.Type, s.Order
			select s).FirstOrDefault();
	}

	public void AssignOpenMatchesToStations(ChallongeObservableMatch[] matches)
	{
		if (Dict == null)
		{
			return;
		}
		ChallongeObservableMatch[] array = matches.Where((ChallongeObservableMatch m) => !m.IsMatchInProgress).ToArray();
		ChallongeStationModel[] array2 = (from s in Dict.Values
			where s.Status == ChallongeStationStatus.Open && s.Type != ChallongeStationType.NoAssign
			orderby s.Type, s.Order
			select s).ToArray();
		int num = array2.Length;
		if (num == 0 || array.Length == 0)
		{
			return;
		}
		ChallongeObservableMatch[] source = (from m in array
			orderby m.RoundOrder, m.IsWinners, m.Identifier
			select m).ToArray();
		var lastMatch = source.Select((ChallongeObservableMatch m, int i) => new
		{
			Match = m,
			Index = i
		}).Take(num).Last();
		List<ChallongeObservableMatch> list = source.TakeWhile((ChallongeObservableMatch m, int i) => i <= lastMatch.Index || (m.IsWinners == lastMatch.Match.IsWinners && m.RoundOrder == lastMatch.Match.RoundOrder)).ToList();
		ChallongeStationModel[] array3 = array2.Where((ChallongeStationModel s) => s.Type == ChallongeStationType.Stream || s.Type == ChallongeStationType.Recording).ToArray();
		_ = array3.Length;
		var array4 = (from m in list
			orderby m.Player1.Seed + m.Player2.Seed, new int[2]
			{
				m.Player1.Seed,
				m.Player2.Seed
			}.Min()
			select m).ToArray().Zip(array3, (ChallongeObservableMatch m, ChallongeStationModel s) => new
		{
			Match = m,
			Station = s
		}).ToArray();
		var array5 = array4;
		foreach (var anon in array5)
		{
			list.Remove(anon.Match);
		}
		ChallongeStationModel[] second = array2.Where((ChallongeStationModel s) => s.Type == ChallongeStationType.Premium || s.Type == ChallongeStationType.Standard || s.Type == ChallongeStationType.Backup).ToArray();
		var second2 = list.Zip(second, (ChallongeObservableMatch m, ChallongeStationModel s) => new
		{
			Match = m,
			Station = s
		}).ToArray();
		foreach (var item in array4.Concat(second2))
		{
			item.Match.AssignPlayersToStation(item.Station.Name);
		}
	}

	public void AttemptFreeStation(string stationName)
	{
		if (Dict != null && stationName != null && Dict.TryGetValue(stationName, out var value))
		{
			value.Status = ChallongeStationStatus.Open;
		}
	}

	public void AttemptClaimStation(string stationName)
	{
		if (Dict != null && stationName != null && Dict.TryGetValue(stationName, out var value))
		{
			value.Status = ChallongeStationStatus.InUse;
		}
	}

	public void MoveDown(string name)
	{
		int num = Dict[name].Order + 1;
		if (num >= Dict.Count)
		{
			return;
		}
		foreach (KeyValuePair<string, ChallongeStationModel> item in Dict)
		{
			ChallongeStationModel value = item.Value;
			if (value.Order == num)
			{
				value.Order = Dict[name].Order;
				break;
			}
		}
		Dict[name].Order++;
	}

	public void MoveUp(string name)
	{
		int num = Dict[name].Order - 1;
		if (num < 0)
		{
			return;
		}
		foreach (KeyValuePair<string, ChallongeStationModel> item in Dict)
		{
			ChallongeStationModel value = item.Value;
			if (value.Order == num)
			{
				value.Order = Dict[name].Order;
				break;
			}
		}
		Dict[name].Order--;
	}

	public void Add(string name, string type)
	{
		if (name.Trim() != "" && !Dict.ContainsKey(name))
		{
			ChallongeStationModel challongeStationModel = new ChallongeStationModel(name, Dict.Count + 1);
			challongeStationModel.SetType(type);
			Dict.Add(name, challongeStationModel);
			Save();
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
		List<ChallongeStationModel> list = new List<ChallongeStationModel>();
		Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.stationNames = new StringCollection();
		Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.stationTypes = new StringCollection();
		foreach (KeyValuePair<string, ChallongeStationModel> item in Dict)
		{
			list.Add(item.Value);
		}
		list.Sort((ChallongeStationModel a, ChallongeStationModel b) => a.Order.CompareTo(b.Order));
		foreach (ChallongeStationModel item2 in list)
		{
			Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.stationNames.Add(item2.Name);
			Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.stationTypes.Add(item2.Type.ToString());
		}
		Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.Save();
	}

	public void NewAssignment(string stationName, ChallongeObservableParticipant player1, ChallongeObservableParticipant player2, ChallongeObservableMatch match)
	{
		_uiContext.Post(((Action<object>)delegate
		{
			if (Application.Current.Windows.OfType<ChallongeOrganizerWindow>().Count() > 0)
			{
				ChallongeOrganizerWindow challongeOrganizerWindow = Application.Current.Windows.OfType<ChallongeOrganizerWindow>().First();
				if (Instance.Dict.TryGetValue(stationName, out var value) && value.isPrimaryStream())
				{
					if (challongeOrganizerWindow.playersSwapped)
					{
						challongeOrganizerWindow.swapPlayers();
					}
					challongeOrganizerWindow.p1Score.Text = "0";
					challongeOrganizerWindow.p2Score.Text = "0";
					challongeOrganizerWindow.p1Name.Text = player1.OverlayName;
					challongeOrganizerWindow.p2Name.Text = player2.OverlayName;
					challongeOrganizerWindow.round.Text = match.RoundNamePreferred;
					if (match.isWinnersGrandFinal)
					{
						if (match.Player1IsPrereqMatchLoser)
						{
							challongeOrganizerWindow.p1Name.Text += " (L)";
							challongeOrganizerWindow.p2Name.Text += " (W)";
						}
						else
						{
							challongeOrganizerWindow.p1Name.Text += " (W)";
							challongeOrganizerWindow.p2Name.Text += " (L)";
						}
					}
				}
			}
		}).Invoke, null);
	}

	public void ProgressChange(int newProgress)
	{
		_uiContext.Post(((Action<object>)delegate
		{
			if (Application.Current.Windows.OfType<ChallongeOrganizerWindow>().Count() > 0)
			{
				ChallongeOrganizerWindow challongeOrganizerWindow = Application.Current.Windows.OfType<ChallongeOrganizerWindow>().First();
				if (newProgress == 100)
				{
					challongeOrganizerWindow.endTournament.Visibility = Visibility.Visible;
				}
				else
				{
					challongeOrganizerWindow.endTournament.Visibility = Visibility.Collapsed;
				}
			}
		}).Invoke, null);
	}

	public void CompletionChange(bool complete, List<ChallongeObservableParticipant> top4)
	{
		_uiContext.Post(((Action<object>)delegate
		{
			ChallongeOrganizerWindow challongeOrganizerWindow = null;
			if (Application.Current.Windows.OfType<ChallongeOrganizerWindow>().Count() > 0)
			{
				challongeOrganizerWindow = Application.Current.Windows.OfType<ChallongeOrganizerWindow>().First();
			}
			ChallongeMatchDisplayView challongeMatchDisplayView = (ChallongeMatchDisplayView)Application.Current.Windows.OfType<MainWindow>().First().content.Content;
			if (complete)
			{
				if (challongeOrganizerWindow != null)
				{
					challongeOrganizerWindow.endTournament.Visibility = Visibility.Collapsed;
				}
				if (top4.Count == 4)
				{
					if (challongeOrganizerWindow != null)
					{
						challongeOrganizerWindow.round.Text = top4[0].OverlayName + " Wins!";
						challongeOrganizerWindow.p1Name.Text = "Player One";
						challongeOrganizerWindow.p2Name.Text = "Player Two";
						challongeOrganizerWindow.p1Score.Text = "0";
						challongeOrganizerWindow.p2Score.Text = "0";
					}
					challongeMatchDisplayView.Winners.Visibility = Visibility.Visible;
					challongeMatchDisplayView.winner1.Text = top4[0].OverlayName + " Wins!";
					challongeMatchDisplayView.winner2.Text = "2nd: " + top4[1].OverlayName;
					challongeMatchDisplayView.winner3.Text = "3rd: " + top4[2].OverlayName;
					challongeMatchDisplayView.winner4.Text = "4th: " + top4[3].OverlayName;
					challongeMatchDisplayView.CompleteAnimation();
				}
			}
			else
			{
				if (challongeOrganizerWindow != null)
				{
					challongeOrganizerWindow.endTournament.Visibility = Visibility.Visible;
				}
				challongeMatchDisplayView.Winners.Visibility = Visibility.Collapsed;
			}
		}).Invoke, null);
	}
}
