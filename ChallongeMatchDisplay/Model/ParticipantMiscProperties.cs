using System;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class ParticipantMiscProperties
{
	public DateTime? UtcTimeMissing { get; private set; }

	public DateTime? UtcTimeMatchAssigned { get; private set; }

	public string StationAssignment { get; private set; }

	private ParticipantMiscProperties()
	{
	}

	public ParticipantMiscProperties(DateTime? utcTimeMissing, DateTime? utcTimeMatchAssigned, string stationAssignment)
	{
		UtcTimeMissing = utcTimeMissing;
		UtcTimeMatchAssigned = utcTimeMatchAssigned;
		StationAssignment = stationAssignment;
	}

	public static ParticipantMiscProperties Parse(string miscString)
	{
		ParticipantMiscProperties participantMiscProperties = new ParticipantMiscProperties();
		string[] array = miscString.Split(';');
		if (array.Length == 3)
		{
			if (DateTime.TryParse(array[0], out var result))
			{
				participantMiscProperties.UtcTimeMissing = result;
			}
			if (DateTime.TryParse(array[1], out result))
			{
				participantMiscProperties.UtcTimeMatchAssigned = result;
			}
			if (array[2] != "{NULL}")
			{
				participantMiscProperties.StationAssignment = array[2];
			}
		}
		return participantMiscProperties;
	}

	public override string ToString()
	{
		return string.Format("{0};{1};{2}", UtcTimeMissing.HasValue ? UtcTimeMissing.ToString() : "{NULL}", UtcTimeMatchAssigned.HasValue ? UtcTimeMatchAssigned.ToString() : "{NULL}", (StationAssignment != null) ? StationAssignment.ToString() : "{NULL}");
	}
}
