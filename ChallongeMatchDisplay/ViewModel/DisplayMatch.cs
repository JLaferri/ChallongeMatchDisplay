using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fizzi.Applications.ChallongeVisualization.Model;

namespace Fizzi.Applications.ChallongeVisualization.ViewModel
{
    class DisplayMatch
    {
        public ObservableMatch Match { get; private set; }
        public DisplayType MatchDisplayType { get; private set; }

        public DisplayMatch(ObservableMatch match, DisplayType displayType)
        {
            Match = match;
            MatchDisplayType = displayType;
        }

        public enum DisplayType
        {
            Assigned,
            Unassigned
        }
    }
}
