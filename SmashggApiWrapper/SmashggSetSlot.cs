namespace Fizzi.Libraries.SmashggApiWrapper;

public class SmashggSetSlot
{
	public string Id { get; set; }

	public SmashggEntrant Entrant { get; set; }

	public string PrereqType { get; set; }

	public string PrereqId { get; set; }

	public int? PrereqPlacement { get; set; }
}
