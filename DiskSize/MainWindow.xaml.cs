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

	#region Event Handlers

	private void CommandExit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		this.Close();
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
