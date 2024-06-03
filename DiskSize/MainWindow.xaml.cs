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

	#endregion

	#region Properties

	MainWindowViewModel ViewModel { get; set; } = new();

	#endregion

	#region Methods

	private bool CompareCancelled = false;
	private void MatchDirectories(string leftPath, ObservableCollection<FileItem> leftItems, int level)
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

		if (leftPath?.Length > 259)
		{
			return;
		}

		if (Directory.Exists(leftPath) && !Utils.DirectoryAllowed(leftPath))
		{
			return;
		}

		List<FileItem> allItems = [];

		if (leftPath != null)
		{
			foreach (FileItem f in SearchDirectory(leftPath, level))
			{
				allItems.Add(new FileItem() { Name = f.Name, IsFolder = f.IsFolder, Level = level });
			}
		}


		foreach (FileItem fileItem in allItems)
		{
			if (CompareCancelled)
			{
				return;
			}

			if (fileItem.IsFolder)
			{
				//leftItem.IsExpanded = true;
				{
					MatchDirectories(Path.Combine(Utils.FixRootPath(leftPath), fileItem.Name), fileItem.Children, level + 1);
					foreach (FileItem child in fileItem.Children)
					{
						child.Parent = fileItem;
					}
				}
			}

			leftItems.Add(fileItem);
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
		ObservableCollection<FileItem> items = [];

		MatchDirectories(@"c:\temp\", items, 0);

		ViewModel.LeftFolder = items;

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
