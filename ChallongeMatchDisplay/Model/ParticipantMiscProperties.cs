using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    class ParticipantMiscProperties
    {
        public DateTime? UtcTimeMissing { get; set; }

        private ParticipantMiscProperties() { }

        public static ParticipantMiscProperties Parse(string miscString)
        {
            var result = new ParticipantMiscProperties();

            DateTime timeMissing;
            if (DateTime.TryParse(miscString, out timeMissing)) result.UtcTimeMissing = timeMissing;
            else result.UtcTimeMissing = null;

            return result;
        }

        public override string ToString()
        {
            return string.Format("{0}", UtcTimeMissing.HasValue ? UtcTimeMissing.ToString() : "NULL");
        }
    }
}
