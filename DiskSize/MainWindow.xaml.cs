using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace DiskSize;

public partial class MainWindow : Window
{

	#region Constructor

	public MainWindow()
	{
		InitializeComponent();
	}

	private bool CompareCancelled = false;
	private void MatchDirectories(string leftPath, ObservableCollection<FileItem> leftItems, string rightPath, ObservableCollection<FileItem> rightItems, int level)
	{
		if (CompareCancelled)
		{
			return;
		}

		//if (level == 2)
		//{
		//	if (leftPath != null)
		//	{
		//		progressHandler.Report(Char.ToUpper(leftPath[leftRoot.Length]) - 'A');
		//	}
		//	else if (rightPath != null)
		//	{
		//		progressHandler.Report(Char.ToUpper(rightPath[rightRoot.Length]) - 'A');
		//	}
		//}

		if (leftPath?.Length > 259 || rightPath?.Length > 259)
		{
			return;
		}

		if (Directory.Exists(leftPath) && !Utils.DirectoryAllowed(leftPath) || Directory.Exists(rightPath) && !Utils.DirectoryAllowed(rightPath))
		{
			return;
		}

		// Sorted dictionary holding matched pairs of files and folders in the current directory.
		// Folders are prefixed with "*" to not get conflict between a file named "X" to the left, and a folder named "X" to the right.
		SortedDictionary<string, FileItemPair> allItems = [];

		if (leftPath != null)
		{
			foreach (FileItem f in SearchDirectory(leftPath, level))
			{
				f.Type = TextState.Deleted;
				allItems.Add(f.Key, new FileItemPair(f, new FileItem() { IsFolder = f.IsFolder, Type = TextState.Filler, Level = level }));
			}
		}

		if (rightPath != null)
		{
			foreach (FileItem f in SearchDirectory(rightPath, level))
			{
				if (!allItems.ContainsKey(f.Key))
				{
					f.Type = TextState.New;
					allItems.Add(f.Key, new FileItemPair(new FileItem() { IsFolder = f.IsFolder, Type = TextState.Filler, Level = level }, f));
				}
				else
				{
					allItems[f.Key].RightItem = f;
					allItems[f.Key].LeftItem.Type = TextState.FullMatch;
				}
			}
		}

		foreach (KeyValuePair<string, FileItemPair> pair in allItems)
		{
			if (CompareCancelled)
			{
				return;
			}

			FileItem leftItem = pair.Value.LeftItem;
			FileItem rightItem = pair.Value.RightItem;

			leftItem.CorrespondingItem = rightItem;
			rightItem.CorrespondingItem = leftItem;

			if (leftItem.IsFolder)
			{
				//leftItem.IsExpanded = true;

				// TODO: Refactor this
				if (DirectoryIsIgnored(leftItem.Name) || DirectoryIsIgnored(rightItem.Name))
				{
					if (DirectoryIsIgnored(leftItem.Name))
					{
						leftItem.Type = TextState.Ignored;
					}
					if (DirectoryIsIgnored(rightItem.Name))
					{
						rightItem.Type = TextState.Ignored;
					}
				}
				else
				{
					MatchDirectories(leftItem.Name == "" ? null : Path.Combine(Utils.FixRootPath(leftPath), leftItem.Name), leftItem.Children, rightItem.Name == "" ? null : Path.Combine(Utils.FixRootPath(rightPath), rightItem.Name), rightItem.Children, level + 1);
					foreach (FileItem child in leftItem.Children)
					{
						child.Parent = leftItem;
					}
					foreach (FileItem child in rightItem.Children)
					{
						child.Parent = rightItem;
					}

					if (leftItem.Type == TextState.FullMatch && leftItem.ChildDiffExists)
					{
						leftItem.Type = TextState.PartialMatch;
						rightItem.Type = TextState.PartialMatch;
					}
				}
			}
			else
			{
				// TODO: Refactor this
				if (FileIsIgnored(leftItem.Name) || FileIsIgnored(rightItem.Name))
				{
					if (FileIsIgnored(leftItem.Name))
					{
						leftItem.Type = TextState.Ignored;
					}
					if (FileIsIgnored(rightItem.Name))
					{
						rightItem.Type = TextState.Ignored;
					}
				}
				else
				{
					if (leftItem.Type == TextState.FullMatch)
					{
						if (leftItem.Size != rightItem.Size || (leftItem.Date != rightItem.Date && leftItem.Checksum != rightItem.Checksum))
						{
							leftItem.Type = TextState.PartialMatch;
							rightItem.Type = TextState.PartialMatch;
						}
					}
				}
			}

			leftItems.Add(leftItem);
			rightItems.Add(rightItem);
		}
	}

	private static List<FileItem> SearchDirectory(string path, int level)
	{
		path = Utils.FixRootPath(path);
		List<FileItem> items = [];

		IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
		IntPtr findHandle = WinApi.FindFirstFile(Path.Combine(path, "*"), out WIN32_FIND_DATA findData);

		string newPath;

		if (findHandle != INVALID_HANDLE_VALUE)
		{
			do
			{
				if (findData.cFileName != "." && findData.cFileName != "..")
				{
					newPath = Path.Combine(path, findData.cFileName);
					items.Add(new FileItem(newPath, level, findData));
				}
			}
			while (WinApi.FindNextFile(findHandle, out findData));
		}

		WinApi.FindClose(findHandle);

		return items;
	}

	#endregion

	#region Event Handlers

	private void CommandExit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		this.Close();
	}

	private void CommandAnalyze_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{

	}

	private void FolderDiff_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
	{

	}

	private void LeftColumnScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
	{

	}

	private void LeftFolderHorizontalScrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{

	}

	private void LeftSide_DragDrop(object sender, DragEventArgs e)
	{

	}

	private void LeftSide_PreviewDragOver(object sender, DragEventArgs e)
	{

	}

	private void LeftFolder_SelectionChanged(FileItem file)
	{

	}

	#endregion

}
