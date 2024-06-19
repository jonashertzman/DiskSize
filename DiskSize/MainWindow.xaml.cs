using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DiskSize;

public partial class MainWindow : Window
{

	#region Members

	bool renderComplete = false;

	#endregion

	#region Constructor

	public MainWindow()
	{
		InitializeComponent();

		DataContext = ViewModel;
	}

	#endregion

	#region Properties

	MainWindowViewModel ViewModel { get; set; } = new();

	#endregion

	#region Methods

	private bool CompareCancelled = false;

	private void AnalyzeDirectory(string leftPath, ObservableCollection<FileItem> leftItems, int level)
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
					AnalyzeDirectory(Path.Combine(Utils.FixRootPath(leftPath), fileItem.Name), fileItem.Children, level + 1);
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


	private void UpdateColumnWidths()
	{
		if (!renderComplete)
			return;

		ViewModel.NameColumnWidth = Math.Max(LeftColumns.ColumnDefinitions[0].Width.Value, 20);
		ViewModel.SizeColumnWidth = Math.Max(LeftColumns.ColumnDefinitions[2].Width.Value, 20);
		ViewModel.DateColumnWidth = Math.Max(LeftColumns.ColumnDefinitions[4].Width.Value, 20);

		double totalWidth = 0;

		foreach (ColumnDefinition d in LeftColumns.ColumnDefinitions)
		{
			totalWidth += d.Width.Value;
		}

		LeftColumns.Width = totalWidth;
		HorizontalScrollbar.ViewportSize = LeftFolder.ActualWidth;
		HorizontalScrollbar.Maximum = totalWidth - LeftFolder.ActualWidth;
		HorizontalScrollbar.LargeChange = LeftFolder.ActualWidth;
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

		AnalyzeDirectory(@"c:\temp\", items, 1);

		ViewModel.LeftFolder = items;

	}

	private void FolderDiff_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
	{
		int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
		VerticalTreeScrollbar.Value -= lines;
	}

	private void LeftColumns_Resized(object sender, SizeChangedEventArgs e)
	{
		UpdateColumnWidths();
	}

	private void LeftColumnScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
	{
		UpdateColumnWidths();
	}

	private void LeftFolderHorizontalScrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{

	}

	private void LeftFolder_SelectionChanged(FileItem file)
	{

	}

	private void Window_ContentRendered(object sender, EventArgs e)
	{
		renderComplete = true;

		UpdateColumnWidths();
	}

	#endregion

}
