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
		Tree.Focus();

		if (ViewModel.Path == "")
		{
			BrowseFolderWindow browseLeft = new() { DataContext = ViewModel, Owner = this, Title = "Select Directory to Analyze" };
			browseLeft.ShowDialog();

			if (browseLeft.DialogResult == false)
			{
				return;
			}
			ViewModel.Path = browseLeft.SelectedPath;
		}

		startTime = DateTime.UtcNow;

		ObservableCollection<FileItem> items = [];

		currentRoot = ViewModel.Path;
		if (!currentRoot.EndsWith('\\'))
		{
			currentRoot += "\\";
		}

		FileItem rootItem;
		WIN32_FIND_DATA findData = new()
		{
			dwFileAttributes = FileAttributes.Directory,
			cFileName = currentRoot.TrimEnd('\\')
		};

		rootItem = new FileItem(Path.TrimEndingDirectorySeparator(currentRoot), 1, findData)
		{
			IsExpanded = true
		};

		items.Add(rootItem);
		AnalyzeDirectory(currentRoot, rootItem.Children, 2);

		long size = 0;
		foreach (FileItem child in rootItem.Children)
		{
			size += child.Size;
			child.Parent = rootItem;
		}
		rootItem.Size = size;

		ViewModel.SizeColumnWidth = new(Utils.MeasureText(rootItem.Size.ToString("N0"), SizeColumnHeader).Width + 5);

		ViewModel.FileItems = items;

		endTime = DateTime.UtcNow;
		UpdateStatus(null, true);
	}

	private void AnalyzeDirectory(string path, ObservableCollection<FileItem> items, int level)
	{
		if (analyzeCancelled) return;
		if (path?.Length > 259) return;
		if (Directory.Exists(path) && !Utils.DirectoryAllowed(path)) return;


		if (level > 1)
		{
			//UpdateStatus(path);
		}

		foreach (FileItem fileItem in SearchDirectory(path, level))
		{
			if (analyzeCancelled)
			{
				return;
			}

			if (fileItem.IsFolder)
			{
				//fileItem.IsExpanded = true;
				{
					AnalyzeDirectory(Path.Combine(path, fileItem.Name), fileItem.Children, level + 1);

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

		IntPtr findHandle = WinApi.FindFirstFile(Path.Combine(path, "*"), out WIN32_FIND_DATA findData);

		string newPath;

		if (findHandle != WinApi.INVALID_HANDLE_VALUE)
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

	DateTime lastStatusUpdateTime = DateTime.UtcNow;
	DateTime startTime = new();
	DateTime endTime = new();
	string currentRoot;
	private void UpdateStatus(string currentPath, bool finalUpdate = false)
	{
		if (finalUpdate || (DateTime.UtcNow - lastStatusUpdateTime).TotalMilliseconds >= 500)
		{
			const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_";
			int percentageComplete;

			string status;
			if (currentPath != null)
			{
				char firstLetter = char.ToUpper(currentPath[currentRoot.Length]);
				int index = alphabet.IndexOf(firstLetter);
				percentageComplete = index == -1 ? index : (int)((float)(index / (float)alphabet.Length) * 100.0);

				status = currentPath;
			}
			else
			{
				percentageComplete = 0;

				status = TimeSpanToShortString(endTime.Subtract(startTime));
			}

			StatusBar.Text = status;
			Debug.Print("---- " + status + " " + percentageComplete);

			lastStatusUpdateTime = DateTime.UtcNow;
		}
	}

	private string TimeSpanToShortString(TimeSpan timeSpan)
	{
		if (timeSpan.TotalHours >= 1)
		{
			return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
		}
		if (timeSpan.Minutes > 0)
		{
			return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
		}
		return $"{timeSpan.Seconds}.{timeSpan.Milliseconds.ToString().PadLeft(3, '0')}s";
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
		return;

		if (!renderComplete)
			return;

		ViewModel.NameColumnWidth = new(Math.Max(LeftColumns.ColumnDefinitions[0].Width.Value, 50));
		ViewModel.SizeColumnWidth = new(Math.Max(LeftColumns.ColumnDefinitions[2].Width.Value, 50));
		ViewModel.DateColumnWidth = new(Math.Max(LeftColumns.ColumnDefinitions[4].Width.Value, 50));

		double totalWidth = 0;

		foreach (ColumnDefinition d in LeftColumns.ColumnDefinitions)
		{
			totalWidth += d.Width.Value;
		}

		ViewModel.AllColumnsWidth = totalWidth;

		HorizontalScrollbar.ViewportSize = Tree.ActualWidth;
		HorizontalScrollbar.Maximum = totalWidth - Tree.ActualWidth;
		HorizontalScrollbar.LargeChange = Tree.ActualWidth;
	}

	#endregion

	#region Event Handlers

	private void TreeGrid_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
	{
		int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
		VerticalTreeScrollbar.Value -= lines;
	}

	private void TreeColumns_Resized(object sender, SizeChangedEventArgs e)
	{
		UpdateColumnWidths();
	}

	private void TreeColumns_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
	{
		UpdateColumnWidths();
	}

	private void HorizontalScrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{

	}

	private void Tree_SelectionChanged(object sender, FileItemEventArgs e)
	{

	}

	private void Window_ContentRendered(object sender, EventArgs e)
	{
		if (Environment.GetCommandLineArgs().Length > 1)
		{
			ViewModel.Path = Environment.GetCommandLineArgs()[1];
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

	private void BrowseButton_Click(object sender, RoutedEventArgs e)
	{
		BrowseFolderWindow browseFolderWindow = new() { DataContext = ViewModel, Owner = this, SelectedPath = ViewModel.Path };
		browseFolderWindow.ShowDialog();

		if (browseFolderWindow.DialogResult == true)
		{
			ViewModel.Path = browseFolderWindow.SelectedPath;
		}
	}

	#region Commands

	private void CommandExit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		this.Close();
	}

	private void CommandAnalyze_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		Analyze();
	}

	private void CommandUp_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{

	}

	#endregion

	#endregion

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		ViewModel.SizeColumnWidth = new(100);
	}

	private void Button_Click_1(object sender, RoutedEventArgs e)
	{
		ViewModel.SizeColumnWidth = new(200);
	}

}
