using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Fizzi.Libraries.ChallongeApiWrapper
{
    [DataContract(Name="participant")]
    public class ChallongeParticipant
    {
        [DataMember(Name="id")]
        public int Id { get; set; }
        [DataMember(Name="name")]
        public string Name { get; set; }
        [DataMember(Name="challongeUsername")]
        public string ChallongeUsername { get; set; }

        [DataMember(Name="seed")]
        public int Seed { get; set; }

        [DataMember(Name="misc")]
        public string Misc { get; set; }

        public string NameOrUsername { get { return string.IsNullOrEmpty(Name) ? ChallongeUsername : Name; } }

        [System.Runtime.Serialization.OnDeserialized]
        void OnDeserialized(System.Runtime.Serialization.StreamingContext c) {
            Misc = (Misc == null) ? "" : Misc;
        }
    }
}
