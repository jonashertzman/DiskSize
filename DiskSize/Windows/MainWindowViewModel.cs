using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;

namespace DiskSize;

public class MainWindowViewModel : INotifyPropertyChanged
{

	#region Members

	readonly DispatcherTimer repaintTimer = new();

	#endregion

	#region Constructor

	public MainWindowViewModel()
	{
		repaintTimer.Interval = new TimeSpan(300000);
		repaintTimer.Tick += RepaintTimer_Tick;
	}

	#endregion

	#region Properties

	public string Title
	{
		get { return "File Diff"; }
	}

	public string Version
	{
		get { return "1.5"; }
	}

	public string BuildNumber
	{
		get
		{
			DateTime buildDate = new FileInfo(Environment.ProcessPath).LastWriteTime;
			return $"{buildDate:yy}{buildDate.DayOfYear:D3}";
		}
	}

	bool newBuildAvailable = false;
	public bool NewBuildAvailable
	{
		get { return newBuildAvailable; }
		set { newBuildAvailable = value; OnPropertyChanged(nameof(NewBuildAvailable)); }
	}

	public string ApplicationName
	{
		get { return $"{Title} {Version}"; }
	}

	public string FullApplicationName
	{
		get { return $"{Title} {Version} (Build {BuildNumber})"; }
	}

	public bool CheckForUpdates
	{
		get { return AppSettings.CheckForUpdates; }
		set { AppSettings.CheckForUpdates = value; OnPropertyChanged(nameof(CheckForUpdates)); }
	}

	bool guiFrozen = false;
	public bool GuiFrozen
	{
		get { return guiFrozen; }
		set
		{
			guiFrozen = value;
			if (value)
			{
				ProgressVisible = true;
			}
			else
			{
				ProgressVisible = false;
			}
			OnPropertyChanged(nameof(GuiFrozen));
		}
	}

	bool progressVisible = false;
	public bool ProgressVisible
	{
		get { return progressVisible; }
		set { progressVisible = value; OnPropertyChanged(nameof(ProgressVisible)); }
	}

	FileItem rootItem;
	public FileItem RootItem
	{
		get { return rootItem; }
		set { rootItem = value; OnPropertyChangedRepaint(nameof(RootItem)); }
	}

	public int MaxVerticalScroll
	{
		get
		{
			return Math.Max(maxLeftVerticalScroll, maxRightVerticalScroll);
		}
	}

	int maxLeftVerticalScroll;
	public int MaxLeftVerticalScroll
	{
		get { return maxLeftVerticalScroll; }
		set { maxLeftVerticalScroll = value; OnPropertyChanged(nameof(MaxLeftVerticalScroll)); OnPropertyChanged(nameof(MaxVerticalScroll)); }
	}

	int maxRightVerticalScroll;
	public int MaxRightVerticalScroll
	{
		get { return maxRightVerticalScroll; }
		set { maxRightVerticalScroll = value; OnPropertyChanged(nameof(MaxRightVerticalScroll)); OnPropertyChanged(nameof(MaxVerticalScroll)); }
	}

	int visibleLines;
	public int VisibleLines
	{
		get { return visibleLines; }
		set { visibleLines = value; OnPropertyChanged(nameof(VisibleLines)); OnPropertyChanged(nameof(MaxVerticalScroll)); }
	}

	string path = "";
	public string Path
	{
		get { return path; }
		set
		{
			path = value;
			OnPropertyChangedRepaint(nameof(Path));
		}
	}

	public double NameColumnWidth
	{
		get { return AppSettings.NameColumnWidth; }
		set { AppSettings.NameColumnWidth = value; OnPropertyChangedSlowRepaint(nameof(NameColumnWidth)); }
	}

	public double SizeColumnWidth
	{
		get { return AppSettings.SizeColumnWidth; }
		set { AppSettings.SizeColumnWidth = value; OnPropertyChangedSlowRepaint(nameof(SizeColumnWidth)); }
	}

