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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fizzi.Applications.ChallongeVisualization.ViewModel;

namespace Fizzi.Applications.ChallongeVisualization.View
{
    /// <summary>
    /// Interaction logic for MatchDisplayView.xaml
    /// </summary>
    public partial class MatchDisplayView : UserControl
    {
        public MatchDisplayView()
        {
            InitializeComponent();
        }

        private void PlayerBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            var solidBrush = border.Background as SolidColorBrush;
            if (solidBrush == null) return;

            if (solidBrush.Color == Colors.Red) border.Background = new SolidColorBrush(Colors.SlateBlue);
            else if (solidBrush.Color == Colors.SlateBlue) border.Background = new SolidColorBrush(Colors.Red);
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Check if control key is pressed
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var control = (Grid)sender;
                var transform = control.RenderTransform as ScaleTransform;

                var newScale = transform.ScaleX + (0.1 * (e.Delta / 120));
                control.RenderTransform = new ScaleTransform(newScale, newScale);

                e.Handled = true;
            }
        }

        OrganizerWindow organizerWindow = null;

        private void TOButton_Click(object sender, RoutedEventArgs e)
        {
            if (organizerWindow == null)
            {
                var mvm = this.DataContext as MainViewModel;
                var organizerVm = mvm.OrgViewModel;

                organizerWindow = new OrganizerWindow()
                {
                    DataContext = organizerVm
                };

                organizerWindow.Closed += (sender2, e2) =>
                {
                    organizerWindow = null;
                };

                organizerWindow.Show();
            }
            else
            {
                if (organizerWindow.WindowState == System.Windows.WindowState.Minimized)
                {
                    organizerWindow.WindowState = System.Windows.WindowState.Normal;
                }

                organizerWindow.Activate();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (organizerWindow != null)
            {
                organizerWindow.Close();
            }
        }
    }
}
