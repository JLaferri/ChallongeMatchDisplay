using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.Model;
using Fizzi.Libraries.SmashggApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel;

internal class SmashggOrganizerViewModel : IOrganizerViewModel, INotifyPropertyChanged, IDisposable
{
	private bool _isBusy;

	private string _errorMessage;

	private SmashggObservableMatch _selectedMatch;

	private SmashggStation _selectedStation;

	private IDisposable matchesMonitoring;

	private IDisposable matchStateMonitoring;

	public MainViewModel Mvm { get; private set; }

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

	public string ErrorMessage
	{
		get
		{
			return _errorMessage;
		}
		set
		{
			this.RaiseAndSetIfChanged("ErrorMessage", ref _errorMessage, value, this.PropertyChanged);
		}
	}

	public ObservableCollection<SmashggDisplayMatch> OpenMatches { get; private set; }

	public ObservableCollection<SmashggStation> OpenStations { get; private set; }

	public SmashggObservableMatch SelectedMatch
	{
		get
		{
			return _selectedMatch;
		}
		set
		{
			this.RaiseAndSetIfChanged("SelectedMatch", ref _selectedMatch, value, this.PropertyChanged);
		}
	}

	public SmashggStation SelectedStation
	{
		get
		{
			return _selectedStation;
		}
		set
		{
			this.RaiseAndSetIfChanged("SelectedStation", ref _selectedStation, value, this.PropertyChanged);
		}
	}

	public ICommand ImportStationFile { get; private set; }

	public ICommand AutoAssignPending { get; private set; }

	public ICommand CallPendingAnywhere { get; private set; }

	public ICommand ClearAllAssignments { get; private set; }

	public event PropertyChangedEventHandler PropertyChanged;

	public SmashggOrganizerViewModel(MainViewModel mvm, SynchronizationContext dispatcher)
	{
		SmashggOrganizerViewModel smashggOrganizerViewModel = this;
		Mvm = mvm;
		OpenMatches = new ObservableCollection<SmashggDisplayMatch>();
		OpenStations = new ObservableCollection<SmashggStation>();
		IObservable<EventPattern<PropertyChangedEventArgs>> source = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(delegate(PropertyChangedEventHandler h)
		{
			mvm.PropertyChanged += h;
		}, delegate(PropertyChangedEventHandler h)
		{
			mvm.PropertyChanged -= h;
		});
		matchesMonitoring = (from _ in (from ep in source
				where ep.EventArgs.PropertyName == "DisplayMatches"
				select ep into _
				select Unit.Default).StartWith(Unit.Default)
			where mvm.SmashggDisplayMatches != null
			select _).ObserveOn(dispatcher).Subscribe(delegate
		{
			smashggOrganizerViewModel.initialize(mvm.SmashggDisplayMatches.Where((SmashggDisplayMatch dm) => dm.MatchDisplayType == SmashggDisplayMatch.DisplayType.Assigned).ToArray());
		});
	}

	private void initialize(SmashggDisplayMatch[] matches)
	{
		OpenMatches.Clear();
		if (matchStateMonitoring != null)
		{
			matchStateMonitoring.Dispose();
		}
		IDisposable[] disposables = matches.Select(delegate(SmashggDisplayMatch dm)
		{
			SmashggObservableMatch i = dm.Match;
			return (from ep in Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(delegate(PropertyChangedEventHandler h)
				{
					i.PropertyChanged += h;
				}, delegate(PropertyChangedEventHandler h)
				{
					i.PropertyChanged -= h;
				})
				where ep.EventArgs.PropertyName == "State"
				select ep into _
				select Unit.Default).StartWith(Unit.Default).ObserveOnDispatcher().Subscribe(delegate
			{
				if (i.State == SmashggMatchState.ACTIVE)
				{
					OpenMatches.Add(dm);
				}
				else
				{
					OpenMatches.Remove(dm);
				}
			});
		}).ToArray();
		matchStateMonitoring = new CompositeDisposable(disposables);
	}

	public void Dispose()
	{
		if (matchStateMonitoring != null)
		{
			matchStateMonitoring.Dispose();
		}
		if (matchesMonitoring != null)
		{
			matchesMonitoring.Dispose();
		}
	}
}
