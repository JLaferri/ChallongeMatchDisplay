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

        public DisplayMatch(ObservableMatch match, DisplayType displayType)
        {
            Match = match;
            MatchDisplayType = displayType;

            Player1Wins = Command.Create(() => true, () => Match.ReportPlayer1Victory(SetScore.Create(1, 0)));
            Player2Wins = Command.Create(() => true, () => Match.ReportPlayer2Victory(SetScore.Create(0, 1)));

            Player1WinsScored = Command.Create<SetScore[]>(_ => true, scores => Match.ReportPlayer1Victory(scores));
            Player2WinsScored = Command.Create<SetScore[]>(_ => true, scores => Match.ReportPlayer2Victory(scores));

            Player1ToggleMissing = Command.Create(() => true, () => Match.Player1.IsMissing = !Match.Player1.IsMissing);
            Player2ToggleMissing = Command.Create(() => true, () => Match.Player2.IsMissing = !Match.Player2.IsMissing);

            AssignStation = Command.Create<Station>(_ => true, s => Match.AssignPlayersToStation(s.Name));
            CallMatchAnywhere = Command.Create(() => true, () => Match.AssignPlayersToStation("Any"));
            CallMatch = Command.Create<Station>(_ => true, s =>
            {
                if (!match.IsMatchInProgress)
                {
                    if (s != null) Match.AssignPlayersToStation(s.Name);
                    else Match.AssignPlayersToStation("Any");
                }
            });
            UncallMatch = Command.Create(() => true, () => Match.ClearStationAssignment());
        }

        public enum DisplayType
        {
            Assigned,
            Unassigned
        }
    }
}
