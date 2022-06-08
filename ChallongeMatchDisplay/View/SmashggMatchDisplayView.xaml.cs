using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.Model;
using Fizzi.Applications.ChallongeVisualization.View;
using Fizzi.Applications.ChallongeVisualization.ViewModel;
using PlayGround.Engine.Controls;


namespace Fizzi.Applications.ChallongeVisualization.View
{
    public partial class SmashggMatchDisplayView : UserControl, INotifyPropertyChanged
    {
        private double _textSizeRatio;

        private SmashggOrganizerWindow organizerWindow;


        public double TextSizeRatio
        {
            get
            {
                return _textSizeRatio;
            }
            set
            {
                this.RaiseAndSetIfChanged("TextSizeRatio", ref _textSizeRatio, value, this.PropertyChanged);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SmashggMatchDisplayView() {
            InitializeComponent();
            VisualPersistenceManager vpm = VisualPersistenceManager.Instance;
            TextSizeRatio = vpm.TextSizeRatio;
            PropertyChanged += delegate (object sender, PropertyChangedEventArgs e) {
                if (e.PropertyName == "TextSizeRatio") {
                    foreach (ColumnDefinition item in (IEnumerable<ColumnDefinition>)HeaderRowGrid.ColumnDefinitions) {
                        if (item.Width == GridLength.Auto) {
                            item.Width = new GridLength(0.0);
                            item.Width = GridLength.Auto;
                        }
                    }
                }
            };
            (from ep in Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(delegate (PropertyChangedEventHandler h) {
                PropertyChanged += h;
            }, delegate (PropertyChangedEventHandler h) {
                PropertyChanged -= h;
            })
             where ep.EventArgs.PropertyName == "TextSizeRatio"
             select ep).Throttle(TimeSpan.FromSeconds(5.0)).Subscribe(delegate {
                 vpm.TextSizeRatio = TextSizeRatio;
                 vpm.Save();
             });
        }

        private void PlayerBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (sender is Border border && border.Background is SolidColorBrush solidColorBrush) {
                if (solidColorBrush.Color == Colors.Red) {
                    border.Background = new SolidColorBrush(Colors.SlateBlue);
                } else if (solidColorBrush.Color == Colors.SlateBlue) {
                    border.Background = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                Grid obj = (Grid)sender;
                double num = (obj.RenderTransform as ScaleTransform).ScaleX + 0.1 * (double)(e.Delta / 120);
                obj.RenderTransform = new ScaleTransform(num, num);
                e.Handled = true;
            }
        }

        private void TOButton_Click(object sender, RoutedEventArgs e) {
            if (organizerWindow == null) {
                IOrganizerViewModel orgViewModel = (base.DataContext as MainViewModel).OrgViewModel;
                organizerWindow = new SmashggOrganizerWindow {
                    DataContext = orgViewModel
                };
                organizerWindow.Closed += delegate {
                    organizerWindow = null;
                };
                organizerWindow.Show();
            } else {
                if (organizerWindow.WindowState == WindowState.Minimized) {
                    organizerWindow.WindowState = WindowState.Normal;
                }
                organizerWindow.Activate();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            Winners.Visibility = Visibility.Collapsed;
            MyParticleSystem.Visibility = Visibility.Collapsed;
            MyParticleSystem.Stop();
            if (organizerWindow != null) {
                organizerWindow.Close();
            }
        }

        public void CompleteAnimation() {
            MyParticleSystem.Visibility = Visibility.Visible;
            MyParticleSystem.Start();
        }
    }
}
