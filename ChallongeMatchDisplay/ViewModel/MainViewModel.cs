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

namespace Fizzi.Applications.ChallongeVisualization.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }

        private ScreenType _currentScreen;
        public ScreenType CurrentScreen { get { return _currentScreen; } set { this.RaiseAndSetIfChanged("CurrentScreen", ref _currentScreen, value, PropertyChanged); } }

        private string _apiKey;
        public string ApiKey { get { return _apiKey; } set { this.RaiseAndSetIfChanged("ApiKey", ref _apiKey, value, PropertyChanged); } }

        public ChallongePortal Portal { get; private set; }

        public Tournament[] TournamentCollection { get; private set; }

        private Tournament _selectedTournament;
        public Tournament SelectedTournament { get { return _selectedTournament; } set { this.RaiseAndSetIfChanged("SelectedTournament", ref _selectedTournament, value, PropertyChanged); } }

        public TournamentContext Context { get; private set; }

        private bool _isError;
        public bool IsError { get { return _isError; } set { this.RaiseAndSetIfChanged("IsError", ref _isError, value, PropertyChanged); } }

        private string _errorMessage;
        public string ErrorMessage { get { return _errorMessage; } set { this.RaiseAndSetIfChanged("ErrorMessage", ref _errorMessage, value, PropertyChanged); } }

        public ICommand NextCommand { get; private set; }
        public ICommand Back { get; private set; }

        public MainViewModel()
        {
            CurrentScreen = ScreenType.ApiKey;
            
            NextCommand = Command.Create(() => true, () =>
            {
                switch (CurrentScreen)
                {
                    case ScreenType.ApiKey:
                        Portal = new ChallongePortal(ApiKey);

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
                        Context = new TournamentContext(Portal, SelectedTournament.Id);
                        Context.StartPolling(TimeSpan.FromSeconds(3));
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
                }
                CurrentScreen = (ScreenType)((int)CurrentScreen - 1);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
