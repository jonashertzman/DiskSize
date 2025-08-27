using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace DiskSize;

static class Utils
{

	const int WS_MAXIMIZEBOX = 0x10000;
	const int WS_MINIMIZEBOX = 0x20000;

	public static bool DirectoryAllowed(string path)
	{
		try
		{
			Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
		}
		catch
		{
			return false;
		}
		return true;
	}

	public static void HideMinimizeAndMaximizeButtons(Window window)
	{
		window.SourceInitialized += (sender, eventArgs) =>
		{
			HWND hwnd = (HWND)new System.Windows.Interop.WindowInteropHelper(window).Handle;
			int style = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
			_ = PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
		};
	}

	public static string TimeSpanToShortString(TimeSpan timeSpan)
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

	public static SolidColorBrush ToBrush(this string colorString)
	{
		try
		{
			return new BrushConverter().ConvertFrom(colorString) as SolidColorBrush;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static string FixRootPath(string path)
	{
		// Directory.GetDirectories, Directory.GetFiles and Path.Combine does not work on root paths without trailing backslashes.
		if (path.EndsWith(':'))
		{
			return path += "\\";
		}
		return path;
	}

	internal static Size MeasureText(string text, TextBlock control)
	{
		Typeface typeface = new(control.FontFamily, control.FontStyle, control.FontWeight, control.FontStretch);

		FormattedText formattedText = new(
			text,
			CultureInfo.CurrentCulture,
			control.FlowDirection,
			typeface,
			control.FontSize,
			control.Foreground,
			new NumberSubstitution(),
			TextFormattingMode.Display,
			VisualTreeHelper.GetDpi(control).PixelsPerDip
		);

		return new Size(formattedText.Width, formattedText.Height);
	}

}
