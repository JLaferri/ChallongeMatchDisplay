using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Libraries.ChallongeApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class ChallongeTournamentContext : ITournamentContext, IDisposable, INotifyPropertyChanged
{
	private readonly int tournamentId;

	private TimeSpan? _scanInterval;

	private int? _pollEvery;

	private ChallongeObservableTournament _tournament;

	private string _errorMessage;

	private IDisposable pollSubscription;

	public ChallongePortal Portal { get; private set; }

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
			return _tournament;
		}
		private set
		{
			this.RaiseAndSetIfChanged("Tournament", ref _tournament, (ChallongeObservableTournament)value, this.PropertyChanged);
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

	private Tuple<ChallongeTournament, IEnumerable<ChallongeParticipant>, IEnumerable<ChallongeMatch>> queryData()
	{
		try
		{
			ChallongeTournament item = Portal.ShowTournament(tournamentId);
			IEnumerable<ChallongeParticipant> participants = Portal.GetParticipants(tournamentId);
			IEnumerable<ChallongeMatch> matches = Portal.GetMatches(tournamentId);
			ErrorMessage = null;
			return Tuple.Create(item, participants, matches);
		}
		catch (ChallongeApiException ex)
		{
			if (ex.Errors != null)
			{
				ErrorMessage = ex.Errors.Aggregate((string one, string two) => one + "\r\n" + two);
			}
			else
			{
				ErrorMessage = $"Error with ResponseStatus \"{ex.RestResponse.ResponseStatus}\" and StatusCode \"{ex.RestResponse.StatusCode}\". {ex.RestResponse.ErrorMessage}";
			}
			return null;
		}
	}

	public ChallongeTournamentContext(ChallongePortal portal, int tournamentId)
	{
		Portal = portal;
		this.tournamentId = tournamentId;
		Tuple<ChallongeTournament, IEnumerable<ChallongeParticipant>, IEnumerable<ChallongeMatch>> tuple = queryData();
		if (tuple != null)
		{
			Tournament = new ChallongeObservableTournament(tuple.Item1, this);
			((ChallongeObservableTournament)Tournament).Initialize(tuple.Item2, tuple.Item3);
		}
	}

	public void StartSynchronization(TimeSpan timeInterval, int pollEvery)
	{
		StopSynchronization();
		pollSubscription = Observable.Interval(timeInterval).Subscribe(delegate(long num)
		{
			if (num % pollEvery == 0L)
			{
				Refresh();
			}
			else
			{
				CommitChanges();
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
		Tuple<ChallongeTournament, IEnumerable<ChallongeParticipant>, IEnumerable<ChallongeMatch>> tuple = queryData();
		if (tuple != null)
		{
			if (Tournament == null)
			{
				Tournament = new ChallongeObservableTournament(tuple.Item1, this);
				((ChallongeObservableTournament)Tournament).Initialize(tuple.Item2, tuple.Item3);
			}
			else
			{
				((ChallongeObservableTournament)Tournament).Update(tuple.Item1, tuple.Item2, tuple.Item3);
			}
		}
	}

	public void CommitChanges()
	{
		if (Tournament == null || ((ChallongeObservableTournament)Tournament).Participants == null)
		{
			return;
		}
		foreach (KeyValuePair<int, ChallongeObservableParticipant> p in ((ChallongeObservableTournament)Tournament).Participants)
		{
			Dirtyable<ParticipantMiscProperties> miscDirtyable = p.Value.MiscProperties;
			try
			{
				miscDirtyable.CommitIfDirty(delegate
				{
					Portal.SetParticipantMisc(((ChallongeObservableTournament)Tournament).Id, p.Value.Id, miscDirtyable.Value.ToString());
				});
			}
			catch (Exception)
			{
			}
		}
	}

	public void EndTournament()
	{
		Portal.EndTournament(tournamentId);
	}

	public void Dispose()
	{
		StopSynchronization();
	}
}