	public double DateColumnWidth
	{
		get { return AppSettings.DateColumnWidth; }
		set { AppSettings.DateColumnWidth = value; OnPropertyChangedSlowRepaint(nameof(DateColumnWidth)); }
	}

	public double FilesColumnWidth
	{
		get { return AppSettings.FilesColumnWidth; }
		set { AppSettings.FilesColumnWidth = value; OnPropertyChangedSlowRepaint(nameof(FilesColumnWidth)); }
	}

	SortColumn sortBy = SortColumn.Size;
	public SortColumn SortBy
	{
		get { return sortBy; }
		set { sortBy = value; OnPropertyChangedRepaint(nameof(SortBy)); }
	}

	Sorting sortDirection = Sorting.Descending;
	public Sorting SortDirection
	{
		get { return sortDirection; }
		set { sortDirection = value; OnPropertyChangedRepaint(nameof(SortDirection)); }
	}



	public Themes Theme
	{
		get { return AppSettings.Theme; }
		set { AppSettings.Theme = value; OnPropertyChangedRepaint(null); } // Refresh all properties when changing theme
	}

	public bool LightTheme
	{
		get { return Theme == Themes.Light; }
	}



	// UI colors
	public Brush WindowForeground
	{
		get { return AppSettings.WindowForeground; }
		set { AppSettings.WindowForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(WindowForeground)); }
	}

	public Brush DisabledForeground
	{
		get { return AppSettings.DisabledForeground; }
		set { AppSettings.DisabledForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(DisabledForeground)); }
	}

	public Brush WindowBackground
	{
		get { return AppSettings.WindowBackground; }
		set { AppSettings.WindowBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(WindowBackground)); }
	}

	public Brush DialogBackground
	{
		get { return AppSettings.DialogBackground; }
		set { AppSettings.DialogBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(DialogBackground)); }
	}

	public Brush ControlLightBackground
	{
		get { return AppSettings.ControlLightBackground; }
		set { AppSettings.ControlLightBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(ControlLightBackground)); }
	}

	public Brush ControlDarkBackground
	{
		get { return AppSettings.ControlDarkBackground; }
		set { AppSettings.ControlDarkBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(ControlDarkBackground)); }
	}

	public Brush BorderForeground
	{
		get { return AppSettings.BorderForeground; }
		set { AppSettings.BorderForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(BorderForeground)); }
	}

	public Brush BorderDarkForeground
	{
		get { return AppSettings.BorderDarkForeground; }
		set { AppSettings.BorderDarkForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(BorderDarkForeground)); }
	}

	public Brush HighlightBackground
	{
		get { return AppSettings.HighlightBackground; }
		set { AppSettings.HighlightBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(HighlightBackground)); }
	}

	public Brush HighlightBorder
	{
		get { return AppSettings.HighlightBorder; }
		set { AppSettings.HighlightBorder = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(HighlightBorder)); }
	}

	public Brush AttentionBackground
	{
		get { return AppSettings.AttentionBackground; }
		set { AppSettings.AttentionBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(AttentionBackground)); }
	}



	int updateTrigger;
	public int UpdateTrigger
	{
		get { return updateTrigger; }
		set { updateTrigger = value; OnPropertyChanged(nameof(UpdateTrigger)); }
	}

	#endregion

	#region Events

	private void RepaintTimer_Tick(object sender, EventArgs e)
	{
		repaintTimer.Stop();
		UpdateTrigger++;
	}

	#endregion

	#region INotifyPropertyChanged

	public event PropertyChangedEventHandler PropertyChanged;

	private void OnPropertyChangedSlowRepaint(string name)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		if (!repaintTimer.IsEnabled)
		{
			repaintTimer.Start();
		}
	}

	public void OnPropertyChangedRepaint(string name)
	{
		UpdateTrigger++;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	public void OnPropertyChanged(string name)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	#endregion

}
