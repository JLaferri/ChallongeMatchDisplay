using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Fizzi.Applications.ChallongeVisualization.ViewModel;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.Model;
using Fizzi.Libraries.ChallongeApiWrapper;
using System.Xml;

namespace Fizzi.Applications.ChallongeVisualization.View
{
    /// <summary>
    /// Interaction logic for OrganizerWindow.xaml
    /// </summary>
    public partial class OrganizerWindow : Window
    {
		public bool playersSwapped = false;

        public OrganizerWindow()
        {
            InitializeComponent();

			MainViewModel mvm = Application.Current.MainWindow.DataContext as MainViewModel;
			eventTextbox.Text = mvm.Context.Tournament.Name;

			UpdateScoreboardInterface();
			UpdateInstructions();

			mvm.OrgViewModel.reinitializeStations(Stations.Instance.Dict.Values.ToArray<Station>());
		}

		public void UpdateInstructions()
		{
			if (Properties.Settings.Default.showInstructions)
			{
				instructions.Visibility = Visibility.Visible;
				instructions.IsEnabled = true;
				instructions.Height = Double.NaN;
			}
			else
			{
				instructions.Visibility = Visibility.Hidden;
				instructions.IsEnabled = false;
				instructions.Height = 0;
			}
		}

		public void UpdateScoreboardInterface()
		{
			if (Properties.Settings.Default.enableScoreboard)
			{
				scoreboard.Visibility = Visibility.Visible;
				scoreboard.IsEnabled = true;
				scoreboard.Height = Double.NaN;
			}
			else
			{
				scoreboard.Visibility = Visibility.Hidden;
                scoreboard.IsEnabled = false;
				scoreboard.Height = 0;
			}
		}

        private void ImportStationsButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as OrganizerViewModel;

