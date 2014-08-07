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
    }
}
