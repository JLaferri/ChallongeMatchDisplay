using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fizzi.Applications.ChallongeVisualization.Model;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Threading;
using Fizzi.Applications.ChallongeVisualization.Common;
using System.Windows.Input;
using Fizzi.Libraries.ChallongeApiWrapper;
using System.IO;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel
{
    class OrganizerViewModel : INotifyPropertyChanged, IDisposable
    {
        public MainViewModel Mvm { get; private set; }

        public ObservableCollection<ObservableMatch> OpenMatches { get; private set; }
        public ObservableCollection<Station> OpenStations { get; private set; }

        private ObservableMatch _selectedMatch;
        public ObservableMatch SelectedMatch { get { return _selectedMatch; } set { this.RaiseAndSetIfChanged("SelectedMatch", ref _selectedMatch, value, PropertyChanged); } }

        private Station _selectedStation;
        public Station SelectedStation { get { return _selectedStation; } set { this.RaiseAndSetIfChanged("SelectedStation", ref _selectedStation, value, PropertyChanged); } }

        private IDisposable matchesMonitoring;
        private IDisposable matchStateMonitoring;
        private IDisposable stationMonitoring;

        public ICommand AssignStation { get; private set; }
        public ICommand AssignNoStation { get; private set; }
        public ICommand UnassignStation { get; private set; }

        public ICommand Player1Wins { get; private set; }
        public ICommand Player2Wins { get; private set; }

        public ICommand ImportStationFile { get; private set; }

        public OrganizerViewModel(MainViewModel mvm)
        {
            Mvm = mvm;

            OpenMatches = new ObservableCollection<ObservableMatch>();
            OpenStations = new ObservableCollection<Station>();

            var tournament = mvm.Context.Tournament;
            var tournamentPropertyChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => tournament.PropertyChanged += h, h => tournament.PropertyChanged -= h);

            //Monitor if matches change (for example on a bracket reset)
            matchesMonitoring = tournamentPropertyChanged.Where(ep => ep.EventArgs.PropertyName == "Matches")
                .Select(_ => System.Reactive.Unit.Default).StartWith(System.Reactive.Unit.Default)
                .ObserveOnDispatcher().Subscribe(_ => initialize(mvm.Context.Tournament.Matches));

            AssignStation = Command.Create(() => true, () =>
            {
                SelectedMatch.AssignPlayersToStation(SelectedStation.Name);

                //Move selection to the next unassigned match for easier batch match assignment
                SelectedMatch = OpenMatches.OrderBy(m => m.RoundOrder).ThenBy(m => m.IsWinners).ThenBy(m => m.Identifier)
                    .SkipWhile(m => m != SelectedMatch).FirstOrDefault(m => !m.IsMatchInProgress);
            });

            AssignNoStation = Command.Create(() => true, () =>
            {
                SelectedMatch.AssignPlayersToStation("Any");

                //Move selection to the next unassigned match for easier batch match assignment
                SelectedMatch = OpenMatches.OrderBy(m => m.RoundOrder).ThenBy(m => m.IsWinners).ThenBy(m => m.Identifier)
                    .SkipWhile(m => m != SelectedMatch).FirstOrDefault(m => !m.IsMatchInProgress);
            });

            UnassignStation = Command.Create(() => true, () =>
            {
                if (SelectedMatch.Player1 != null) SelectedMatch.Player1.ClearStationAssignment();
                if (SelectedMatch.Player2 != null) SelectedMatch.Player2.ClearStationAssignment();
            });

            Player1Wins = Command.Create(() => true, () =>
            {
                SelectedMatch.ReportPlayer1Victory(SetScore.Create(1, 0));
            });

            Player2Wins = Command.Create(() => true, () =>
            {
                SelectedMatch.ReportPlayer2Victory(SetScore.Create(0, 1));
            });

            ImportStationFile = Command.Create<System.Windows.Window>(_ => true, window =>
            {
                var ofd = new Microsoft.Win32.OpenFileDialog()
                {
                    Filter = "Text List (*.csv;*.txt)|*.csv;*.txt|All files (*.*)|*.*",
                    RestoreDirectory = true,
                    Title = "Browse for Station File"
                };

                var result = ofd.ShowDialog(window);
                if (result.HasValue && result.Value)
                {
                    var path = ofd.FileName;

                    initializeStations(path);
                }
            });
        }

        private void initialize(Dictionary<int, ObservableMatch> matches)
        {
            OpenMatches.Clear();

            if (matchStateMonitoring != null) matchStateMonitoring.Dispose();

            var subscriptions = matches.Select(kvp => kvp.Value).Select(m =>
            {
                var matchPropChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => m.PropertyChanged += h, h => m.PropertyChanged -= h);

                return matchPropChanged.Where(ep => ep.EventArgs.PropertyName == "State")
                    .Select(_ => System.Reactive.Unit.Default).StartWith(System.Reactive.Unit.Default)
                    .ObserveOnDispatcher().Subscribe(_ =>
                    {
                        if (m.State == "open") OpenMatches.Add(m);
                        else OpenMatches.Remove(m);
                    });
            }).ToArray();

            matchStateMonitoring = new CompositeDisposable(subscriptions);
        }

        private void initializeStations(string[] stationNames)
        {
            //Load stations
            OpenStations.Clear();

            if (stationMonitoring != null) stationMonitoring.Dispose();

            //Only allow distinct station names and dont allow any station called "Any" as that is a reserved name
            var uniqueStations = stationNames.Distinct().Where(name => name.Trim().ToLower() != "any").Select((name, i) => new Station(name, i)).ToArray();

            //Start by adding all stations as open stations to the collection
            foreach (var s in uniqueStations) OpenStations.Add(s);

            //Load up Stations instance with new stations
            var stations = Stations.Instance;
            stations.LoadNew(uniqueStations);

            //Hook up station status change monitoring events for all stations
            var allStations = stations.Dict;
            var subscriptions = allStations.Select(kvp => kvp.Value).Select(s =>
            {
                var stationPropChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => s.PropertyChanged += h, h => s.PropertyChanged -= h);

                return stationPropChanged.Where(ep => ep.EventArgs.PropertyName == "Status")
                    .ObserveOnDispatcher().Subscribe(_ =>
                    {
                        if (s.Status == StationStatus.Open) OpenStations.Add(s);
                        else OpenStations.Remove(s);
                    });
            }).ToArray();

            //Get the names of all currently "in use" stations and mark them as in use, removing them from the observable collection via the event listener that was just hooked up
            var inUseStations = OpenMatches.Select(m => m.StationAssignment).Where(sn => sn != null);
            foreach (var sn in inUseStations)
            {
                stations.AttemptClaimStation(sn);
            }

            stationMonitoring = new CompositeDisposable(subscriptions);
        }

        private void initializeStations(string filePath)
        {
            //Get station names from file
            var stationNames = File.ReadAllLines(filePath);

            //Initialize stations
            initializeStations(stationNames);
        }

        public void Dispose()
        {
            if (matchStateMonitoring != null) matchStateMonitoring.Dispose();
            if (matchesMonitoring != null) matchesMonitoring.Dispose();
            if (stationMonitoring != null) stationMonitoring.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
