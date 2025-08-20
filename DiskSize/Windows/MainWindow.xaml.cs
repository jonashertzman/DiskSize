using System.IO;
using System.Windows;

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

	private void Analyze()
	{
		Debug.Print($"--- {nameof(Analyze)} ---");

		Tree.Focus();
		try
		{
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
		}
		catch (Exception e)
		{
			MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			return;
		}

		ViewModel.GuiFrozen = true;

		//ProgressBarCompare.Value = 0;
		//ProgressBarCompare.Maximum = leftLines.Count + rightLines.Count;

		BackgroundAnalyze.progressHandler = new Progress<string>(AnalyzeStatusUpdate);
		Task.Run(() => BackgroundAnalyze.Analyze(ViewModel.Path)).ContinueWith(AnalyzeFinished, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void AnalyzeStatusUpdate(string currentPath)
	{
		StatusBar.Text = currentPath;
	}

	private void AnalyzeFinished(Task<Tuple<FileItem, TimeSpan>> task)
	{
		Debug.Print($"--- {nameof(AnalyzeFinished)} ---");

		FileItem rootItem = task.Result.Item1;

		ViewModel.SizeColumnWidth = Utils.MeasureText(rootItem.Size.ToString("N0"), SizeColumnHeader).Width + 15;

		ViewModel.RootItem = rootItem;

		StatusBar.Text = TimeSpanToShortString(task.Result.Item2);
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
		if (!renderComplete)
			return;

		//double totalWidth = 0;

		//foreach (ColumnDefinition d in LeftColumns.ColumnDefinitions)
		//{
		//	totalWidth += d.Width.Value;
		//}

		//HorizontalScrollbar.ViewportSize = Tree.ActualWidth;
		//HorizontalScrollbar.Maximum = totalWidth - Tree.ActualWidth;
		//HorizontalScrollbar.LargeChange = Tree.ActualWidth;
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

		Analyze();
	}

	private void NameColumn_Click(object sender, RoutedEventArgs e)
	{
		if (ViewModel.SortBy == SortColumn.Name)
		{
			ViewModel.SortDirection = ViewModel.SortDirection == Sorting.Descending ? Sorting.Ascending : Sorting.Descending;
		}
		else
		{
			ViewModel.SortBy = SortColumn.Name;
		}
	}

	private void SizeColumn_Click(object sender, RoutedEventArgs e)
	{
		if (ViewModel.SortBy == SortColumn.Size)
		{
			ViewModel.SortDirection = ViewModel.SortDirection == Sorting.Descending ? Sorting.Ascending : Sorting.Descending;
		}
		else
		{
			ViewModel.SortBy = SortColumn.Size;
		}
	}

	private void DateColumn_Click(object sender, RoutedEventArgs e)
	{
		if (ViewModel.SortBy == SortColumn.Date)
		{
			ViewModel.SortDirection = ViewModel.SortDirection == Sorting.Descending ? Sorting.Ascending : Sorting.Descending;
		}
		else
		{
			ViewModel.SortBy = SortColumn.Date;
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
		string parent = Path.GetDirectoryName(Path.GetFullPath(ViewModel.Path));

		if (parent != null)
		{
			ViewModel.Path = parent;

			Analyze();
		}
	}

	#endregion

	#endregion

}
