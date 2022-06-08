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
using System.ComponentModel;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.ViewModel;

namespace Fizzi.Applications.ChallongeVisualization.View
{
    /// <summary>
    /// Interaction logic for ReportScoreWindow.xaml
    /// </summary>
    public partial class ChallongeReportScoreWindow : Window, INotifyPropertyChanged
    {
        private int _player1Score;
        public int Player1Score
        {
            get { return _player1Score; }
            set
            {
                _player1Score = value;
                updateConfirmationMessage();
            }
        }

        private int _player2Score;
        public int Player2Score
        {
            get { return _player2Score; }
            set
            {
                _player2Score = value;
                updateConfirmationMessage();
            }
        }

        //True when player 1 wins, False when player 2 wins
        private bool _player1Victory;
        public bool Player1Victory 
        { 
            get { return _player1Victory; } 
            set 
            { 
                this.RaiseAndSetIfChanged("Player1Victory", ref _player1Victory, value, PropertyChanged);
                updateConfirmationMessage();
            } 
        }

        //Confirmation message
        private string _confirmationMessage;
        public string ConfirmationMessage { get { return _confirmationMessage; } set { this.RaiseAndSetIfChanged("ConfirmationMessage", ref _confirmationMessage, value, PropertyChanged); } }

        public ChallongeReportScoreWindow()
        {
            InitializeComponent();
        }

        private void updateConfirmationMessage()
        {
            var match = (ChallongeDisplayMatch)this.DataContext;

            if (Player1Victory) ConfirmationMessage = string.Format("{0} wins {1}-{2}", match.Match.Player1.Name, Player1Score, Player2Score);
            else ConfirmationMessage = string.Format("{0} wins {1}-{2}", match.Match.Player2.Name, Player2Score, Player1Score);
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            //If scores don't make sense, warn the user
            if ((Player1Score >= Player2Score && !Player1Victory) || (Player2Score >= Player1Score && Player1Victory))
            {
                var result = MessageBox.Show(this, "The player marked as the victor does not appear to have a higher score than the loser. Report match as is?", "Warning", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No) return;
            }

            //Scores have been decided
            DialogResult = true;
            Close();
        }

        private void Player1WinsButton_Click(object sender, RoutedEventArgs e)
        {
            Player1Victory = true;
        }

        private void Player2WinsButton_Click(object sender, RoutedEventArgs e)
        {
            Player1Victory = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

		private void ScoreScrollWheel(object sender, MouseWheelEventArgs e)
		{
			int direction = 1;
			if (e.Delta < 0) direction = -1;
			TextBox source = (e.Source as TextBox);
			adjustScore(source, direction);
		}

		private void adjustScore(TextBox source, int adjustment)
		{
			int value = 0;
			int.TryParse(source.Text, out value);
			value = value + adjustment;
			if (value < 0) value = 0;
			if (value > 99) value = 99;
			source.Text = value.ToString();
		}

		private void p1Left_Click(object sender, RoutedEventArgs e)
		{
			adjustScore(player1Score, -1);
		}

		private void p1Right_Click(object sender, RoutedEventArgs e)
		{
			adjustScore(player1Score, 1);
		}

		private void p2Left_Click(object sender, RoutedEventArgs e)
		{
			adjustScore(player2Score, -1);
		}

		private void p2Right_Click(object sender, RoutedEventArgs e)
		{
			adjustScore(player2Score, 1);
		}
	}
}
