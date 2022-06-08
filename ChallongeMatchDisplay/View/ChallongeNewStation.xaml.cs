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
using Fizzi.Applications.ChallongeVisualization.ViewModel;
using Fizzi.Applications.ChallongeVisualization.Model;

namespace Fizzi.Applications.ChallongeVisualization.View
{
	/// <summary>
	/// Interaction logic for NewStation.xaml
	/// </summary>
	public partial class ChallongeNewStation : Window
	{
		public ChallongeNewStation()
		{
			InitializeComponent();

			foreach (var item in Enum.GetValues(typeof(ChallongeStationType)))
				stationType.Items.Add(item);
		}

		private void createButton_Click(object sender, RoutedEventArgs e)
		{
			ChallongeStations.Instance.Add(stationName.Text, stationType.Text);
			var vm = Application.Current.Windows.OfType<ChallongeOrganizerWindow>().First().DataContext as ChallongeOrganizerViewModel;
            vm.reinitializeStations(ChallongeStations.Instance.Dict.Values.ToArray<ChallongeStationModel>());
			Close();
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
