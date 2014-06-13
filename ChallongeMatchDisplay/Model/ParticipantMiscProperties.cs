using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    class ParticipantMiscProperties
    {
        public bool IsMissing { get; set; }

        private ParticipantMiscProperties() { }

        public static ParticipantMiscProperties Parse(string miscString)
        {
            var result = new ParticipantMiscProperties();

            if (string.IsNullOrEmpty(miscString))
            {
                result.IsMissing = false;
            }
            else
            {
                result.IsMissing = bool.Parse(miscString);
            }

            return result;
        }

        public override string ToString()
        {
            return IsMissing.ToString();
        }
    }
}
