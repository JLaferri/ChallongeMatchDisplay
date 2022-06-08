using System.Collections.Generic;
using System.ComponentModel;
using Fizzi.Applications.ChallongeVisualization.Common;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class ChallongeStationModel : INotifyPropertyChanged
{
	private ChallongeStationStatus _status;

	public string Name { get; set; }

	public int Order { get; set; }

	public ChallongeStationStatus Status
	{
		get
		{
			return _status;
		}
		set
		{
			this.RaiseAndSetIfChanged("Status", ref _status, value, this.PropertyChanged);
		}
	}

	public ChallongeStationType Type { get; set; }

	public event PropertyChangedEventHandler PropertyChanged;

	public void SetType(string stationText)
	{
		stationText = stationText.ToLower().Trim();
		ChallongeStationType type = ChallongeStationType.Standard;
		switch (stationText)
		{
		case "stream":
			type = ChallongeStationType.Stream;
			break;
		case "recording":
			type = ChallongeStationType.Recording;
			break;
		case "premium":
			type = ChallongeStationType.Premium;
			break;
		case "backup":
			type = ChallongeStationType.Backup;
			break;
		case "noassign":
			type = ChallongeStationType.NoAssign;
			break;
		}
		Type = type;
	}

	public ChallongeStationModel(string name, int order)
		: this(name, order, ChallongeStationType.Standard)
	{
	}

	public ChallongeStationModel(string name, int order, ChallongeStationType type)
	{
		Name = name;
		Order = order;
		Type = type;
		Status = ChallongeStationStatus.Open;
	}

	public bool isPrimaryStream()
	{
		if (Type == ChallongeStationType.Stream)
		{
			foreach (KeyValuePair<string, ChallongeStationModel> item in ChallongeStations.Instance.Dict)
			{
				if (item.Value.Type == ChallongeStationType.Stream)
				{
					if (item.Key == Name)
					{
						return true;
					}
					return false;
				}
			}
		}
		return false;
	}
}
