using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    class ParticipantMiscProperties
    {
        public DateTime? UtcTimeMissing { get; private set; }
        public DateTime? UtcTimeMatchAssigned { get; private set; }
        public string StationAssignment { get; private set; }

        private ParticipantMiscProperties() { }

        public ParticipantMiscProperties(DateTime? utcTimeMissing, DateTime? utcTimeMatchAssigned, string stationAssignment)
        {
            UtcTimeMissing = utcTimeMissing;
            UtcTimeMatchAssigned = utcTimeMatchAssigned;
            StationAssignment = stationAssignment;
        }

        public static ParticipantMiscProperties Parse(string miscString)
        {
            var result = new ParticipantMiscProperties();

            var splitString = miscString.Split(';');

            if (splitString.Length == 3)
            {
                DateTime dateTimeBuffer;

                if (DateTime.TryParse(splitString[0], out dateTimeBuffer)) result.UtcTimeMissing = dateTimeBuffer;
                if (DateTime.TryParse(splitString[1], out dateTimeBuffer)) result.UtcTimeMatchAssigned = dateTimeBuffer;
                if (splitString[2] != "{NULL}") result.StationAssignment = splitString[2];
            }

            return result;
        }

        public override string ToString()
        {
            return string.Format("{0};{1};{2}", UtcTimeMissing.HasValue ? UtcTimeMissing.ToString() : "{NULL}",
                UtcTimeMatchAssigned.HasValue ? UtcTimeMatchAssigned.ToString() : "{NULL}",
                StationAssignment != null ? StationAssignment.ToString() : "{NULL}");
        }
    }
}
