using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Applications.ChallongeVisualization.ViewModel;
using Fizzi.Libraries.SmashggApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.View;

public partial class SmashggEventPhaseGroupSelectionView : UserControl, INotifyPropertyChanged
{
	private bool _phaseGroupItemSelected;

	public bool PhaseGroupItemSelected
	{
		get
		{
			return _phaseGroupItemSelected;
		}
		set
		{
			if (value)
			{
				((MainViewModel)base.DataContext).SmashggSelectedEventPhaseGroupData = GetSelectedEventPhaseGroupData();
				this.RaiseAndSetIfChanged("PhaseGroupItemSelected", ref _phaseGroupItemSelected, value, this.PropertyChanged);
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public SmashggEventPhaseGroupSelectionView()
	{
		InitializeComponent();
	}

	private void PhaseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		((MainViewModel)base.DataContext).SmashggNextCommand.Execute(null);
	}

	public ItemsControl GetSelectedTreeViewItemParent(TreeViewItem item)
	{
		DependencyObject parent = VisualTreeHelper.GetParent(item);
		while (!(parent is TreeViewItem) && !(parent is TreeView))
		{
			parent = VisualTreeHelper.GetParent(parent);
		}
		return parent as ItemsControl;
	}

	private Tuple<long, long, long> GetSelectedEventPhaseGroupData()
	{
		TreeViewItem treeViewItem = (TreeViewItem)GetSelectedTreeViewItemParent(treeView.ItemContainerGenerator.ContainerFromItemRecursive(treeView.SelectedItem));
		TreeViewItem item = (TreeViewItem)GetSelectedTreeViewItemParent(treeViewItem);
		ItemsControl selectedTreeViewItemParent = GetSelectedTreeViewItemParent(item);
		long id = ((SmashggPhaseGroup)treeView.SelectedItem).Id;
		long id2 = ((SmashggPhase)treeViewItem.DataContext).Id;
		long id3 = ((SmashggTournament)selectedTreeViewItemParent.DataContext).Id;
		return new Tuple<long, long, long>(id, id2, id3);
	}
}
