using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Fizzi.Libraries.ChallongeApiWrapper;
using System.Windows.Input;
using Fizzi.Applications.ChallongeVisualization.Common;
using System.ComponentModel;
using Fizzi.Applications.ChallongeVisualization.Model;
using System.Net;
using System.Reactive.Linq;
using System.IO;
using RestSharp;
using System.Collections.ObjectModel;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }

        private ScreenType _currentScreen;
        public ScreenType CurrentScreen { get { return _currentScreen; } set { this.RaiseAndSetIfChanged("CurrentScreen", ref _currentScreen, value, PropertyChanged); } }

        private string _apiKey;
        public string ApiKey { get { return _apiKey; } set { this.RaiseAndSetIfChanged("ApiKey", ref _apiKey, value, PropertyChanged); } }

        private string _subdomain;
        public string Subdomain { get { return _subdomain; } set { this.RaiseAndSetIfChanged("Subdomain", ref _subdomain, value, PropertyChanged); } }

        public ChallongePortal Portal { get; private set; }

        public Tournament[] TournamentCollection { get; private set; }

        private List<DisplayMatch> _displayMatches;
        public List<DisplayMatch> DisplayMatches { get { return _displayMatches; } set { this.RaiseAndSetIfChanged("DisplayMatches", ref _displayMatches, value, PropertyChanged); } }

        private Tournament _selectedTournament;
        public Tournament SelectedTournament { get { return _selectedTournament; } set { this.RaiseAndSetIfChanged("SelectedTournament", ref _selectedTournament, value, PropertyChanged); } }

        public TournamentContext Context { get; private set; }

        private PropertyChangedEventHandler matchesChangedHandler;

        private bool _isError;
        public bool IsError { get { return _isError; } set { this.RaiseAndSetIfChanged("IsError", ref _isError, value, PropertyChanged); } }

        private bool _newVersionAvailable;
        public bool NewVersionAvailable { get { return _newVersionAvailable; } set { this.RaiseAndSetIfChanged("NewVersionAvailable", ref _newVersionAvailable, value, PropertyChanged); } }

        private string _errorMessage;
        public string ErrorMessage { get { return _errorMessage; } set { this.RaiseAndSetIfChanged("ErrorMessage", ref _errorMessage, value, PropertyChanged); } }

        public ICommand NextCommand { get; private set; }
        public ICommand Back { get; private set; }

        public string ThreadUrl { get { return "http://smashboards.com/threads/challonge-match-display-application-helping-tournaments-run-faster.358186/"; } }

        public OrganizerViewModel OrgViewModel { get; private set; }
        
        public MainViewModel()
        {
            CurrentScreen = ScreenType.ApiKey;

            //Observable.Start(() =>
            //{
            //    try
            //    {
            //        //I'm considering doing an http request to smashboards to find if a new version is released. I think smashboard's anti-DDOS protection is preventing it from working
            //        WebRequest request = WebRequest.Create(ThreadUrl);
            //        request.Credentials = CredentialCache.DefaultCredentials;

            //        WebResponse response = request.GetResponse();
            //        if (((HttpWebResponse)response).StatusDescription == "OK")
            //        {
            //            Stream dataStream = response.GetResponseStream();
            //            StreamReader reader = new StreamReader(dataStream);
            //            string responseFromServer = reader.ReadToEnd();
            //            reader.Close();
            //        }
            //        response.Close();
            //    }
            //    catch { /* ignore */ }
            //});

            NextCommand = Command.Create(() => true, () =>
            {
                switch (CurrentScreen)
                {
                    case ScreenType.ApiKey:
                        var subdomain = string.IsNullOrWhiteSpace(Subdomain) ? null : Subdomain;
                        Portal = new ChallongePortal(ApiKey, subdomain);

                        try
                        {
                            TournamentCollection = Portal.GetTournaments().OrderByDescending(t => t.CreatedAt).ToArray();
                        }
                        catch (ChallongeApiException ex)
                        {
                            if (ex.Errors != null) ErrorMessage = ex.Errors.Aggregate((one, two) => one + "\r\n" + two);
                            else ErrorMessage = string.Format("Error with ResponseStatus \"{0}\" and StatusCode \"{1}\".", ex.RestResponse.ResponseStatus,
                                ex.RestResponse.StatusCode);

                            IsError = true;
                            return;
                        }
                        break;
                    case ScreenType.TournamentSelection:
                        if (Context != null) Context.Dispose();
                        if (matchesChangedHandler != null) Context.Tournament.PropertyChanged -= matchesChangedHandler;

                        Context = new TournamentContext(Portal, SelectedTournament.Id);
                        Context.StartSynchronization(TimeSpan.FromMilliseconds(500), 6);

                        //Load up matches into display matches. This is done to allow ordering of assigned matches over unassigned matches without having to refresh the view
                        DisplayMatches = Context.Tournament.Matches.Select(kvp => new DisplayMatch(kvp.Value, DisplayMatch.DisplayType.Assigned))
                            .Concat(Context.Tournament.Matches.Select(kvp => new DisplayMatch(kvp.Value, DisplayMatch.DisplayType.Unassigned))).ToList();

                        //This handler is used to keep matches display matches in sync with tournament context matches. If the matches in the context change, re-generate the display matches
                        matchesChangedHandler = new PropertyChangedEventHandler((sender, e) =>
                        {
                            if (e.PropertyName == "Matches")
                            {
                                if (Context.Tournament.Matches == null) DisplayMatches = null;
                                else
                                {
                                    DisplayMatches = Context.Tournament.Matches.Select(kvp => new DisplayMatch(kvp.Value, DisplayMatch.DisplayType.Assigned))
                                        .Concat(Context.Tournament.Matches.Select(kvp => new DisplayMatch(kvp.Value, DisplayMatch.DisplayType.Unassigned))).ToList();
                                }
                            }
                        });
                        Context.Tournament.PropertyChanged += matchesChangedHandler;

                        //Create TO View Model
                        OrgViewModel = new OrganizerViewModel(this);
                        break;
                }

                //Clear any errors that may have existed
                IsError = false;
                ErrorMessage = null;

                CurrentScreen = (ScreenType)((int)CurrentScreen + 1);
            });

            Back = Command.Create(() => true, () =>
            {
                switch (CurrentScreen)
                {
                    case ScreenType.TournamentSelection:
                        ApiKey = null;
                        break;
                    case ScreenType.PendingMatchView:
                        if (OrgViewModel != null)
                        {
                            OrgViewModel.Dispose();
                            OrgViewModel = null;
                        }
                        break;
                }
                CurrentScreen = (ScreenType)((int)CurrentScreen - 1);
            });

            
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
