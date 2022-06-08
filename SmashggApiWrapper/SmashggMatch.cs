using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fizzi.Libraries.SmashggApiWrapper;

public class SmashggMatch
{
	public string Id { get; set; }

	public List<SmashggSetSlot> Slots { get; set; }

	public int? WinnerId { get; set; }

	public int Round { get; set; }

	public SmashggMatchState State { get; set; }

	public string Identifier { get; set; }

	public string FullRoundText { get; set; }

	public SmashggStation Station { get; set; }

	[JsonConverter(typeof(UnixDateTimeConverter))]
	public DateTime? StartedAt { get; set; }

	[JsonConverter(typeof(UnixDateTimeConverter))]
	public DateTime? CreatedAt { get; set; }
}
