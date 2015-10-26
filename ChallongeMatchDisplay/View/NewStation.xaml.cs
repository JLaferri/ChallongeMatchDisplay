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
	public partial class NewStation : Window
	{
		public NewStation()
		{
			InitializeComponent();

			foreach (var item in Enum.GetValues(typeof(StationType)))
				stationType.Items.Add(item);
		}

		private void createButton_Click(object sender, RoutedEventArgs e)
		{
			Stations.Instance.Add(stationName.Text, stationType.Text);
			var vm = Application.Current.Windows.OfType<OrganizerWindow>().First().DataContext as OrganizerViewModel;
            vm.reinitializeStations(Stations.Instance.Dict.Values.ToArray<Station>());
			Close();
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
