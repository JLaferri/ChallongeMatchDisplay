using System;
using System.ComponentModel;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal interface ITournamentContext : IDisposable, INotifyPropertyChanged
{
	IObservableTournament Tournament { get; }

	void EndTournament();

	void StartSynchronization(TimeSpan timeInterval, int pollEvery);
}
