using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Fizzi.Libraries.ChallongeApiWrapper
{
    [DataContract(Name = "tournament")]
    public class ChallongeTournament
    {
        [DataMember(Name = "created_at")]
        private string CreatedAtString { get; set; }
        public DateTime? CreatedAt { get; set; }
        [DataMember(Name = "started_at")]
        private string StartedAtString { get; set; }
        public DateTime? StartedAt { get; set; }
        [DataMember(Name = "completed_at")]
        private string CompletedAtString { get; set; }
        public DateTime? CompletedAt { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
        public string Description { get; set; }
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "participants_count")]
        public int ParticipantsCount { get; set; }
        [DataMember(Name = "progress_meter")]
        public int ProgressMeter { get; set; }

        [DataMember(Name = "state")]
        public string State { get; set; }
        [DataMember(Name = "tournament_type")]
        public string TournamentType { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
        [DataMember(Name = "full_challonge_url")]
        public string FullChallongeUrl { get; set; }
        [DataMember(Name = "live_image_url")]
        public string LiveImageUrl { get; set; }

        [System.Runtime.Serialization.OnDeserialized]
        void OnDeserialized(System.Runtime.Serialization.StreamingContext c) {
            CreatedAt = DateTime.Parse(CreatedAtString);
            if (StartedAtString != null) StartedAt = DateTime.Parse(StartedAtString);
            if (CompletedAtString != null) CompletedAt = DateTime.Parse(CompletedAtString);
        }
    }
}
