using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fizzi.Libraries.SmashggApiWrapper;

public class SmashggPhaseGroup
{
	public long Id { get; set; }

	public string DisplayIdentifier { get; set; }

	public SmashggPhaseGroupState State { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public SmashggBracketType BracketType { get; set; }
}
