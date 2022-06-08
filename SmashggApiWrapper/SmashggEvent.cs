using System.Collections.Generic;

namespace Fizzi.Libraries.SmashggApiWrapper;

public class SmashggEvent
{
	public long Id { get; set; }

	public string Name { get; set; }

	public int NumEntrants { get; set; }

	public List<SmashggPhase> Phases { get; set; }
}
