using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.Model;
using Fizzi.Applications.ChallongeVisualization.Properties;
using Fizzi.Libraries.ChallongeApiWrapper;
using Fizzi.Libraries.SmashggApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel;

internal class MainViewModel : INotifyPropertyChanged
{
	private string _mostRecentVersion;

	private ScreenType _currentScreen;

	private string _challongeApiKey;

	private string _challongeSubdomain;

	private string _smashggApiToken;

	private string _smashggSlug;

	private List<ChallongeDisplayMatch> _challongeDisplayMatches;

	private List<SmashggDisplayMatch> _smashggDisplayMatches;

	private ChallongeTournament _challongeSelectedTournament;

	private Tuple<long, long, long> _smashggSelectedEventPhaseGroupData;

	private PropertyChangedEventHandler matchesChangedHandler;

	private bool _newVersionAvailable;

	private bool _isBusy;

	private bool _isVersionOutdatedVisible;

	private string _challongeErrorMessage;

	private string _smashggErrorMessage;

	public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

	public string MostRecentVersion
	{
		get
		{
			return _mostRecentVersion;
		}
		set
		{
			this.RaiseAndSetIfChanged("MostRecentVersion", ref _mostRecentVersion, value, this.PropertyChanged);
		}
	}

	public ScreenType CurrentScreen
	{
		get
		{
			return _currentScreen;
		}
		set
		{
			this.RaiseAndSetIfChanged("CurrentScreen", ref _currentScreen, value, this.PropertyChanged);
		}
	}

	public string ChallongeApiKey
	{
		get
		{
			return _challongeApiKey;
		}
		set
		{
			this.RaiseAndSetIfChanged("ChallongeApiKey", ref _challongeApiKey, value, this.PropertyChanged);
		}
	}

	public string ChallongeSubdomain
	{
		get
		{
			return _challongeSubdomain;
		}
		set
		{
			this.RaiseAndSetIfChanged("Subdomain", ref _challongeSubdomain, value, this.PropertyChanged);
		}
	}

	public string SmashggApiToken
	{
		get
		{
			return _smashggApiToken;
		}
		set
		{
			this.RaiseAndSetIfChanged("SmashggApiKey", ref _smashggApiToken, value, this.PropertyChanged);
		}
	}

	public string SmashggSlug
	{
		get
		{
			return _smashggSlug;
		}
		set
		{
			this.RaiseAndSetIfChanged("SmashggSlug", ref _smashggSlug, value, this.PropertyChanged);
		}
	}

	public ChallongePortal MyChallongePortal { get; private set; }

	public SmashggPortal MySmashggPortal { get; private set; }

	public ChallongeTournament[] ChallongeTournamentCollection { get; private set; }

	public SmashggTournament[] SmashggTournamentCollection { get; private set; }

	public List<ChallongeDisplayMatch> ChallongeDisplayMatches
	{
		get
		{
			return _challongeDisplayMatches;
		}
		set
		{
			this.RaiseAndSetIfChanged("ChallongeDisplayMatches", ref _challongeDisplayMatches, value, this.PropertyChanged);
		}
	}

	public List<SmashggDisplayMatch> SmashggDisplayMatches
	{
		get
		{
			return _smashggDisplayMatches;
		}
		set
		{
			this.RaiseAndSetIfChanged("SmashggDisplayMatches", ref _smashggDisplayMatches, value, this.PropertyChanged);
		}
	}

	public ChallongeTournament ChallongeSelectedTournament
	{
		get
		{
			return _challongeSelectedTournament;
		}
		set
		{
			this.RaiseAndSetIfChanged("ChallongeSelectedTournament", ref _challongeSelectedTournament, value, this.PropertyChanged);
		}
	}

