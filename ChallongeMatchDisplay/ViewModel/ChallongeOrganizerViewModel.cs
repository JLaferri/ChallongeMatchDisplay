using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.Model;
using Fizzi.Applications.ChallongeVisualization.Properties;
using Fizzi.Libraries.ChallongeApiWrapper;
using Microsoft.Win32;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel;

internal class ChallongeOrganizerViewModel : IOrganizerViewModel, INotifyPropertyChanged, IDisposable
{
	private bool _isBusy;

	private string _errorMessage;

	private ChallongeObservableMatch _selectedMatch;

	private ChallongeStationModel _selectedStation;

	private IDisposable matchesMonitoring;

	private IDisposable matchStateMonitoring;

	private IDisposable stationMonitoring;

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

	public ObservableCollection<ChallongeDisplayMatch> OpenMatches { get; private set; }

	public ObservableCollection<ChallongeStationModel> OpenStations { get; private set; }

	public ChallongeObservableMatch SelectedMatch
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

	public ChallongeStationModel SelectedStation
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

	public ChallongeOrganizerViewModel(MainViewModel mvm, SynchronizationContext dispatcher)
	{
		ChallongeOrganizerViewModel challongeOrganizerViewModel = this;
		Mvm = mvm;
		OpenMatches = new ObservableCollection<ChallongeDisplayMatch>();
		OpenStations = new ObservableCollection<ChallongeStationModel>();
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
			where mvm.ChallongeDisplayMatches != null
			select _).ObserveOn(dispatcher).Subscribe(delegate
		{
			challongeOrganizerViewModel.initialize(mvm.ChallongeDisplayMatches.Where((ChallongeDisplayMatch dm) => dm.MatchDisplayType == ChallongeDisplayMatch.DisplayType.Assigned).ToArray());
		});
		ImportStationFile = Command.Create((Window _) => true, delegate(Window window)
		{
			try
			{
				OpenFileDialog openFileDialog = new OpenFileDialog
				{
					Filter = "Text List (*.csv;*.txt)|*.csv;*.txt|All files (*.*)|*.*",
					RestoreDirectory = true,
					Title = "Browse for Station File"
				};
				bool? flag = openFileDialog.ShowDialog(window);
				if (flag.HasValue && flag.Value)
				{
					string fileName = openFileDialog.FileName;
					challongeOrganizerViewModel.initializeStations(fileName);
				}
			}
			catch (Exception ex3)
			{
				MessageBox.Show(window, "Error encountered trying to import station list: " + ex3.NewLineDelimitedMessages(), "Error Importing", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		});
		Action onStart = delegate
		{
			challongeOrganizerViewModel.ErrorMessage = null;
			challongeOrganizerViewModel.IsBusy = true;
		};
		Action onCompletion = delegate
		{
			challongeOrganizerViewModel.IsBusy = false;
		};
		Action<Exception> onError = delegate(Exception ex)
		{
			if (ex.InnerException is ChallongeApiException)
			{
				ChallongeApiException ex2 = (ChallongeApiException)ex.InnerException;
				if (ex2.Errors != null)
				{
					challongeOrganizerViewModel.ErrorMessage = ex2.Errors.Aggregate((string one, string two) => one + "\r\n" + two);
				}
				else
				{
					challongeOrganizerViewModel.ErrorMessage = $"Error with ResponseStatus \"{ex2.RestResponse.ResponseStatus}\" and StatusCode \"{ex2.RestResponse.StatusCode}\". {ex2.RestResponse.ErrorMessage}";
				}
			}
			else
			{
				challongeOrganizerViewModel.ErrorMessage = ex.NewLineDelimitedMessages();
			}
			challongeOrganizerViewModel.IsBusy = false;
		};
		AutoAssignPending = Command.CreateAsync(() => true, delegate
		{
			ChallongeStations.Instance.AssignOpenMatchesToStations(challongeOrganizerViewModel.OpenMatches.Select((ChallongeDisplayMatch dm) => dm.Match).ToArray());
		}, onStart, onCompletion, onError);
		CallPendingAnywhere = Command.CreateAsync(() => true, delegate
		{
			ChallongeDisplayMatch[] array2 = challongeOrganizerViewModel.OpenMatches.Where((ChallongeDisplayMatch m) => !m.Match.IsMatchInProgress).ToArray();
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].Match.AssignPlayersToStation("Any");
			}
		}, onStart, onCompletion, onError);
		ClearAllAssignments = Command.CreateAsync(() => true, delegate
		{
			ChallongeDisplayMatch[] array = challongeOrganizerViewModel.OpenMatches.Where((ChallongeDisplayMatch m) => m.Match.IsMatchInProgress).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Match.ClearStationAssignment();
			}
		}, onStart, onCompletion, onError);
	}

	private void initialize(ChallongeDisplayMatch[] matches)
	{
		loadStationsFromSettings();
		OpenMatches.Clear();
		if (matchStateMonitoring != null)
		{
			matchStateMonitoring.Dispose();
		}
		IDisposable[] disposables = matches.Select(delegate(ChallongeDisplayMatch dm)
		{
			ChallongeObservableMatch i = dm.Match;
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
				if (i.State == "open")
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

	public void reinitializeStations(ChallongeStationModel[] uniqueStations)
	{
		OpenStations.Clear();
		if (stationMonitoring != null)
		{
			stationMonitoring.Dispose();
		}
		foreach (ChallongeStationModel challongeStationModel in uniqueStations)
		{
			if (challongeStationModel.Status == ChallongeStationStatus.Open)
			{
				OpenStations.Add(challongeStationModel);
			}
		}
		ChallongeStations instance = ChallongeStations.Instance;
		instance.LoadNew(uniqueStations);
		IDisposable[] disposables = (from kvp in instance.Dict
			select kvp.Value into s
			select (from ep in Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(delegate(PropertyChangedEventHandler h)
				{
					s.PropertyChanged += h;
				}, delegate(PropertyChangedEventHandler h)
				{
					s.PropertyChanged -= h;
				})
				where ep.EventArgs.PropertyName == "Status"
				select ep).ObserveOnDispatcher().Subscribe(delegate
			{
				if (s.Status == ChallongeStationStatus.Open)
				{
					OpenStations.Add(s);
				}
				else
				{
					OpenStations.Remove(s);
				}
			})).ToArray();
		foreach (string item in from m in OpenMatches
			select m.Match.StationAssignment into sn
			where sn != null
			select sn)
		{
			instance.AttemptClaimStation(item);
		}
		stationMonitoring = new CompositeDisposable(disposables);
		instance.Save();
	}

	private void initializeStations(string filePath)
	{
		ChallongeStationModel[] uniqueStations = (from a in File.ReadAllLines(filePath).Select(delegate(string line)
			{
				string[] array = line.Split(',');
				string name = ((array.Length == 0) ? string.Empty : array[0]);
				ChallongeStationType type = ChallongeStationType.Standard;
				if (array.Length > 1)
				{
					switch (array[1].Trim().ToLower())
					{
					case "stream":
						type = ChallongeStationType.Stream;
						break;
					case "recording":
						type = ChallongeStationType.Recording;
						break;
					case "premium":
						type = ChallongeStationType.Premium;
						break;
					case "backup":
						type = ChallongeStationType.Backup;
						break;
					case "noassign":
						type = ChallongeStationType.NoAssign;
						break;
					}
				}
				return new
				{
					Name = name,
					Type = type
				};
			})
			group a by a.Name into g
			select g.First()).Where(a =>
		{
			string text = a.Name.Trim().ToLower();
			return text != "any" && !string.IsNullOrWhiteSpace(text);
		}).Select((a, i) => new ChallongeStationModel(a.Name, i, a.Type)).ToArray();
		reinitializeStations(uniqueStations);
	}

	private void loadStationsFromSettings()
	{
		if (Settings.Default.stationNames != null && Settings.Default.stationNames.Count > 0)
		{
			ChallongeStationModel[] array = new ChallongeStationModel[Settings.Default.stationNames.Count];
			for (int i = 0; i < Settings.Default.stationNames.Count; i++)
			{
				ChallongeStationType type = ChallongeStationType.Standard;
				string name = Settings.Default.stationNames[i];
				switch ((Settings.Default.stationTypes == null || Settings.Default.stationTypes.Count <= i) ? "" : Settings.Default.stationTypes[i].Trim().ToLower())
				{
				case "stream":
					type = ChallongeStationType.Stream;
					break;
				case "recording":
					type = ChallongeStationType.Recording;
					break;
				case "premium":
					type = ChallongeStationType.Premium;
					break;
				case "backup":
					type = ChallongeStationType.Backup;
					break;
				case "noassign":
					type = ChallongeStationType.NoAssign;
					break;
				}
				ChallongeStationModel challongeStationModel = (array[i] = new ChallongeStationModel(name, i, type));
			}
			reinitializeStations(array);
		}
		else
		{
			ChallongeStationModel[] uniqueStations = new ChallongeStationModel[0];
			reinitializeStations(uniqueStations);
		}
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
		if (stationMonitoring != null)
		{
			stationMonitoring.Dispose();
		}
	}
}
