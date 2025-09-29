using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiskSize;

static class Utils
{

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

	internal static Size MeasureText(string text, Control control)
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
