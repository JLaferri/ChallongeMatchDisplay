using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Fizzi.Applications.ChallongeVisualization.View;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class SmashggStations
{
	private static volatile SmashggStations instance;

	private static object syncRoot = new object();

	public static SmashggStations Instance
	{
		get
		{
			if (instance == null)
			{
				lock (syncRoot)
				{
					if (instance == null)
					{
						instance = new SmashggStations();
					}
				}
			}
			return instance;
		}
	}

	private SmashggStations()
	{
	}

	public void CompletionChange(bool complete, List<SmashggObservableEntrant> top2)
	{
		Application.Current.Dispatcher.Invoke(delegate
		{
			SmashggOrganizerWindow smashggOrganizerWindow = null;
			if (Application.Current.Windows.OfType<SmashggOrganizerWindow>().Count() > 0)
			{
				smashggOrganizerWindow = Application.Current.Windows.OfType<SmashggOrganizerWindow>().First();
			}
			SmashggMatchDisplayView smashggMatchDisplayView = (SmashggMatchDisplayView)Application.Current.Windows.OfType<MainWindow>().First().content.Content;
			if (complete)
			{
				if (smashggOrganizerWindow != null)
				{
					smashggOrganizerWindow.endTournament.Visibility = Visibility.Collapsed;
				}
				if (top2.Count == 2)
				{
					if (smashggOrganizerWindow != null)
					{
						smashggOrganizerWindow.round.Text = top2[0].OverlayName + " Wins!";
						smashggOrganizerWindow.p1Name.Text = "Player One";
						smashggOrganizerWindow.p2Name.Text = "Player Two";
						smashggOrganizerWindow.p1Score.Text = "0";
						smashggOrganizerWindow.p2Score.Text = "0";
					}
					smashggMatchDisplayView.Winners.Visibility = Visibility.Visible;
					smashggMatchDisplayView.winner1.Text = top2[0].OverlayName + " Wins!";
					smashggMatchDisplayView.winner2.Text = "2nd: " + top2[1].OverlayName;
					smashggMatchDisplayView.CompleteAnimation();
				}
			}
			else
			{
				if (smashggOrganizerWindow != null)
				{
					smashggOrganizerWindow.endTournament.Visibility = Visibility.Visible;
				}
				smashggMatchDisplayView.Winners.Visibility = Visibility.Collapsed;
			}
		});
	}
}
