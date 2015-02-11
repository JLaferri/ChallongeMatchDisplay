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

        private TimeSpan? _scanInterval;
        public TimeSpan? ScanInterval { get { return _scanInterval; } private set { this.RaiseAndSetIfChanged("ScanInterval", ref _scanInterval, value, PropertyChanged); } }

        private int? _pollEvery;
        public int? PollEvery { get { return _pollEvery; } private set { this.RaiseAndSetIfChanged("PollEvery", ref _pollEvery, value, PropertyChanged); } }

        private ObservableTournament _tournament;
        public ObservableTournament Tournament { get { return _tournament; } private set { this.RaiseAndSetIfChanged("Tournament", ref _tournament, value, PropertyChanged); } }

        public bool IsError { get { return _errorMessage != null; } }

        private string _errorMessage;
        public string ErrorMessage 
        { 
            get { return _errorMessage; } 
            private set 
            { 
                this.RaiseAndSetIfChanged("ErrorMessage", ref _errorMessage, value, PropertyChanged);
                this.Raise("IsError", PropertyChanged);
            } 
        }

        private IDisposable pollSubscription = null;

        private Tuple<Tournament, IEnumerable<Participant>, IEnumerable<Match>> queryData()
        {
            try
            {
                var tournament = Portal.ShowTournament(tournamentId);
                var participants = Portal.GetParticipants(tournamentId);
                var matches = Portal.GetMatches(tournamentId);

                ErrorMessage = null;

                return Tuple.Create(tournament, participants, matches);
            }
            catch (ChallongeApiException ex)
            {
                if (ex.Errors != null) ErrorMessage = ex.Errors.Aggregate((one, two) => one + "\r\n" + two);
                else ErrorMessage = string.Format("Error with ResponseStatus \"{0}\" and StatusCode \"{1}\". {2}", ex.RestResponse.ResponseStatus,
                    ex.RestResponse.StatusCode, ex.RestResponse.ErrorMessage);

                return null;
            }
        }

        public TournamentContext(ChallongePortal portal, int tournamentId)
        {
            Portal = portal;
            this.tournamentId = tournamentId;

            var queryResults = queryData();
            if (queryResults != null)
            {
                Tournament = new ObservableTournament(queryResults.Item1, this);
                Tournament.Initialize(queryResults.Item2, queryResults.Item3);
            }
        }

        public void StartSynchronization(TimeSpan timeInterval, int pollEvery)
        {
            StopSynchronization(); //Ensure polling has stopped before starting

            pollSubscription = Observable.Interval(timeInterval).Subscribe(num => 
            {
                //During a scan, either commit local changes or poll 
                if (num % pollEvery == 0) Refresh();
                else CommitChanges();
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

        /// <summary>
        /// Refresh context data
        /// </summary>
        public void Refresh()
        {
            var queryResults = queryData();

            if (queryResults != null)
            {
                if (Tournament == null)
                {
                    Tournament = new ObservableTournament(queryResults.Item1, this);
                    Tournament.Initialize(queryResults.Item2, queryResults.Item3);
                }
                else Tournament.Update(queryResults.Item1, queryResults.Item2, queryResults.Item3);
            }
        }

        public void CommitChanges()
        {
            if (Tournament != null && Tournament.Participants != null)
            {
                foreach (var p in Tournament.Participants)
                {
                    var miscDirtyable = p.Value.MiscProperties;

                    try
                    {
                        miscDirtyable.CommitIfDirty(() => Portal.SetParticipantMisc(Tournament.Id, p.Value.Id, miscDirtyable.Value.ToString()));
                    }
                    catch (Exception)
                    {
                        //If an exception is thrown trying to commit dirty data the object will stay marked as dirty and a commit will be attempted
                        //next time a scan occurs. The error itself is ignored
                    }
                }
            }
        }

        public void Dispose()
        {
            StopSynchronization();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
