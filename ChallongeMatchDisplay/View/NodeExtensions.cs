using System.Windows.Controls;

public static class NodeExtensions
{
	public static TreeViewItem ContainerFromItemRecursive(this ItemContainerGenerator root, object item) {
		if (root.ContainerFromItem(item) is TreeViewItem result) {
			return result;
		}
		foreach (object item2 in root.Items) {
			TreeViewItem treeViewItem2 = ((root.ContainerFromItem(item2) is TreeViewItem treeViewItem) ? ContainerFromItemRecursive(treeViewItem.ItemContainerGenerator, item) : null);
			if (treeViewItem2 != null) {
				return treeViewItem2;
			}
		}
		return null;
	}
}
