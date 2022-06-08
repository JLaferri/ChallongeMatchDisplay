using System.Collections.Generic;

namespace Fizzi.Libraries.SmashggApiWrapper;

public class SmashggEntrant
{
	public int Id { get; set; }

	public string Name { get; set; }

	public List<SmashggParticipant> Participants { get; set; }
}
