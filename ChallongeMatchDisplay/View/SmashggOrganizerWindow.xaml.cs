using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using Fizzi.Applications.ChallongeVisualization.Model;
using Fizzi.Applications.ChallongeVisualization.Properties;
using Fizzi.Applications.ChallongeVisualization.ViewModel;
using Fizzi.Libraries.SmashggApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.View;

public partial class SmashggOrganizerWindow : Window
{
	public bool playersSwapped;

	public SmashggOrganizerWindow()
	{
		InitializeComponent();
		MainViewModel mainViewModel = Application.Current.MainWindow.DataContext as MainViewModel;
		eventTextbox.Text = ((SmashggObservablePhaseGroup)mainViewModel.Context.Tournament).Name;
		UpdateScoreboardInterface();
		UpdateInstructions();
	}

	public void UpdateInstructions()
	{
		if (Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.showInstructions)
		{
			instructions.Visibility = Visibility.Visible;
			instructions.IsEnabled = true;
			instructions.Height = double.NaN;
		}
		else
		{
			instructions.Visibility = Visibility.Hidden;
			instructions.IsEnabled = false;
			instructions.Height = 0.0;
		}
	}

	public void UpdateScoreboardInterface()
	{
		if (Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.enableScoreboard)
		{
			scoreboard.Visibility = Visibility.Visible;
			scoreboard.IsEnabled = true;
			scoreboard.Height = double.NaN;
		}
		else
		{
			scoreboard.Visibility = Visibility.Hidden;
			scoreboard.IsEnabled = false;
			scoreboard.Height = 0.0;
		}
	}

	private void ImportStationsButton_Click(object sender, RoutedEventArgs e)
	{
		if (base.DataContext is ChallongeOrganizerViewModel challongeOrganizerViewModel)
		{
			challongeOrganizerViewModel.ImportStationFile.Execute(this);
		}
	}

	private void ExitMenu_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void ReportScoreMenuItem_Click(object sender, RoutedEventArgs e)
	{
		int num = 0;
		ChallongeDisplayMatch challongeDisplayMatch;
		if (sender is MenuItem)
		{
			MenuItem obj = sender as MenuItem;
			challongeDisplayMatch = obj.DataContext as ChallongeDisplayMatch;
			Border border = (Border)((ContextMenu)obj.Parent).PlacementTarget;
			Button button = (Button)border.TemplatedParent;
			num = ((Grid)button.Parent).Children.OfType<Button>().Select((Button b, int i) => new
			{
				Button = b,
				Index = i
			}).First(a => a.Button == button)
				.Index;
		}
		else
		{
			Button obj2 = sender as Button;
			challongeDisplayMatch = obj2.DataContext as ChallongeDisplayMatch;
			if (obj2.Name == "p2ReportScore")
			{
				num = 1;
			}
		}
		ChallongeReportScoreWindow reportScoreWindow = new ChallongeReportScoreWindow
		{
			Owner = this,
			DataContext = challongeDisplayMatch,
			Player1Victory = (num == 0)
		};
		reportScoreWindow.Player1Score = ((num == 0) ? 1 : 0);
		reportScoreWindow.Player2Score = ((num != 0) ? 1 : 0);
		if (challongeDisplayMatch != null && reportScoreWindow.ShowDialog() == true)
		{
			SetScore[] parameter = new SetScore[1] { SetScore.Create(reportScoreWindow.Player1Score, reportScoreWindow.Player2Score) };
			if (reportScoreWindow.Player1Victory)
			{
				challongeDisplayMatch.Player1WinsScored.Execute(parameter);
			}
			else
			{
				challongeDisplayMatch.Player2WinsScored.Execute(parameter);
			}
		}
	}

	private void Settings_Click(object sender, RoutedEventArgs e)
	{
		new Settings().ShowDialog();
		UpdateScoreboardInterface();
		UpdateInstructions();
	}

	private void StationEdit(object sender, DataGridCellEditEndingEventArgs e)
	{
		MessageBox.Show("Editing stations through client is currently not supported.");
	}

	private void RemoveStation_Click(object sender, RoutedEventArgs e)
	{
		MessageBox.Show("Removing stations through client is currently not supported.");
	}

	private void AddStation_Click(object sender, RoutedEventArgs e)
	{
		MessageBox.Show("Adding stations through client is currently not supported.");
	}

	private void MoveDown_Click(object sender, RoutedEventArgs e)
	{
		MessageBox.Show("Moving stations through the client is currently not supported.");
	}

	private void MoveUp_Click(object sender, RoutedEventArgs e)
	{
		MessageBox.Show("Moving stations through the client is currently not supported.");
	}

	private void SelectStation(string identifier)
	{
		foreach (SmashggStation item in (IEnumerable)StationsDataGrid.Items)
		{
			if (item.Identifier == identifier)
			{
				StationsDataGrid.SelectedItem = item;
				break;
			}
		}
	}

	private void ScoreScrollWheel(object sender, MouseWheelEventArgs e)
	{
		int num = 1;
		if (e.Delta < 0)
		{
			num = -1;
		}
		TextBox obj = e.Source as TextBox;
		int result = 0;
		int.TryParse(obj.Text, out result);
		result += num;
		if (result < 0)
		{
			result = 0;
		}
		if (result > 99)
		{
			result = 99;
		}
		obj.Text = result.ToString();
	}

