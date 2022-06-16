using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fizzi.Libraries.ChallongeApiWrapper
{
    [DataContract(Name = "match")]
    public class ChallongeMatch
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "player1_id")]
        public int? Player1Id { get; set; }
        [DataMember(Name = "player2_id")]
        public int? Player2Id { get; set; }

        [DataMember(Name = "player1_is_prereq_match_loser")]
        public bool Player1IsPrereqMatchLoser { get; set; }
        [DataMember(Name = "player1_prereq_match_id")]
        public int? Player1PrereqMatchId { get; set; }
        [DataMember(Name = "player2_is_prereq_match_loser")]
        public bool Player2IsPrereqMatchLoser { get; set; }
        [DataMember(Name = "player2_prereq_match_id")]
        public int? Player2PrereqMatchId { get; set; }
        [DataMember(Name = "winner_id")]
        public int? WinnerId { get; set; }
        [DataMember(Name = "loser_id")]
        public int? LoserId { get; set; }

        [DataMember(Name = "round")]
        public int Round { get; set; }
        [DataMember(Name = "state")]
        public string State { get; set; }
        [DataMember(Name = "identifier")]
        public string Identifier { get; set; }

        [DataMember(Name = "started_at")]
        private string StartedAtString { get; set; }

        public DateTime? StartedAt { get; set; }

        [System.Runtime.Serialization.OnDeserialized]
        void OnDeserialized(System.Runtime.Serialization.StreamingContext c) {
            StartedAt = DateTime.Parse(StartedAtString);
        }
    }
}
