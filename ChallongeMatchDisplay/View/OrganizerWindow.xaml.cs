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
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Libraries.ChallongeApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.View
{
    /// <summary>
    /// Interaction logic for OrganizerWindow.xaml
    /// </summary>
    public partial class OrganizerWindow : Window
    {
        public OrganizerWindow()
        {
            InitializeComponent();
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
            var item = sender as MenuItem;
            var displayMatch = item.DataContext as DisplayMatch;

            //Find whether the player 1 panel or the player 2 panel was clicked. This is used to set victor
            var border = (Border)((ContextMenu)item.Parent).PlacementTarget;
            var button = (Button)border.TemplatedParent;
            var grid = (Grid)button.Parent;

            //Get index of button in the grid's children. If index is 0, player 1 panel was clicked, index 1 is player 2
            var index = grid.Children.OfType<Button>().Select((b, i) => new { Button = b, Index = i }).First(a => a.Button == button).Index;

            var reportScoreWindow = new ReportScoreWindow()
            {
                Owner = this,
                DataContext = item.DataContext,
                Player1Victory = index == 0
            };

            if (displayMatch != null && reportScoreWindow.ShowDialog() == true)
            {
                var scores = new SetScore[] { SetScore.Create(reportScoreWindow.Player1Score, reportScoreWindow.Player2Score) };

                if (reportScoreWindow.Player1Victory) displayMatch.Player1WinsScored.Execute(scores);
                else displayMatch.Player2WinsScored.Execute(scores);
            }
        }
    }
}
