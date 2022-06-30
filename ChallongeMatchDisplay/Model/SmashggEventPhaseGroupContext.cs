using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Libraries.SmashggApiWrapper;
using log4net;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class SmashggEventPhaseGroupContext : ITournamentContext, IDisposable, INotifyPropertyChanged
{
    ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	private readonly long phaseGroupId;

	private readonly long phaseId;

	private readonly long tournamentId;

	private TimeSpan? _scanInterval;

	private int? _pollEvery;

	private SmashggObservablePhaseGroup _phaseGroup;

	private string _errorMessage;

	private IDisposable pollSubscription;

	public SmashggPortal Portal { get; private set; }

	public TimeSpan? ScanInterval
	{
		get
		{
			return _scanInterval;
		}
		private set
		{
			this.RaiseAndSetIfChanged("ScanInterval", ref _scanInterval, value, this.PropertyChanged);
		}
	}

	public int? PollEvery
	{
		get
		{
			return _pollEvery;
		}
		private set
		{
			this.RaiseAndSetIfChanged("PollEvery", ref _pollEvery, value, this.PropertyChanged);
		}
	}

	public IObservableTournament Tournament
	{
		get
		{
			return _phaseGroup;
		}
		private set
		{
			this.RaiseAndSetIfChanged("Tournament", ref _phaseGroup, (SmashggObservablePhaseGroup)value, this.PropertyChanged);
		}
	}

	public bool IsError => _errorMessage != null;

	public string ErrorMessage
	{
		get
		{
			return _errorMessage;
		}
		private set
		{
			this.RaiseAndSetIfChanged("ErrorMessage", ref _errorMessage, value, this.PropertyChanged);
			this.Raise("IsError", this.PropertyChanged);
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	private Tuple<SmashggPhaseGroup, IEnumerable<SmashggMatch>, SmashggTournament, SmashggPhase> queryData()
	{
		try
		{
			SmashggPhaseGroup item = Portal.ShowPhaseGroup(phaseGroupId);
			IEnumerable<SmashggMatch> matches = Portal.GetMatches(phaseGroupId);
			SmashggTournament item2 = Portal.ShowTournament(tournamentId);
			SmashggPhase item3 = Portal.ShowPhase(phaseId);
			ErrorMessage = null;
			return Tuple.Create(item, matches, item2, item3);
		}
		catch (SmashggApiException ex)
		{
			if (ex.Errors != null)
			{
				ErrorMessage = ex.Errors.Aggregate((string one, string two) => one + "\r\n" + two);
			}
			else
			{
				ErrorMessage = $"Error with ResponseStatus \"{ex.RestResponse.ResponseStatus}\" and StatusCode \"{ex.RestResponse.StatusCode}\". {ex.RestResponse.ErrorMessage}";
				Log.Error("SmashggApiException", ex);
			}
			return null;
		}
	}

	public SmashggEventPhaseGroupContext(SmashggPortal portal, long phaseGroupId, long phaseId, long tournamentId)
	{
		Portal = portal;
		this.phaseGroupId = phaseGroupId;
		this.phaseId = phaseId;
		this.tournamentId = tournamentId;
		Tuple<SmashggPhaseGroup, IEnumerable<SmashggMatch>, SmashggTournament, SmashggPhase> tuple = queryData();
		if (tuple != null)
		{
			Tournament = new SmashggObservablePhaseGroup(tuple.Item1, tuple.Item4, tuple.Item3, this);
			((SmashggObservablePhaseGroup)Tournament).Initialize(tuple.Item2);
		}
	}

	public void StartSynchronization(TimeSpan timeInterval, int pollEvery)
	{
		StopSynchronization();
		pollSubscription = Observable.Interval(timeInterval).Subscribe(delegate(long num)
		{
			if (num % pollEvery == 0L)
			{
				try {
					Refresh();
				} catch (Exception ex) {
					Log.Error("Refresh Failed", ex);
                }
			}
		});
		ScanInterval = timeInterval;
		PollEvery = pollEvery;
	}

	public void StopSynchronization()
	{
		if (pollSubscription != null)
		{
			pollSubscription.Dispose();
			pollSubscription = null;
			ScanInterval = null;
			PollEvery = null;
		}
	}

	public void Refresh()
	{
		Tuple<SmashggPhaseGroup, IEnumerable<SmashggMatch>, SmashggTournament, SmashggPhase> tuple = queryData();
		if (tuple != null)
		{
			if (Tournament == null)
			{
				Tournament = new SmashggObservablePhaseGroup(tuple.Item1, tuple.Item4, tuple.Item3, this);
				((SmashggObservablePhaseGroup)Tournament).Initialize(tuple.Item2);
			}
			else
			{
				((SmashggObservablePhaseGroup)Tournament).Update(tuple.Item1, tuple.Item4, tuple.Item3, tuple.Item2);
			}
		}
	}

	public void Dispose()
	{
		StopSynchronization();
	}

	public void EndTournament()
	{
	}
}
