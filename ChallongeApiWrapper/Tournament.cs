using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fizzi.Libraries.ChallongeApiWrapper
{
    public class Tournament
    {
        public DateTime? CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }

        public int ParticipantsCount { get; set; }
        public int ProgressMeter { get; set; }

        public string State { get; set; }
        public string TournamentType { get; set; }

        public string Url { get; set; }
        public string FullChallongeUrl { get; set; }
        public string LiveImageUrl { get; set; }
    }
}
