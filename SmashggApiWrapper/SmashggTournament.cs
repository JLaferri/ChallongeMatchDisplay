using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fizzi.Libraries.SmashggApiWrapper;

public class SmashggTournament
{
	[JsonConverter(typeof(UnixDateTimeConverter))]
	public DateTime CreatedAt { get; set; }

	[JsonConverter(typeof(UnixDateTimeConverter))]
	public DateTime StartAt { get; set; }

	public string Name { get; set; }

	public long Id { get; set; }

	public SmashggTournamentState State { get; set; }

	public SmashggTournamentType TournamentType { get; set; }

	public string Url { get; set; }

	public List<SmashggEvent> Events { get; set; }
}
