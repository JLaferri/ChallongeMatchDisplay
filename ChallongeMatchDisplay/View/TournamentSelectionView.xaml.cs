﻿using System;
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
    /// Interaction logic for TournamentSelectionView.xaml
    /// </summary>
    public partial class TournamentSelectionView : UserControl
    {
        public TournamentSelectionView()
        {
            InitializeComponent();
        }

		private void TournamentDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var mvm = (MainViewModel)this.DataContext;
			mvm.NextCommand.Execute(null);
		}
	}
}
