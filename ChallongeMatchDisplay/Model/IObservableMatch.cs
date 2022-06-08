using System.ComponentModel;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal interface IObservableMatch : INotifyPropertyChanged
{
	IObservableParticipant Winner { get; }

	IObservableParticipant Loser { get; }

	int Round { get; }
}
