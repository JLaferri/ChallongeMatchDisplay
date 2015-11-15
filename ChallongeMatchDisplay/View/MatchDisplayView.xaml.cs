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
using Fizzi.Applications.ChallongeVisualization.Model;
using System.ComponentModel;
using Fizzi.Applications.ChallongeVisualization.Common;
using System.Reactive.Linq;

namespace Fizzi.Applications.ChallongeVisualization.View
{
    /// <summary>
    /// Interaction logic for MatchDisplayView.xaml
    /// </summary>
    public partial class MatchDisplayView : UserControl, INotifyPropertyChanged
    {
        private double _textSizeRatio;
        public double TextSizeRatio { get { return _textSizeRatio; } set { this.RaiseAndSetIfChanged("TextSizeRatio", ref _textSizeRatio, value, PropertyChanged); } }

        public MatchDisplayView()
        {
            InitializeComponent();

            var vpm = Model.VisualPersistenceManager.Instance;
            TextSizeRatio = vpm.TextSizeRatio;

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "TextSizeRatio")
                {
                    foreach (var cd in HeaderRowGrid.ColumnDefinitions)
                    {
                        if (cd.Width == System.Windows.GridLength.Auto)
                        {
                            //Columns that are set to auto-width are forced to shrink and recompute their width
                            //This was done because when the text shrinks, the columns were not being properly resized
                            cd.Width = new GridLength(0);
                            cd.Width = GridLength.Auto;
                        }
                    }
                }
            };

            var propChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => this.PropertyChanged += h, h => this.PropertyChanged -= h);

            //Monitor for visual display changes and write to persistent storage in a throttled manner to limit the amount of writes done to the hard drive
            propChanged.Where(ep =>
            {
                var propName = ep.EventArgs.PropertyName;
                return propName == "TextSizeRatio";
            }).Throttle(TimeSpan.FromSeconds(5)).Subscribe(_ =>
            {
                vpm.TextSizeRatio = TextSizeRatio;

                vpm.Save();
            });
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
			//**************************Easily test the end tournament animation...
			/*
			List<ObservableParticipant> top4 = new List<ObservableParticipant>();
			var mvm2 = this.DataContext as MainViewModel;
			top4.Add(mvm2.Context.Tournament.Participants.ElementAt(0).Value);
			top4.Add(mvm2.Context.Tournament.Participants.ElementAt(1).Value);
			top4.Add(mvm2.Context.Tournament.Participants.ElementAt(2).Value);
			top4.Add(mvm2.Context.Tournament.Participants.ElementAt(3).Value);
			Stations.Instance.CompletionChange(true, top4);
			return;
			*/

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
			Winners.Visibility = Visibility.Collapsed;
			MyParticleSystem.Visibility = Visibility.Collapsed;
			MyParticleSystem.Stop();

			if (organizerWindow != null)
            {
                organizerWindow.Close();
            }
		}

		public void CompleteAnimation()
		{
			MyParticleSystem.Visibility = Visibility.Visible;
			MyParticleSystem.Start();
        }

		public event PropertyChangedEventHandler PropertyChanged;
    }
}