            if (vm != null)
            {
                //try
                //{
                    vm.ImportStationFile.Execute(this);
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(this, "An error was encountered: " + ex.NewLineDelimitedMessages(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
            }
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
		{
			Close();
        }

        private void ReportScoreMenuItem_Click(object sender, RoutedEventArgs e)
        {
			DisplayMatch displayMatch;
			int index = 0;
			
			if (sender is MenuItem)
			{
				var item = sender as MenuItem;
				displayMatch = item.DataContext as DisplayMatch;

				//Find whether the player 1 panel or the player 2 panel was clicked. This is used to set victor
				var border = (Border)((ContextMenu)item.Parent).PlacementTarget;
				var button = (Button)border.TemplatedParent;
				var grid = (Grid)button.Parent;

				//Get index of button in the grid's children. If index is 0, player 1 panel was clicked, index 1 is player 2
				index = grid.Children.OfType<Button>().Select((b, i) => new { Button = b, Index = i }).First(a => a.Button == button).Index;
			}
			else
			{
				var item = sender as Button;
				displayMatch = item.DataContext as DisplayMatch;
				if (item.Name == "p2ReportScore")
					index = 1;
			}

            var reportScoreWindow = new ReportScoreWindow()
            {
                Owner = this,
                DataContext = displayMatch,
                Player1Victory = index == 0
            };

			reportScoreWindow.Player1Score = (index == 0) ? 1 : 0;
			reportScoreWindow.Player2Score = (index == 0) ? 0 : 1;

            if (displayMatch != null && reportScoreWindow.ShowDialog() == true)
            {
                var scores = new SetScore[] { SetScore.Create(reportScoreWindow.Player1Score, reportScoreWindow.Player2Score) };

                if (reportScoreWindow.Player1Victory) displayMatch.Player1WinsScored.Execute(scores);
                else displayMatch.Player2WinsScored.Execute(scores);
            }
        }

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			Settings settings = new Settings();
			settings.ShowDialog();
			UpdateScoreboardInterface();
			UpdateInstructions();
		}

		private void StationEdit(object sender, DataGridCellEditEndingEventArgs e)
		{
			Station station = (e.Row.Item as Station);
            string name = station.Name;

            if (e.Column.Header.ToString() == "Name")
			{
				if ((e.Column.GetCellContent(e.Row) as TextBox).Text.Trim() != "")
				{
					if (station.Name != (e.Column.GetCellContent(e.Row) as TextBox).Text)
					{
						if (Stations.Instance.Dict.ContainsKey((e.Column.GetCellContent(e.Row) as TextBox).Text))
						{
							MessageBox.Show("A station with that name already exists.", "Cannot Edit Station", MessageBoxButton.OK, MessageBoxImage.Error);
							(e.Column.GetCellContent(e.Row) as TextBox).Text = station.Name;
						}
						else
						{
							station.Name = (e.Column.GetCellContent(e.Row) as TextBox).Text;
							ReinitializeStations();
						}
					}
				}
				else
				{
					MessageBox.Show("Please enter a name.", "Cannot Edit Station", MessageBoxButton.OK, MessageBoxImage.Error);
					(e.Column.GetCellContent(e.Row) as TextBox).Text = station.Name;
				}
			}
			else if (e.Column.Header.ToString() == "Auto Type")
			{
				station.SetType((e.Column.GetCellContent(e.Row) as ComboBox).SelectedValue.ToString());
				ReinitializeStations();
			}
			SelectStation(name);
		}

		private void ReinitializeStations()
		{
            var vm = this.DataContext as OrganizerViewModel;
			vm.reinitializeStations(Stations.Instance.Dict.Values.ToArray<Station>());
			Stations.Instance.Save();
		}

		private void RemoveStation_Click(object sender, RoutedEventArgs e)
		{
			if (StationsDataGrid.SelectedIndex >= 0)
			{
				string name = (StationsDataGrid.SelectedItem as Station).Name;
				Stations.Instance.Delete(name);
				ReinitializeStations();
			}
			else
			{
				MessageBox.Show("Please select the station you would like to remove first.");
			}
        }

		private void AddStation_Click(object sender, RoutedEventArgs e)
		{
			NewStation newStation = new NewStation();
			newStation.ShowDialog();
		}

		private void MoveDown_Click(object sender, RoutedEventArgs e)
		{
			if (StationsDataGrid.SelectedIndex >= 0)
			{
				string name = (StationsDataGrid.SelectedItem as Station).Name;
				Stations.Instance.MoveDown(name);
				ReinitializeStations();
				SelectStation(name);
			}
			else
			{
				MessageBox.Show("Please select the station you would like to move first.");
			}
		}

		private void MoveUp_Click(object sender, RoutedEventArgs e)
		{
			if (StationsDataGrid.SelectedIndex >= 0)
			{
				string name = (StationsDataGrid.SelectedItem as Station).Name;
				Stations.Instance.MoveUp(name);
				ReinitializeStations();
				SelectStation(name);
			}
			else
			{
				MessageBox.Show("Please select the station you would like to move first.");
			}

		}

		private void SelectStation(string name)
		{
			foreach (Station station in StationsDataGrid.Items)
			{
				if (station.Name == name)
				{
					StationsDataGrid.SelectedItem = station;
					break;
				}
			}
		}

		private void ScoreScrollWheel(object sender, MouseWheelEventArgs e)
		{
			int direction = 1;
			if (e.Delta < 0) direction = -1;
			TextBox source = (e.Source as TextBox);
			int value = 0;
			int.TryParse(source.Text, out value);
			value = value + direction;
			if (value < 0) value = 0;
			if (value > 99) value = 99;
            source.Text = value.ToString();
		}

		private void submitScore_Click(object sender, RoutedEventArgs e)
		{
			var vm = this.DataContext as OrganizerViewModel;
			Station primaryStation = null;
			DisplayMatch primaryMatch = null;

			foreach (KeyValuePair<string, Station> entry in Stations.Instance.Dict)
			{
				Station station = entry.Value;
				if (station.isPrimaryStream())
					primaryStation = station;
			}

			if (primaryStation != null)
			{
				foreach (DisplayMatch match in vm.OpenMatches)
				{
					if (match.Match.IsMatchInProgress && match.Match.StationAssignment == primaryStation.Name)
					{
						primaryMatch = match;
						break;
					}
                }

				if (primaryMatch != null)
				{
					int p1ScoreValue = 0;
					int p2ScoreValue = 0;
					int.TryParse(p1Score.Text, out p1ScoreValue);
					int.TryParse(p2Score.Text, out p2ScoreValue);

					if (playersSwapped)
					{
						int tmpValue = p1ScoreValue;
						p1ScoreValue = p2ScoreValue;
						p2ScoreValue = tmpValue;
					}

					var scores = new SetScore[] { SetScore.Create(p1ScoreValue, p2ScoreValue) };

					if (p1ScoreValue > p2ScoreValue)
						primaryMatch.Player1WinsScored.Execute(scores);
					else if (p1ScoreValue < p2ScoreValue)
                        primaryMatch.Player2WinsScored.Execute(scores);
					else
						MessageBox.Show("Both players are tied with the same score. Increase one player's score first so a winner can be determined.", "Cannot Report Score", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				else
					MessageBox.Show("There is no match in progress on the primary streaming station.", "Cannot Report Score", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
				MessageBox.Show("There is no primary streaming station. Please set at least one station to type Stream. The first station in the list of type Stream will be the primary streaming station.", "Cannot Report Score", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void swap_Click(object sender, RoutedEventArgs e)
		{
			swapPlayers();
		}

		public void swapPlayers()
		{
			playersSwapped = !playersSwapped;

			string tmpName = p1Name.Text;
			p1Name.Text = p2Name.Text;
			p2Name.Text = tmpName;

			string tmpScore = p1Score.Text;
			p1Score.Text = p2Score.Text;
			p2Score.Text = tmpScore;

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
			TextBox textbox = e.Source as TextBox;
			string name = textbox.Name;
			if (textbox.Name == "eventTextbox") name = "event";

			if (Properties.Settings.Default.outputFormat == 1)
				generateOverlayXML();
			else
				writeTextFile(name, textbox.Text);
		}

		private void generateOverlayXML()
		{
			string path = Properties.Settings.Default.overlayPath.Trim(new Char[] { ' ', '\\', '/' });
			createOverlayPath();
			path += "\\overlay.xml";

			try
			{
				using (XmlWriter writer = XmlWriter.Create(path))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("Fields");
					writer.WriteElementString("p1Name", (p1Name == null) ? "" : p1Name.Text);
					writer.WriteElementString("p1Score", (p1Score == null) ? "" : p1Score.Text);
					writer.WriteElementString("p2Name", (p2Name == null) ? "" : p2Name.Text);
					writer.WriteElementString("p2Score", (p2Score == null) ? "" : p2Score.Text);
					writer.WriteElementString("round", (round == null) ? "" : round.Text);
					writer.WriteElementString("event", (eventTextbox == null) ? "" : eventTextbox.Text);
					writer.WriteEndElement();
					writer.WriteEndDocument();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("An error occurred while writing output file: '{0}'", e);
			}
		}

		private void createOverlayPath()
		{
			string path = Properties.Settings.Default.overlayPath.Trim(new Char[] { ' ', '\\', '/' });

			try
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				if (!Directory.Exists(path))
				{
					MessageBox.Show("The overlay output directory does not exist and could not be created.", "Cannot Output Overlay File", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("The overlay output directory does not exist and could not be created.\n\nMore information: " + e.ToString(), "Cannot Output Overlay File", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
		}

		private void writeTextFile(string file, string contents)
		{
			string path = Properties.Settings.Default.overlayPath.Trim(new Char[] {' ', '\\', '/'});
			createOverlayPath();
			path += "\\" + file + ".txt";

			try
			{
				File.WriteAllText(path, contents);
			}
			catch (Exception e)
			{
				Console.WriteLine("An error occurred while writing output file: '{0}'", e);
			}
		}

		private void hideInstructions_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.showInstructions = false;
			Properties.Settings.Default.Save();
			UpdateInstructions();
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			AboutView about = new AboutView();
			about.ShowDialog();
		}

		private void endTournament_Click(object sender, RoutedEventArgs e)
		{
			MainViewModel mvm = Application.Current.MainWindow.DataContext as MainViewModel;
			mvm.Context.EndTournament();
		}
	}
}
