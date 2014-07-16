using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fizzi.Libraries.ChallongeApiWrapper;
using Fizzi.Applications.ChallongeVisualization.Common;
using System.ComponentModel;
using System.Reactive.Linq;

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

        public string State { get { return source.State; } }
        public string Identifier { get { return source.Identifier; } }

        public DateTime? StartedAt { get { return source.StartedAt; } }
        #endregion

        public TournamentContext OwningContext { get; private set; }

        #region Convenience Properties
        public ObservableParticipant Player1 { get { return Player1Id.HasValue ? OwningContext.Tournament.Participants[Player1Id.Value] : null; } }
        public ObservableParticipant Player2 { get { return Player2Id.HasValue ? OwningContext.Tournament.Participants[Player2Id.Value] : null; } }

        public ObservableMatch Player1PreviousMatch { get { return Player1PrereqMatchId.HasValue ? OwningContext.Tournament.Matches[Player1PrereqMatchId.Value] : null; } }
        public ObservableMatch Player2PreviousMatch { get { return Player2PrereqMatchId.HasValue ? OwningContext.Tournament.Matches[Player2PrereqMatchId.Value] : null; } }

        public int PlayerCount { get { return (new bool[] { Player1Id.HasValue, Player2Id.HasValue }).Where(b => b).Count(); } }

        //This property fixes the problem that can occur in double-elim brackets where challonge will call losers 2 losers 1 because losers 1 was all byes
        //This happens when the tournament has 5-6, 9-12, 17-24, etc players.
        public int Round { get; private set; }

        public string RoundName 
        { 
            get 
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
        }

        public int RoundOrder 
        { 
            get 
            { 
                switch (OwningContext.Tournament.TournamentType)
                {
                    case "double elimination":
                        return Round < 0 ? Math.Abs(Round) / 2 + 1 : Round;
                    default:
                        return Round;
                }
            } 
        }

        public bool IsWinners { get { return Round < 0; } }

        public string Player1SourceString
        {
            get
            {
                if (Player1PreviousMatch == null) return "N/A";

                string previousMatchCode = Player1PreviousMatch.Identifier;

                if (Player1IsPrereqMatchLoser) return "Loser of " + previousMatchCode;
                else return "Winner of " + previousMatchCode;
            }
        }

        public string Player2SourceString
        {
            get
            {
                if (Player2PreviousMatch == null) return "N/A";

                string previousMatchCode = Player2PreviousMatch.Identifier;

                if (Player2IsPrereqMatchLoser) return "Loser of " + previousMatchCode;
                else return "Winner of " + previousMatchCode;
            }
        }

        public TimeSpan? TimeSinceAvailable { get { return StartedAt.HasValue ? DateTime.Now - StartedAt.Value : default(TimeSpan?); } }

        public DateTime? TimeStationAssigned { get { return Player1 != null ? Player1.UtcTimeMatchAssigned : default(DateTime?); } }
        public string AssignedStation { get { return Player1 != null ? Player1.StationAssignment : null; } }

        public TimeSpan? TimeSinceAssigned { get { return TimeStationAssigned.HasValue ? DateTime.Now - TimeStationAssigned.Value : default(TimeSpan?); } }
        #endregion

        public ObservableMatch(Match match, TournamentContext context)
        {
            source = match;
            OwningContext = context;

            //Round doesn't change, initialize RoundFixed
            var totalPlayerCount = context.Tournament.ParticipantsCount;
            var lowerBoundExponent = Math.Floor(Math.Log(totalPlayerCount, 2));

            var lowerBound = Math.Pow(2, lowerBoundExponent);
            if (match.Round < 0 && totalPlayerCount > lowerBound && totalPlayerCount <= lowerBound + (lowerBound / 2))
            {
                Round = match.Round - 1;
            }
            else Round = match.Round;

            //Listen for when properties changed to that changed events for the convenience properties can also be fired.
            this.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Player1Id":
                        this.Raise("Player1", PropertyChanged);
                        this.Raise("PlayerCount", PropertyChanged);
                        if (Player1 != null) Player1.IsMissing = false; //When a player gets added to a match, clear their missing flag
                        break;
                    case "Player2Id":
                        this.Raise("Player2", PropertyChanged);
                        this.Raise("PlayerCount", PropertyChanged);
                        if (Player2 != null) Player2.IsMissing = false; //When a player gets added to a match, clear their missing flag
                        break;
                    case "Player1PrereqMatchId":
                        this.Raise("Player1PreviousMatch", PropertyChanged);
                        break;
                    case "Player2PrereqMatchId":
                        this.Raise("Player2PreviousMatch", PropertyChanged);
                        break;
                    case "StartedAt":
                        this.Raise("TimeSinceAvailable", PropertyChanged);
                        break;
                    case "State":
                        //Clear station assignments if match state changes
                        if (Player1 != null) Player1.ClearStationAssignment();
                        if (Player2 != null) Player2.ClearStationAssignment();
                        break;
                }
            };
        }

        public void AssignPlayersToStation(string stationName)
        {
            if (PlayerCount == 2)
            {
                //Assign players to a station
                Player1.StationAssignment = stationName;
                Player1.UtcTimeMatchAssigned = DateTime.UtcNow;

                Player2.StationAssignment = stationName;
                Player2.UtcTimeMatchAssigned = DateTime.UtcNow;
            }
        }

        public void Update(Match newData)
        {
            var oldData = source;
            source = newData;

            //Raise notify event for any property that has changed value
            foreach (var property in matchProperties)
            {
                if (!object.Equals(property.GetValue(oldData, null), property.GetValue(newData, null))) this.Raise(property.Name, PropertyChanged);
            }

            //Always raise the TimeSinceAvailable property if StatedAt is not null
            if (StartedAt.HasValue)
            {
                this.Raise("TimeSinceAvailable", PropertyChanged);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
