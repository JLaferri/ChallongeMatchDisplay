using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Fizzi.Libraries.ChallongeApiWrapper
{
    [DataContract(Name = "tournament")]
    public class Tournament
    {
        [DataMember(Name = "createdAt")]
        public DateTime? CreatedAt { get; set; }
        [DataMember(Name = "startedAt")]
        public DateTime? StartedAt { get; set; }
        [DataMember(Name = "completedAt")]
        public DateTime? CompletedAt { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
        public string Description { get; set; }
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "participantsCount")]
        public int ParticipantsCount { get; set; }
        [DataMember(Name = "progressMeter")]
        public int ProgressMeter { get; set; }

        [DataMember(Name = "state")]
        public string State { get; set; }
        [DataMember(Name = "tournamentType")]
        public string TournamentType { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
        [DataMember(Name = "fullChallongeUrl")]
        public string FullChallongeUrl { get; set; }
        [DataMember(Name = "liveImageUrl")]
        public string LiveImageUrl { get; set; }
    }
}