	public Tuple<long, long, long> SmashggSelectedEventPhaseGroupData
	{
		get
		{
			return _smashggSelectedEventPhaseGroupData;
		}
		set
		{
			this.RaiseAndSetIfChanged("SmashggSelectedEventPhaseGroupData", ref _smashggSelectedEventPhaseGroupData, value, this.PropertyChanged);
		}
	}

	public ITournamentContext Context { get; private set; }

	public bool NewVersionAvailable
	{
		get
		{
			return _newVersionAvailable;
		}
		set
		{
			this.RaiseAndSetIfChanged("NewVersionAvailable", ref _newVersionAvailable, value, this.PropertyChanged);
		}
	}

	public bool IsBusy
	{
		get
		{
			return _isBusy;
		}
		set
		{
			this.RaiseAndSetIfChanged("IsBusy", ref _isBusy, value, this.PropertyChanged);
		}
	}

	public bool IsVersionOutdatedVisible
	{
		get
		{
			return _isVersionOutdatedVisible;
		}
		set
		{
			this.RaiseAndSetIfChanged("IsVersionOutdatedVisible", ref _isVersionOutdatedVisible, value, this.PropertyChanged);
		}
	}

	public string ChallongeErrorMessage
	{
		get
		{
			return _challongeErrorMessage;
		}
		set
		{
			this.RaiseAndSetIfChanged("ErrorMessage", ref _challongeErrorMessage, value, this.PropertyChanged);
		}
	}

	public string SmashggErrorMessage
	{
		get
		{
			return _smashggErrorMessage;
		}
		set
		{
			this.RaiseAndSetIfChanged("ErrorMessage", ref _smashggErrorMessage, value, this.PropertyChanged);
		}
	}

	public ICommand ChallongeNextCommand { get; private set; }

	public ICommand SmashggNextCommand { get; private set; }

	public ICommand ChallongeBack { get; private set; }

	public ICommand SmashggBack { get; private set; }

	public ICommand IgnoreVersionNotification { get; private set; }

	public string ThreadUrl => "http://smashboards.com/threads/challonge-match-display-application-helping-tournaments-run-faster.358186/";

	public IOrganizerViewModel OrgViewModel { get; private set; }

	public event PropertyChangedEventHandler PropertyChanged;

