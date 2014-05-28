using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fizzi.Libraries.ChallongeApiWrapper;
using Fizzi.Applications.ChallongeVisualization.Common;
using System.ComponentModel;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    class ObservableMatch : INotifyPropertyChanged
    {
        private static System.Reflection.PropertyInfo[] matchProperties = typeof(Match).GetProperties();

        private Match source;

        #region Externalize Source Properties
        public int Id { get { return source.Id; } }

        public int? Player1Id { get { return source.Player1Id; } }
        public int? Player2Id { get { return source.Player2Id; } }

        public bool Player1IsPrereqMatchLoser { get { return source.Player1IsPrereqMatchLoser; } }
        public int? Player1PrereqMatchId { get { return source.Player1PrereqMatchId; } }
        public bool Player2IsPrereqMatchLoser { get { return source.Player2IsPrereqMatchLoser; } }
        public int? Player2PrereqMatchId { get { return source.Player2PrereqMatchId; } }

        public int Round { get { return source.Round; } }
        public string State { get { return source.State; } }

        public DateTime? StartedAt { get { return source.StartedAt; } }
        #endregion

        public TournamentContext OwningContext { get; private set; }

        #region Convenience Properties
        public ObservableParticipant Player1 { get { return Player1Id.HasValue ? OwningContext.Tournament.Participants[Player1Id.Value] : null; } }
        public ObservableParticipant Player2 { get { return Player2Id.HasValue ? OwningContext.Tournament.Participants[Player2Id.Value] : null; } }

        public ObservableMatch Player1PreviousMatch { get { return Player1PrereqMatchId.HasValue ? OwningContext.Tournament.Matches[Player1PrereqMatchId.Value] : null; } }
        public ObservableMatch Player2PreviousMatch { get { return Player2PrereqMatchId.HasValue ? OwningContext.Tournament.Matches[Player2PrereqMatchId.Value] : null; } }

        public string RoundName { get { return getRoundName(); } }

        private string getRoundName()
        {
            switch (OwningContext.Tournament.TournamentType)
            {
                case "double elimination":
                    string roundText;

                    if (Round < 0)
                    {
                        if (Round == OwningContext.Tournament.MinRoundNumber) roundText = "LF";
                        else if (Round == OwningContext.Tournament.MinRoundNumber + 1) roundText = "LSF";
                        else if (Round == OwningContext.Tournament.MinRoundNumber + 2) roundText = "LQF";
                        else roundText = "L" + Math.Abs(Round);
                    }
                    else
                    {
                        if (Round == OwningContext.Tournament.MaxRoundNumber) roundText = "GF";
                        else if (Round == OwningContext.Tournament.MaxRoundNumber - 1) roundText = "WF";
                        else if (Round == OwningContext.Tournament.MaxRoundNumber - 2) roundText = "WSF";
                        else if (Round == OwningContext.Tournament.MaxRoundNumber - 3) roundText = "WQF";
                        else roundText = "W" + Round;
                    }

                    return roundText;
                default:
                    return Round.ToString();
            }
        }

        public int RoundOrder { get { return getRoundOrder(); } }

        private int getRoundOrder()
        {
            switch (OwningContext.Tournament.TournamentType)
            {
                case "double elimination":
                    return Round < 0 ? Math.Abs(Round) : Round - 1;
                default:
                    return Round;
            }
        }

        public bool IsWinners { get { return Round < 0; } }
        #endregion

        public ObservableMatch(Match match, TournamentContext context)
        {
            source = match;
            OwningContext = context;

            //Listen for when properties changed to that changed events for the convenience properties can also be fired.
            this.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Player1Id":
                        this.Raise("Player1", PropertyChanged);
                        break;
                    case "Player2Id":
                        this.Raise("Player2", PropertyChanged);
                        break;
                    case "Player1PrereqMatchId":
                        this.Raise("Player1PreviousMatch", PropertyChanged);
                        break;
                    case "Player2PrereqMatchId":
                        this.Raise("Player2PreviousMatch", PropertyChanged);
                        break;
                }
            };
        }

        public void Update(Match newData)
        {
            var oldData = source;
            source = newData;

            //Raise notify event for any property that has changed value
            foreach (var property in matchProperties)
            {
                if (property.GetValue(oldData, null) != property.GetValue(newData, null)) this.Raise(property.Name, PropertyChanged);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
