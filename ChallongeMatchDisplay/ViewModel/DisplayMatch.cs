using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fizzi.Applications.ChallongeVisualization.Model;
using System.Windows.Input;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Libraries.ChallongeApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel
{
    class DisplayMatch
    {
        public ObservableMatch Match { get; private set; }
        public DisplayType MatchDisplayType { get; private set; }

        public ICommand Player1Wins { get; private set; }
        public ICommand Player2Wins { get; private set; }

        public ICommand Player1WinsScored { get; private set; }
        public ICommand Player2WinsScored { get; private set; }

        public ICommand Player1ToggleMissing { get; private set; }
        public ICommand Player2ToggleMissing { get; private set; }

        public ICommand AssignStation { get; private set; }
        public ICommand CallMatchAnywhere { get; private set; }
        public ICommand CallMatch { get; private set; }
        public ICommand UncallMatch { get; private set; }

        public DisplayMatch(OrganizerViewModel ovm, ObservableMatch match, DisplayType displayType)
        {
            Match = match;
            MatchDisplayType = displayType;

            //Modify ViewModel state when an action is initiated
            Action startAction = () =>
            {
                ovm.ErrorMessage = null;
                ovm.IsBusy = true;
            };

            //Modify ViewModel state when an action is completed
            Action endAction = () =>
            {
                ovm.IsBusy = false;
            };

            //Modify ViewModel state when an action comes back with an exception
            Action<Exception> errorHandler = ex =>
            {
                if (ex.InnerException is ChallongeApiException)
                {
                    var cApiEx = (ChallongeApiException)ex.InnerException;

                    if (cApiEx.Errors != null) ovm.ErrorMessage = cApiEx.Errors.Aggregate((one, two) => one + "\r\n" + two);
                    else ovm.ErrorMessage = string.Format("Error with ResponseStatus \"{0}\" and StatusCode \"{1}\". {2}", cApiEx.RestResponse.ResponseStatus,
                        cApiEx.RestResponse.StatusCode, cApiEx.RestResponse.ErrorMessage);
                }
                else
                {
                    ovm.ErrorMessage = ex.NewLineDelimitedMessages();
                }

                ovm.IsBusy = false;
            };

            Player1Wins = Command.CreateAsync(() => true, () => Match.ReportPlayer1Victory(SetScore.Create(1, 0)), startAction, endAction, errorHandler);
            Player2Wins = Command.CreateAsync(() => true, () => Match.ReportPlayer2Victory(SetScore.Create(0, 1)), startAction, endAction, errorHandler);

            Player1WinsScored = Command.CreateAsync<SetScore[]>(_ => true, scores => Match.ReportPlayer1Victory(scores), _ => startAction(), _ => endAction(), (_, ex) => errorHandler(ex));
            Player2WinsScored = Command.CreateAsync<SetScore[]>(_ => true, scores => Match.ReportPlayer2Victory(scores), _ => startAction(), _ => endAction(), (_, ex) => errorHandler(ex));

            Player1ToggleMissing = Command.CreateAsync(() => true, () => Match.Player1.IsMissing = !Match.Player1.IsMissing, startAction, endAction, errorHandler);
            Player2ToggleMissing = Command.CreateAsync(() => true, () => Match.Player2.IsMissing = !Match.Player2.IsMissing, startAction, endAction, errorHandler);

            AssignStation = Command.CreateAsync<Station>(_ => true, s => Match.AssignPlayersToStation(s.Name), _ => startAction(), _ => endAction(), (_, ex) => errorHandler(ex));
            CallMatchAnywhere = Command.CreateAsync(() => true, () => Match.AssignPlayersToStation("Any"), startAction, endAction, errorHandler);
            CallMatch = Command.CreateAsync<Station>(_ => true, s =>
            {
                if (!match.IsMatchInProgress)
                {
                    if (s != null) Match.AssignPlayersToStation(s.Name);
                    else Match.AssignPlayersToStation("Any");
                }
            }, _ => startAction(), _ => endAction(), (_, ex) => errorHandler(ex));
            UncallMatch = Command.CreateAsync(() => true, () => Match.ClearStationAssignment(), startAction, endAction, errorHandler);
        }

        public enum DisplayType
        {
            Assigned,
            Unassigned
        }
    }
}