	public MainViewModel()
	{
		CurrentScreen = ScreenType.ApiKey;
		ChallongeApiKey = Settings.Default.challonge_apikey;
		ChallongeSubdomain = Settings.Default.challonge_subdomain;
		SmashggApiToken = Settings.Default.smashgg_apitoken;
		SmashggSlug = Settings.Default.smashgg_slug;
		Action onStart = delegate
		{
			ChallongeErrorMessage = null;
			SmashggErrorMessage = null;
			IsBusy = true;
		};
		Action onCompletion = delegate
		{
			IsBusy = false;
		};
		Action<Exception> onError = delegate(Exception ex)
		{
			if (ex.InnerException is ChallongeApiException)
			{
				ChallongeApiException ex4 = (ChallongeApiException)ex.InnerException;
				if (ex4.Errors != null)
				{
					ChallongeErrorMessage = ex4.Errors.Aggregate((string one, string two) => one + "\r\n" + two);
				}
				else
				{
					ChallongeErrorMessage = $"Error with ResponseStatus \"{ex4.RestResponse.ResponseStatus}\" and StatusCode \"{ex4.RestResponse.StatusCode}\". {ex4.RestResponse.ErrorMessage}";
				}
			}
			else
			{
				ChallongeErrorMessage = ex.NewLineDelimitedMessages();
			}
			IsBusy = false;
		};
		Action<Exception> onError2 = delegate(Exception ex)
		{
			if (ex.InnerException is ChallongeApiException)
			{
				ChallongeApiException ex3 = (ChallongeApiException)ex.InnerException;
				if (ex3.Errors != null)
				{
					ChallongeErrorMessage = ex3.Errors.Aggregate((string one, string two) => one + "\r\n" + two);
				}
				else
				{
					SmashggErrorMessage = $"Error with ResponseStatus \"{ex3.RestResponse.ResponseStatus}\" and StatusCode \"{ex3.RestResponse.StatusCode}\". {ex3.RestResponse.ErrorMessage}";
				}
			}
			else
			{
				SmashggErrorMessage = ex.NewLineDelimitedMessages();
			}
			IsBusy = false;
		};
		SynchronizationContext dispatcher = SynchronizationContext.Current;
		ChallongeNextCommand = Command.CreateAsync(() => true, delegate
		{
			switch (CurrentScreen)
			{
			case ScreenType.ApiKey:
			{
				string subdomain = (string.IsNullOrWhiteSpace(ChallongeSubdomain) ? null : ChallongeSubdomain);
				MyChallongePortal = new ChallongePortal(ChallongeApiKey, subdomain);
				ChallongeTournamentCollection = (from t in MyChallongePortal.GetTournaments()
					orderby t.CreatedAt descending
					select t).ToArray();
				try
				{
					// TODO create new version test tournament

					//ChallongePortal challongePortal = new ChallongePortal(ChallongeApiKey, "fizzitestorg");
					//MostRecentVersion = (from t in challongePortal.GetTournaments()
					//	where t.Name == "CMDVersionTest"
					//	select string.Concat(t.Description.Where((char c) => char.IsDigit(c) || c == '.'))).First();
					//int num = Version.Split('.').Zip(MostRecentVersion.Split('.'), (string v, string mrv) => int.Parse(v).CompareTo(int.Parse(mrv))).FirstOrDefault((int i) => i != 0);
					//IsVersionOutdatedVisible = num < 0;
				}
				catch (Exception)
				{
				}
				CurrentScreen = ScreenType.ChallongeTournamentSelection;
				break;
			}
			case ScreenType.ChallongeTournamentSelection:
				if (Context != null)
				{
					Context.Dispose();
				}
				if (matchesChangedHandler != null)
				{
					Context.Tournament.PropertyChanged -= matchesChangedHandler;
				}
				Context = new ChallongeTournamentContext(MyChallongePortal, ChallongeSelectedTournament.Id);
				Context.StartSynchronization(TimeSpan.FromMilliseconds(500.0), 6);
				OrgViewModel = new ChallongeOrganizerViewModel(this, dispatcher);
				ChallongeDisplayMatches = ((ChallongeObservableTournament)Context.Tournament).Matches.Select((KeyValuePair<int, IObservableMatch> kvp) => new ChallongeDisplayMatch((ChallongeOrganizerViewModel)OrgViewModel, (ChallongeObservableMatch)kvp.Value, ChallongeDisplayMatch.DisplayType.Assigned)).Concat(((ChallongeObservableTournament)Context.Tournament).Matches.Select((KeyValuePair<int, IObservableMatch> kvp) => new ChallongeDisplayMatch((ChallongeOrganizerViewModel)OrgViewModel, (ChallongeObservableMatch)kvp.Value, ChallongeDisplayMatch.DisplayType.Unassigned))).ToList();
				matchesChangedHandler = delegate(object sender, PropertyChangedEventArgs e)
				{
					if (e.PropertyName == "Matches")
					{
						if (((ChallongeObservableTournament)Context.Tournament).Matches == null)
						{
							ChallongeDisplayMatches = null;
						}
						else
						{
							ChallongeDisplayMatches = ((ChallongeObservableTournament)Context.Tournament).Matches.Select((KeyValuePair<int, IObservableMatch> kvp) => new ChallongeDisplayMatch((ChallongeOrganizerViewModel)OrgViewModel, (ChallongeObservableMatch)kvp.Value, ChallongeDisplayMatch.DisplayType.Assigned)).Concat(((ChallongeObservableTournament)Context.Tournament).Matches.Select((KeyValuePair<int, IObservableMatch> kvp) => new ChallongeDisplayMatch((ChallongeOrganizerViewModel)OrgViewModel, (ChallongeObservableMatch)kvp.Value, ChallongeDisplayMatch.DisplayType.Unassigned))).ToList();
						}
					}
				};
				Context.Tournament.PropertyChanged += matchesChangedHandler;
				CurrentScreen = ScreenType.ChallongePendingMatchView;
				break;
			}
		}, onStart, onCompletion, onError);
		SmashggNextCommand = Command.CreateAsync(() => true, delegate
		{
			switch (CurrentScreen)
			{
			case ScreenType.ApiKey:
				MySmashggPortal = new SmashggPortal(SmashggApiToken, SmashggSlug);
				SmashggTournamentCollection = (from t in MySmashggPortal.GetTournaments()
					orderby t.CreatedAt descending
					select t).ToArray();
				CurrentScreen = ScreenType.SmashggEventPhaseGroupSelection;
				break;
			case ScreenType.SmashggEventPhaseGroupSelection:
				if (Context != null)
				{
					Context.Dispose();
				}
				if (matchesChangedHandler != null)
				{
					Context.Tournament.PropertyChanged -= matchesChangedHandler;
				}
				Context = new SmashggEventPhaseGroupContext(MySmashggPortal, SmashggSelectedEventPhaseGroupData.Item1, SmashggSelectedEventPhaseGroupData.Item2, SmashggSelectedEventPhaseGroupData.Item3);
				Context.StartSynchronization(TimeSpan.FromMilliseconds(1000.0), 5);
				OrgViewModel = new SmashggOrganizerViewModel(this, dispatcher);
				SmashggDisplayMatches = ((SmashggObservablePhaseGroup)Context.Tournament).Matches.Select((KeyValuePair<string, IObservableMatch> kvp) => new SmashggDisplayMatch((SmashggOrganizerViewModel)OrgViewModel, (SmashggObservableMatch)kvp.Value, SmashggDisplayMatch.DisplayType.Assigned)).Concat(((SmashggObservablePhaseGroup)Context.Tournament).Matches.Select((KeyValuePair<string, IObservableMatch> kvp) => new SmashggDisplayMatch((SmashggOrganizerViewModel)OrgViewModel, (SmashggObservableMatch)kvp.Value, SmashggDisplayMatch.DisplayType.Unassigned))).ToList();
				CurrentScreen = ScreenType.SmashggPendingMatchView;
				break;
			}
		}, onStart, onCompletion, onError2);
		ChallongeBack = Command.CreateAsync(() => true, delegate
		{
			switch (CurrentScreen)
			{
			case ScreenType.ChallongeTournamentSelection:
				ChallongeApiKey = Settings.Default.challonge_apikey;
				CurrentScreen = ScreenType.ApiKey;
				break;
			case ScreenType.ChallongePendingMatchView:
				if (OrgViewModel != null)
				{
					OrgViewModel.Dispose();
					OrgViewModel = null;
				}
				CurrentScreen = ScreenType.ChallongeTournamentSelection;
				break;
			}
		}, onStart, onCompletion, onError);
		SmashggBack = Command.CreateAsync(() => true, delegate
		{
			switch (CurrentScreen)
			{
			case ScreenType.SmashggEventPhaseGroupSelection:
				SmashggApiToken = Settings.Default.smashgg_apitoken;
				CurrentScreen = ScreenType.ApiKey;
				break;
			case ScreenType.SmashggPendingMatchView:
				if (OrgViewModel != null)
				{
					OrgViewModel.Dispose();
					OrgViewModel = null;
				}
				CurrentScreen = ScreenType.SmashggEventPhaseGroupSelection;
				break;
			}
		}, onStart, onCompletion, onError2);
		IgnoreVersionNotification = Command.Create(() => true, delegate
		{
			IsVersionOutdatedVisible = false;
		});
		if (ChallongeApiKey != "")
		{
			ChallongeNextCommand.Execute(null);
		}
	}
}
