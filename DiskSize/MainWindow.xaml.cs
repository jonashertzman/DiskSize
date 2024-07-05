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

	private readonly bool analyzeCancelled = false;

	private void Analyze()
	{
		ObservableCollection<FileItem> items = [];

		AnalyzeDirectory(@"c:\temp\", items, 1);

		ViewModel.LeftFolder = items;
	}

	private void AnalyzeDirectory(string path, ObservableCollection<FileItem> items, int level)
	{
		if (analyzeCancelled)
		{
			return;
		}

		if (path?.Length > 259)
		{
			return;
		}

		if (Directory.Exists(path) && !Utils.DirectoryAllowed(path))
		{
			return;
		}

		foreach (FileItem fileItem in SearchDirectory(path, level))
		{
			if (analyzeCancelled)
			{
				return;
			}

			if (fileItem.IsFolder)
			{
				fileItem.IsExpanded = true;
				{
					AnalyzeDirectory(Path.Combine(Utils.FixRootPath(path), fileItem.Name), fileItem.Children, level + 1);

					long size = 0;
					foreach (FileItem child in fileItem.Children)
					{
						size += child.Size;
						child.Parent = fileItem;
					}
					fileItem.Size = size;
				}
			}

			items.Add(fileItem);
		}
	}

	private List<FileItem> SearchDirectory(string path, int level)
	{
		path = Utils.FixRootPath(path);
		List<FileItem> items = [];

		IntPtr INVALID_HANDLE_VALUE = new(-1);
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

	private void CheckForNewVersion(bool forced = false)
	{
		if (AppSettings.CheckForUpdates && AppSettings.LastUpdateTime < DateTime.Now.AddDays(-5) || forced)
		{
			try
			{
				Debug.Print("Checking for new version...");

				//HttpClient httpClient = new();
				//string result = await httpClient.GetStringAsync("https://jonashertzman.github.io/zzzzzzzzzzz/download/version.txt");

				//Debug.Print($"Latest version found: {result}");
				//ViewModel.NewBuildAvailable = int.Parse(result) > int.Parse(ViewModel.BuildNumber);
			}
			catch (Exception exception)
			{
				Debug.Print($"Version check failed: {exception.Message}");
			}

			AppSettings.LastUpdateTime = DateTime.Now;
		}
	}

	private void LoadSettings()
	{
		AppSettings.LoadSettings();

		this.Left = AppSettings.PositionLeft;
		this.Top = AppSettings.PositionTop;
		this.Width = AppSettings.Width;
		this.Height = AppSettings.Height;
		this.WindowState = AppSettings.WindowState;
	}

	private void SaveSettings()
	{
		AppSettings.PositionLeft = this.Left;
		AppSettings.PositionTop = this.Top;
		AppSettings.Width = this.Width;
		AppSettings.Height = this.Height;
		AppSettings.WindowState = this.WindowState;

		AppSettings.WriteSettingsToDisk();
	}

	private void UpdateColumnWidths()
	{
		if (!renderComplete)
			return;

		ViewModel.NameColumnWidth = Math.Max(LeftColumns.ColumnDefinitions[0].Width.Value, 50);
		ViewModel.SizeColumnWidth = Math.Max(LeftColumns.ColumnDefinitions[2].Width.Value, 50);
		ViewModel.DateColumnWidth = Math.Max(LeftColumns.ColumnDefinitions[4].Width.Value, 50);

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
		Analyze();
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
		if (Environment.GetCommandLineArgs().Length > 2)
		{
			ViewModel.LeftPath = Environment.GetCommandLineArgs()[1];
			Analyze();
		}

		renderComplete = true;

		UpdateColumnWidths();
	}

	private void Window_Initialized(object sender, EventArgs e)
	{
		LoadSettings();
		CheckForNewVersion();
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		SaveSettings();
	}

	#endregion

}
