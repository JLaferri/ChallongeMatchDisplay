using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fizzi.Libraries.ChallongeApiWrapper
{
    public class Match
    {
        public int Id { get; set; }

        public int? Player1Id { get; set; }
        public int? Player2Id { get; set; }

        public bool Player1IsPrereqMatchLoser { get; set; }
        public int? Player1PrereqMatchId { get; set; }
        public bool Player2IsPrereqMatchLoser { get; set; }
        public int? Player2PrereqMatchId { get; set; }
        public int? WinnerId { get; set; }
        public int? LoserId { get; set; }

        public int Round { get; set; }
        public string State { get; set; }
        public string Identifier { get; set; }

        public DateTime? StartedAt { get; set; }
    }
}
