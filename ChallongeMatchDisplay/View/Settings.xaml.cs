using System;
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
using Fizzi.Applications.ChallongeVisualization.Model;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Threading;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Libraries.ChallongeApiWrapper;
using System.IO;
using Fizzi.Applications.ChallongeVisualization.ViewModel;

namespace Fizzi.Applications.ChallongeVisualization.View
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class Settings : Window
	{
		private MainViewModel _mvm;

		public Settings()
		{
			InitializeComponent();
			
			_mvm = Application.Current.MainWindow.DataContext as MainViewModel;
			apiKeyTextBox.Password = _mvm.ApiKey;
			subdomainTextBox.Text = _mvm.Subdomain;
			overlayPath.Text = Properties.Settings.Default.overlayPath;
			saveChallonge.IsChecked = Properties.Settings.Default.challonge_save;
			enableScoreboard.IsChecked = Properties.Settings.Default.enableScoreboard;
			showInstructions.IsChecked = Properties.Settings.Default.showInstructions;
			roundDisplay.SelectedIndex = Properties.Settings.Default.roundDisplayType;
			outputFormat.SelectedIndex = Properties.Settings.Default.outputFormat;

			Update_Challonge_Checkbox();
			Update_Scoreboard_Checkbox();
        }

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			if (saveChallonge.IsChecked == false)
			{
				apiKeyTextBox.Password = "";
				subdomainTextBox.Text = "";
            }

			Properties.Settings.Default.challonge_save = (bool)saveChallonge.IsChecked;
			Properties.Settings.Default.challonge_apikey = apiKeyTextBox.Password;
			Properties.Settings.Default.challonge_subdomain = subdomainTextBox.Text;
			Properties.Settings.Default.overlayPath = overlayPath.Text;
			Properties.Settings.Default.enableScoreboard = (bool)enableScoreboard.IsChecked;
			Properties.Settings.Default.showInstructions = (bool)showInstructions.IsChecked;
			Properties.Settings.Default.roundDisplayType = roundDisplay.SelectedIndex;
			Properties.Settings.Default.outputFormat = outputFormat.SelectedIndex;

			_mvm.ApiKey = Properties.Settings.Default.challonge_apikey;
			_mvm.Subdomain = Properties.Settings.Default.challonge_subdomain;

            Properties.Settings.Default.Save();
			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Update_Challonge_Checkbox()
		{
			if (saveChallonge.IsChecked == true)
			{
				apiKeyTextBox.IsEnabled = true;
				subdomainTextBox.IsEnabled = true;
				apiKeyLabel.IsEnabled = true;
				subdomainLabel.IsEnabled = true;
			}
			else
			{
				apiKeyTextBox.IsEnabled = false;
				subdomainTextBox.IsEnabled = false;
				apiKeyLabel.IsEnabled = false;
				subdomainLabel.IsEnabled = false;
			}
		}

		private void Update_Scoreboard_Checkbox()
		{
			if (enableScoreboard.IsChecked == true)
			{
				overlayPath.IsEnabled = true;
				overlayPathLabel.IsEnabled = true;
				browse.IsEnabled = true;
				roundDisplay.IsEnabled = true;
				roundLabel.IsEnabled = true;
				outputFormat.IsEnabled = true;
			}
			else
			{
				overlayPath.IsEnabled = false;
				overlayPathLabel.IsEnabled = false;
				browse.IsEnabled = false;
				roundLabel.IsEnabled = false;
				roundDisplay.IsEnabled = false;
				outputFormat.IsEnabled = false;
			}
		}

		private void SaveChallonge_Click(object sender, RoutedEventArgs e)
		{
			if (saveChallonge.IsChecked == true)
			{
				ApiWarning dialog = new ApiWarning();
				if (dialog.ShowDialog() == false)
					saveChallonge.IsChecked = false;
			}

			Update_Challonge_Checkbox();
        }

		private void browse_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog();
			dialog.Description = "Select the folder to output overlay txt files to...";
			System.Windows.Forms.DialogResult result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				overlayPath.Text = dialog.SelectedPath;
			}
		}

		private void enableScoreboard_Click(object sender, RoutedEventArgs e)
		{
			Update_Scoreboard_Checkbox();
		}
	}
}
