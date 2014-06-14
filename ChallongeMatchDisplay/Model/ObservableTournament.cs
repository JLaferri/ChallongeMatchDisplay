using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Fizzi.Libraries.ChallongeApiWrapper;
using Fizzi.Applications.ChallongeVisualization.Common;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    class ObservableTournament : INotifyPropertyChanged
    {
        private static System.Reflection.PropertyInfo[] tournamentProperties = typeof(Tournament).GetProperties();

        private Tournament source;

        #region Externalize Source Properties
        public DateTime? CreatedAt { get { return source.CreatedAt; } }
        public DateTime? StartedAt { get { return source.StartedAt; } }
        public DateTime? CompletedAt { get { return source.CompletedAt; } }

        public string Name { get { return source.Name; } }
        public string Description { get { return source.Description; } }
        public int Id { get { return source.Id; } }

        public int ParticipantsCount { get { return source.ParticipantsCount; } }
        public int ProgressMeter { get { return source.ProgressMeter; } }

        public string State { get { return source.State; } }
        public string TournamentType { get { return source.TournamentType; } }

        public string Url { get { return source.Url; } }
        public string FullChallongeUrl { get { return source.FullChallongeUrl; } }
        public string LiveImageUrl { get { return source.LiveImageUrl; } } 
        #endregion

        private Dictionary<int, ObservableParticipant> _participants;
        public Dictionary<int, ObservableParticipant> Participants { get { return _participants; } set { this.RaiseAndSetIfChanged("Participants", ref _participants, value, PropertyChanged); } }

        private Dictionary<int, ObservableMatch> _matches;
        public Dictionary<int, ObservableMatch> Matches { get { return _matches; } set { this.RaiseAndSetIfChanged("Matches", ref _matches, value, PropertyChanged); } }

        public TournamentContext OwningContext { get; private set; }

        public int? MaxRoundNumber { get; private set; }
        public int? MinRoundNumber { get; private set; }

        public ObservableTournament(Tournament tournament, IEnumerable<Participant> playerList, IEnumerable<Match> matchList, TournamentContext context)
        {
            source = tournament;
            OwningContext = context;

            initialize(playerList, matchList);
        }

        public void initialize(IEnumerable<Participant> playerList, IEnumerable<Match> matchList)
        {
            Participants = playerList.ToDictionary(p => p.Id, p => new ObservableParticipant(p, OwningContext));
            Matches = matchList.ToDictionary(m => m.Id, m => new ObservableMatch(m, OwningContext));

            MaxRoundNumber = Matches.Select(m => (int?)m.Value.Round).DefaultIfEmpty().Max();
            MinRoundNumber = Matches.Select(m => (int?)m.Value.Round).DefaultIfEmpty().Min();
        }

        public void Update(Tournament newData, IEnumerable<Participant> playerList, IEnumerable<Match> matchList)
        {
            var oldData = source;
            source = newData;

            //Raise notify event for any property that has changed value
            foreach (var property in tournamentProperties)
            {
                if (!object.Equals(property.GetValue(oldData, null), property.GetValue(newData, null))) this.Raise(property.Name, PropertyChanged);
            }

            //Check if there are any new participants, or if participants have been removed. Also check if match count has changed.
            var participantIntersect = Participants.Select(kvp => kvp.Key).Intersect(playerList.Select(p => p.Id));
            var participantsChanged = Participants.Select(kvp => kvp.Key).Union(playerList.Select(p => p.Id)).Except(participantIntersect).Any();
            var matchCountChanged = Matches.Count != matchList.Count();

            //If true, re-initialize, else update
            if (participantsChanged || matchCountChanged)
            {
                initialize(playerList, matchList);
            }
            else
            {
                //Update all participants
                foreach (var participant in playerList) Participants[participant.Id].Update(participant);

                //Update all matches
                foreach (var match in matchList) Matches[match.Id].Update(match);
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
