using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Fizzi.Libraries.ChallongeApiWrapper;
using System.Reactive.Linq;
using Fizzi.Applications.ChallongeVisualization.Common;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    /// <summary>
    /// Keeps tournament information synchronized via polling
    /// </summary>
    class TournamentContext : IDisposable, INotifyPropertyChanged
    {
        private readonly int tournamentId;

        public ChallongePortal Portal { get; private set; }

        private TimeSpan? _currentPollInterval;
        public TimeSpan? CurrentPollInterval { get { return _currentPollInterval; } private set { this.RaiseAndSetIfChanged("CurrentPollInterval", ref _currentPollInterval, value, PropertyChanged); } }

        private ObservableTournament _tournament;
        public ObservableTournament Tournament { get { return _tournament; } private set { this.RaiseAndSetIfChanged("Tournament", ref _tournament, value, PropertyChanged); } }

        private bool _isError;
        public bool IsError { get { return _isError; } private set { this.RaiseAndSetIfChanged("IsError", ref _isError, value, PropertyChanged); } }

        private string _errorMessage;
        public string ErrorMessage { get { return _errorMessage; } private set { this.RaiseAndSetIfChanged("ErrorMessage", ref _errorMessage, value, PropertyChanged); } }

        private IDisposable pollSubscription = null;

        private Tuple<Tournament, IEnumerable<Participant>, IEnumerable<Match>> queryData()
        {
            try
            {
                var tournament = Portal.ShowTournament(tournamentId);
                var participants = Portal.GetParticipants(tournamentId);
                var matches = Portal.GetMatches(tournamentId);

                IsError = false;
                ErrorMessage = null;

                return Tuple.Create(tournament, participants, matches);
            }
            catch (ChallongeApiException ex)
            {
                if (ex.Errors != null) ErrorMessage = ex.Errors.Aggregate((one, two) => one + "\r\n" + two);
                else ErrorMessage = string.Format("Error with ResponseStatus \"{0}\" and StatusCode \"{1}\".", ex.RestResponse.ResponseStatus,
                    ex.RestResponse.StatusCode);

                IsError = true;
                return null;
            }
        }

        public TournamentContext(ChallongePortal portal, int tournamentId)
        {
            Portal = portal;
            this.tournamentId = tournamentId;

            var queryResults = queryData();
            if (queryResults != null) Tournament = new ObservableTournament(queryResults.Item1, queryResults.Item2, queryResults.Item3, this);
        }

        public void StartPolling(TimeSpan timeInterval)
        {
            StopPolling(); //Ensure polling has stopped before starting

            pollSubscription = Observable.Interval(timeInterval).Subscribe(_ => Refresh());
            CurrentPollInterval = timeInterval;
        }

        public void StopPolling()
        {
            if (pollSubscription != null)
            {
                pollSubscription.Dispose();
                pollSubscription = null;
                CurrentPollInterval = null;
            }
        }

        /// <summary>
        /// Refresh context data
        /// </summary>
        public void Refresh()
        {
            var queryResults = queryData();

            if (queryResults != null)
            {
                if (Tournament == null) Tournament = new ObservableTournament(queryResults.Item1, queryResults.Item2, queryResults.Item3, this);
                else Tournament.Update(queryResults.Item1, queryResults.Item2, queryResults.Item3);
            }
        }

        public void Dispose()
        {
            StopPolling();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
