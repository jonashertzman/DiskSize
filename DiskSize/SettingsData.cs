using System.Windows;

namespace DiskSize;

public class SettingsData
{

	public string Id { get; set; } = Guid.NewGuid().ToString();

	public DateTime LastUpdateTime { get; set; } = DateTime.MinValue;
	public bool CheckForUpdates { get; set; } = true;
	public bool ShowWhiteSpaceCharacters { get; set; } = false;
	public bool MasterDetail { get; set; } = false;

	public string Font { get; set; } = DefaultSettings.Font;
	public int FontSize { get; set; } = DefaultSettings.FontSize;
	public int Zoom { get; set; } = 0;
	public int TabSize { get; set; } = DefaultSettings.TabSize;

	public Themes Theme { get; set; } = Themes.Light;
	public ColorTheme DarkTheme { get; set; } = DefaultSettings.DarkTheme.Clone();
	public ColorTheme LightTheme { get; set; } = DefaultSettings.LightTheme.Clone();

	public double PositionLeft { get; set; }
	public double PositionTop { get; set; }
	public double Width { get; set; } = 700;
	public double Height { get; set; } = 500;
	public double FolderRowHeight { get; set; } = 300;
	public WindowState WindowState { get; set; }

}
