using System;
using System.Collections.Generic;
using System.Windows;
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
using Fizzi.Applications.ChallongeVisualization.Properties;
using System.Xml.Linq;
using Fizzi.Applications.ChallongeVisualization.View;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }

        private string _mostRecentVersion;
        public string MostRecentVersion { get { return _mostRecentVersion; } set { this.RaiseAndSetIfChanged("MostRecentVersion", ref _mostRecentVersion, value, PropertyChanged); } }

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

        private bool _newVersionAvailable;
        public bool NewVersionAvailable { get { return _newVersionAvailable; } set { this.RaiseAndSetIfChanged("NewVersionAvailable", ref _newVersionAvailable, value, PropertyChanged); } }

        private bool _isBusy;
        public bool IsBusy { get { return _isBusy; } set { this.RaiseAndSetIfChanged("IsBusy", ref _isBusy, value, PropertyChanged); } }

        private bool _isVersionOutdatedVisible;
        public bool IsVersionOutdatedVisible { get { return _isVersionOutdatedVisible; } set { this.RaiseAndSetIfChanged("IsVersionOutdatedVisible", ref _isVersionOutdatedVisible, value, PropertyChanged); } }

        private string _errorMessage;
        public string ErrorMessage { get { return _errorMessage; } set { this.RaiseAndSetIfChanged("ErrorMessage", ref _errorMessage, value, PropertyChanged); } }

        public ICommand NextCommand { get; private set; }
        public ICommand Back { get; private set; }

        public ICommand IgnoreVersionNotification { get; private set; }

        public string ThreadUrl { get { return "http://smashboards.com/threads/challonge-match-display-application-helping-tournaments-run-faster.358186/"; } }

        public OrganizerViewModel OrgViewModel { get; private set; }
        
        public MainViewModel()
        {
            CurrentScreen = ScreenType.ApiKey;

			ApiKey = Properties.Settings.Default.challonge_apikey;
			Subdomain = Properties.Settings.Default.challonge_subdomain;

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

			//Modify ViewModel state when an action is initiated
			Action startAction = () =>
            {
                ErrorMessage = null;
                IsBusy = true;
            };

            //Modify ViewModel state when an action is completed
            Action endAction = () =>
            {
                IsBusy = false;
            };

            //Modify ViewModel state when an action comes back with an exception
            Action<Exception> errorHandler = ex =>
            {
                if (ex.InnerException is ChallongeApiException)
                {
                    var cApiEx = (ChallongeApiException)ex.InnerException;

                    if (cApiEx.Errors != null) ErrorMessage = cApiEx.Errors.Aggregate((one, two) => one + "\r\n" + two);
                    else ErrorMessage = string.Format("Error with ResponseStatus \"{0}\" and StatusCode \"{1}\". {2}", cApiEx.RestResponse.ResponseStatus,
                        cApiEx.RestResponse.StatusCode, cApiEx.RestResponse.ErrorMessage);
                }
                else
                {
                    ErrorMessage = ex.NewLineDelimitedMessages();
                }

                IsBusy = false;
            };

            var dispatcher = System.Threading.SynchronizationContext.Current;

            //Handle next button press
            NextCommand = Command.CreateAsync(() => true, () =>
            {
                switch (CurrentScreen)
                {
                    case ScreenType.ApiKey:
                        var subdomain = string.IsNullOrWhiteSpace(Subdomain) ? null : Subdomain;
                        Portal = new ChallongePortal(ApiKey, subdomain);

                        //Load list of tournaments that match apikey/subdomain
                        TournamentCollection = Portal.GetTournaments().OrderByDescending(t => t.CreatedAt).ToArray();

                        try
                        {
                            //This is a silly method for checking whether a new application version exists without me having my own website.
                            //I manage the most recent version number in the description of a tournament hosted on challonge. This code fetches that number
                            var versionCheckPortal = new ChallongePortal(ApiKey, "fizzitestorg");
                            MostRecentVersion = versionCheckPortal.GetTournaments().Where(t => t.Name == "CMDVersionTest").Select(t => 
                            {
                                //Modifying the description seems to put some html formatting into the result. This filters the description for
                                //just the version number by itself
                                var versionResult = string.Concat(t.Description.Where(c => char.IsDigit(c) || c == '.'));
                                return versionResult;
                            }).First();

                            //Check both version numbers to determine if current version is older than recent version
                            var versionCompareResult = Version.Split('.').Zip(MostRecentVersion.Split('.'), (v, mrv) =>
                            {
                                return int.Parse(v).CompareTo(int.Parse(mrv));
                            }).FirstOrDefault(i => i != 0);

                            //If app version is older than most recent version, show message
                            IsVersionOutdatedVisible = versionCompareResult < 0;
                        }
                        catch (Exception)
                        {
                            //If version check fails simply ignore the problem and move on
                            System.Diagnostics.Debug.WriteLine("Version check failed.");
                        }

                        break;
                    case ScreenType.TournamentSelection:
                        if (Context != null) Context.Dispose();
                        if (matchesChangedHandler != null) Context.Tournament.PropertyChanged -= matchesChangedHandler;

                        //Create tournament context from selected tournament
                        Context = new TournamentContext(Portal, SelectedTournament.Id);
                        Context.StartSynchronization(TimeSpan.FromMilliseconds(500), 6);

                        //Create TO View Model
                        OrgViewModel = new OrganizerViewModel(this, dispatcher);

                        //Load up matches into display matches. This is done to allow ordering of assigned matches over unassigned matches without having to refresh the view
                        DisplayMatches = Context.Tournament.Matches.Select(kvp => new DisplayMatch(OrgViewModel, kvp.Value, DisplayMatch.DisplayType.Assigned))
                            .Concat(Context.Tournament.Matches.Select(kvp => new DisplayMatch(OrgViewModel, kvp.Value, DisplayMatch.DisplayType.Unassigned))).ToList();

                        //This handler is used to keep matches display matches in sync with tournament context matches. If the matches in the context change, re-generate the display matches
                        matchesChangedHandler = new PropertyChangedEventHandler((sender, e) =>
                        {
                            if (e.PropertyName == "Matches")
                            {
                                if (Context.Tournament.Matches == null) DisplayMatches = null;
                                else
                                {
                                    DisplayMatches = Context.Tournament.Matches.Select(kvp => new DisplayMatch(OrgViewModel, kvp.Value, DisplayMatch.DisplayType.Assigned))
                                        .Concat(Context.Tournament.Matches.Select(kvp => new DisplayMatch(OrgViewModel, kvp.Value, DisplayMatch.DisplayType.Unassigned))).ToList();
                                }
                            }

							if (e.PropertyName == "ProgressMeter")
							{
								if (Context.Tournament.ProgressMeter == 100)
								{

								}
							}
                        });
                        Context.Tournament.PropertyChanged += matchesChangedHandler;

                        break;
                }

                CurrentScreen = (ScreenType)((int)CurrentScreen + 1);
            }, startAction, endAction, errorHandler);

            Back = Command.CreateAsync(() => true, () =>
            {
                switch (CurrentScreen)
                {
                    case ScreenType.TournamentSelection:
                        ApiKey = Properties.Settings.Default.challonge_apikey;
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
            }, startAction, endAction, errorHandler);

            IgnoreVersionNotification = Command.Create(() => true, () => IsVersionOutdatedVisible = false);


			if (ApiKey != "")
			{
				NextCommand.Execute(null);
			}
		}

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