	private void submitScore_Click(object sender, RoutedEventArgs e)
	{
		ChallongeOrganizerViewModel challongeOrganizerViewModel = base.DataContext as ChallongeOrganizerViewModel;
		ChallongeStationModel challongeStationModel = null;
		ChallongeDisplayMatch challongeDisplayMatch = null;
		foreach (KeyValuePair<string, ChallongeStationModel> item in ChallongeStations.Instance.Dict)
		{
			ChallongeStationModel value = item.Value;
			if (value.isPrimaryStream())
			{
				challongeStationModel = value;
			}
		}
		if (challongeStationModel != null)
		{
			foreach (ChallongeDisplayMatch openMatch in challongeOrganizerViewModel.OpenMatches)
			{
				if (openMatch.Match.IsMatchInProgress && openMatch.Match.StationAssignment == challongeStationModel.Name)
				{
					challongeDisplayMatch = openMatch;
					break;
				}
			}
			if (challongeDisplayMatch != null)
			{
				int result = 0;
				int result2 = 0;
				int.TryParse(p1Score.Text, out result);
				int.TryParse(p2Score.Text, out result2);
				if (playersSwapped)
				{
					int num = result;
					result = result2;
					result2 = num;
				}
				SetScore[] parameter = new SetScore[1] { SetScore.Create(result, result2) };
				if (result > result2)
				{
					challongeDisplayMatch.Player1WinsScored.Execute(parameter);
				}
				else if (result < result2)
				{
					challongeDisplayMatch.Player2WinsScored.Execute(parameter);
				}
				else
				{
					MessageBox.Show("Both players are tied with the same score. Increase one player's score first so a winner can be determined.", "Cannot Report Score", MessageBoxButton.OK, MessageBoxImage.Hand);
				}
			}
			else
			{
				MessageBox.Show("There is no match in progress on the primary streaming station.", "Cannot Report Score", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}
		else
		{
			MessageBox.Show("There is no primary streaming station. Please set at least one station to type Stream. The first station in the list of type Stream will be the primary streaming station.", "Cannot Report Score", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void swap_Click(object sender, RoutedEventArgs e)
	{
		swapPlayers();
	}

	public void swapPlayers()
	{
		playersSwapped = !playersSwapped;
		string text = p1Name.Text;
		p1Name.Text = p2Name.Text;
		p2Name.Text = text;
		string text2 = p1Score.Text;
		p1Score.Text = p2Score.Text;
		p2Score.Text = text2;
		if (playersSwapped)
		{
			p1NameLabel.Content = "Player 2 ";
			p2NameLabel.Content = " Player 1";
		}
		else
		{
			p1NameLabel.Content = "Player 1 ";
			p2NameLabel.Content = " Player 2";
		}
	}

	private void overlayChanged(object sender, TextChangedEventArgs e)
	{
		TextBox textBox = e.Source as TextBox;
		string file = textBox.Name;
		if (textBox.Name == "eventTextbox")
		{
			file = "event";
		}
		if (Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.outputFormat == 1)
		{
			generateOverlayXML();
		}
		else
		{
			writeTextFile(file, textBox.Text);
		}
	}

	private void generateOverlayXML()
	{
		string text = Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.overlayPath.Trim(' ', '\\', '/');
		createOverlayPath();
		text += "\\overlay.xml";
		try
		{
			using XmlWriter xmlWriter = XmlWriter.Create(text);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("Fields");
			xmlWriter.WriteElementString("p1Name", (p1Name == null) ? "" : p1Name.Text);
			xmlWriter.WriteElementString("p1Score", (p1Score == null) ? "" : p1Score.Text);
			xmlWriter.WriteElementString("p2Name", (p2Name == null) ? "" : p2Name.Text);
			xmlWriter.WriteElementString("p2Score", (p2Score == null) ? "" : p2Score.Text);
			xmlWriter.WriteElementString("round", (round == null) ? "" : round.Text);
			xmlWriter.WriteElementString("event", (eventTextbox == null) ? "" : eventTextbox.Text);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
		}
		catch (Exception arg)
		{
			Console.WriteLine("An error occurred while writing output file: '{0}'", arg);
		}
	}

	private void createOverlayPath()
	{
		string path = Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.overlayPath.Trim(' ', '\\', '/');
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			if (!Directory.Exists(path))
			{
				MessageBox.Show("The overlay output directory does not exist and could not be created.", "Cannot Output Overlay File", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("The overlay output directory does not exist and could not be created.\n\nMore information: " + ex.ToString(), "Cannot Output Overlay File", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void writeTextFile(string file, string contents)
	{
		string text = Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.overlayPath.Trim(' ', '\\', '/');
		createOverlayPath();
		text = text + "\\" + file + ".txt";
		try
		{
			File.WriteAllText(text, contents);
		}
		catch (Exception arg)
		{
			Console.WriteLine("An error occurred while writing output file: '{0}'", arg);
		}
	}

	private void hideInstructions_Click(object sender, RoutedEventArgs e)
	{
		Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.showInstructions = false;
		Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.Save();
		UpdateInstructions();
	}

	private void About_Click(object sender, RoutedEventArgs e)
	{
		new AboutView().ShowDialog();
	}

	private void endTournament_Click(object sender, RoutedEventArgs e)
	{
		(Application.Current.MainWindow.DataContext as MainViewModel).Context.EndTournament();
	}
}
