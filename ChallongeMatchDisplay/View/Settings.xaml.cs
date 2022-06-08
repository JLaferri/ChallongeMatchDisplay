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
using System.Windows.Forms;
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

		public Settings() {
			InitializeComponent();
			_mvm = System.Windows.Application.Current.MainWindow.DataContext as MainViewModel;
			challongeApiKeyTextBox.Password = _mvm.ChallongeApiKey;
			challongeSubdomainTextBox.Text = _mvm.ChallongeSubdomain;
			smashggApiTokenTextBox.Password = _mvm.SmashggApiToken;
			smashggSlugTextBox.Text = _mvm.SmashggSlug;
			overlayPath.Text = Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.overlayPath;
			saveChallonge.IsChecked = Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.challonge_save;
			saveSmashgg.IsChecked = Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.smashgg_save;
			enableScoreboard.IsChecked = Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.enableScoreboard;
			showInstructions.IsChecked = Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.showInstructions;
			roundDisplay.SelectedIndex = Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.roundDisplayType;
			outputFormat.SelectedIndex = Fizzi.Applications.ChallongeVisualization.Properties.Settings.Default.outputFormat;
			Update_Challonge_Checkbox();
			Update_Scoreboard_Checkbox();
		}

		private void Save_Click(object sender, RoutedEventArgs e) {
			if (saveChallonge.IsChecked == false) {
				challongeApiKeyTextBox.Password = "";
				challongeSubdomainTextBox.Text = "";
			}
			if (saveSmashgg.IsChecked == false) {
				smashggApiTokenTextBox.Password = "";
				smashggSlugTextBox.Text = "";
			}
			Properties.Settings.Default.challonge_save = saveChallonge.IsChecked.Value;
			Properties.Settings.Default.challonge_apikey = challongeApiKeyTextBox.Password;
			Properties.Settings.Default.challonge_subdomain = challongeSubdomainTextBox.Text;
			Properties.Settings.Default.smashgg_save = saveSmashgg.IsChecked.Value;
			Properties.Settings.Default.smashgg_apitoken = smashggApiTokenTextBox.Password;
			Properties.Settings.Default.smashgg_slug = smashggSlugTextBox.Text;
			Properties.Settings.Default.overlayPath = overlayPath.Text;
			Properties.Settings.Default.enableScoreboard = enableScoreboard.IsChecked.Value;
			Properties.Settings.Default.showInstructions = showInstructions.IsChecked.Value;
			Properties.Settings.Default.roundDisplayType = roundDisplay.SelectedIndex;
			Properties.Settings.Default.outputFormat = outputFormat.SelectedIndex;
			_mvm.ChallongeApiKey = Properties.Settings.Default.challonge_apikey;
			_mvm.ChallongeSubdomain = Properties.Settings.Default.challonge_subdomain;
			_mvm.SmashggApiToken = Properties.Settings.Default.smashgg_apitoken;
			_mvm.SmashggSlug = Properties.Settings.Default.smashgg_slug;
			Properties.Settings.Default.Save();
			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void Update_Challonge_Checkbox() {
			if (saveChallonge.IsChecked == true) {
				challongeApiKeyTextBox.IsEnabled = true;
				challongeSubdomainTextBox.IsEnabled = true;
				challongeApiKeyLabel.IsEnabled = true;
				challongeSubdomainLabel.IsEnabled = true;
			} else {
				challongeApiKeyTextBox.IsEnabled = false;
				challongeSubdomainTextBox.IsEnabled = false;
				challongeApiKeyLabel.IsEnabled = false;
				challongeSubdomainLabel.IsEnabled = false;
			}
		}

		private void Update_Smashgg_Checkbox() {
			if (saveSmashgg.IsChecked == true) {
				smashggApiTokenTextBox.IsEnabled = true;
				smashggSlugTextBox.IsEnabled = true;
				smashggApiTokenLabel.IsEnabled = true;
				smashggSlugLabel.IsEnabled = true;
			} else {
				smashggApiTokenTextBox.IsEnabled = false;
				smashggSlugTextBox.IsEnabled = false;
				smashggApiTokenLabel.IsEnabled = false;
				smashggSlugLabel.IsEnabled = false;
			}
		}

		private void Update_Scoreboard_Checkbox() {
			if (enableScoreboard.IsChecked == true) {
				overlayPath.IsEnabled = true;
				overlayPathLabel.IsEnabled = true;
				browse.IsEnabled = true;
				roundDisplay.IsEnabled = true;
				roundLabel.IsEnabled = true;
				outputFormat.IsEnabled = true;
			} else {
				overlayPath.IsEnabled = false;
				overlayPathLabel.IsEnabled = false;
				browse.IsEnabled = false;
				roundLabel.IsEnabled = false;
				roundDisplay.IsEnabled = false;
				outputFormat.IsEnabled = false;
			}
		}

		private void SaveChallonge_Click(object sender, RoutedEventArgs e) {
			if (saveChallonge.IsChecked == true && new ApiWarning().ShowDialog() == false) {
				saveChallonge.IsChecked = false;
			}
			Update_Challonge_Checkbox();
		}

		private void SaveSmashgg_Click(object sender, RoutedEventArgs e) {
			if (saveSmashgg.IsChecked == true && new ApiWarning().ShowDialog() == false) {
				saveSmashgg.IsChecked = false;
			}
			Update_Smashgg_Checkbox();
		}

		private void browse_Click(object sender, RoutedEventArgs e) {
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = "Select the folder to output overlay txt files to...";
			if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				overlayPath.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private void enableScoreboard_Click(object sender, RoutedEventArgs e) {
			Update_Scoreboard_Checkbox();
		}
	}
}