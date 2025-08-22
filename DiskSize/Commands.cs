using System.Windows.Input;

namespace DiskSize;

public static class Commands
{

	public static readonly RoutedUICommand Exit = new(
		"Exit", nameof(Exit), typeof(Commands),
		[new KeyGesture(Key.F4, ModifierKeys.Alt)]
	);

	public static readonly RoutedUICommand About = new(
		"About", nameof(About), typeof(Commands)
	);

	public static readonly RoutedUICommand Analyze = new(
		"Analyze", nameof(Analyze), typeof(Commands),
		[new KeyGesture(Key.F5)]
	);

	public static readonly RoutedUICommand Cancel = new(
		"Cancel", nameof(Cancel), typeof(Commands),
		[new KeyGesture(Key.Escape)]
	);

	public static readonly RoutedUICommand Up = new(
		"Go to Parent Folder", nameof(Up), typeof(Commands),
		[new KeyGesture(Key.U, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand BrowseLeft = new(
		"Browse Left", nameof(BrowseLeft), typeof(Commands),
		[new KeyGesture(Key.D1, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand Options = new(
		"Options", nameof(Options), typeof(Commands)
	);

	public static readonly RoutedUICommand Find = new(
		"Find", nameof(Find), typeof(Commands),
		[new KeyGesture(Key.F, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand FindNext = new(
		"Find Next", nameof(FindNext), typeof(Commands),
		[new KeyGesture(Key.G, ModifierKeys.Control), new KeyGesture(Key.F3)]
	);

	public static readonly RoutedUICommand FindPrevious = new(
		"Find Previous", nameof(FindPrevious), typeof(Commands),
		[new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift)]
	);

	public static readonly RoutedUICommand CloseFind = new(
		"Close Find", nameof(CloseFind), typeof(Commands),
		[new KeyGesture(Key.Escape)]
	);

	public static readonly RoutedUICommand OpenContainingFolder = new(
		"Open Containing Folder", nameof(OpenContainingFolder), typeof(Commands)
	);

	public static readonly RoutedUICommand CopyPathToClipboard = new(
		"Copy Path to Clipboard", nameof(CopyPathToClipboard), typeof(Commands)
	);

}
