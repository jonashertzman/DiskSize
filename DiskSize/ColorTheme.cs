using System.Reflection;

namespace DiskSize;

public class ColorTheme
{

	#region Properties

	// Editor colors
	public required string SelectionBackground { get; set; }

	// UI colors
	public required string NormalText { get; set; }
	public required string DisabledText { get; set; }

	public required string WindowBackground { get; set; }
	public required string DialogBackground { get; set; }

	public required string ControlLightBackground { get; set; }
	public required string ControlDarkBackground { get; set; }

	public required string BorderLight { get; set; }
	public required string BorderDark { get; set; }

	public required string HighlightBackground { get; set; }
	public required string HighlightBorder { get; set; }

	public required string AttentionBackground { get; set; }

	#endregion

	#region Methods

	public ColorTheme Clone()
	{
		return (ColorTheme)MemberwiseClone();
	}

	internal void SetDefaultsIfNull(ColorTheme defaultTheme)
	{
		foreach (PropertyInfo propertyInfo in this.GetType().GetProperties())
		{
			if (propertyInfo.GetValue(this) == null)
			{
				propertyInfo.SetValue(this, propertyInfo.GetValue(defaultTheme));
			}
		}
	}

	internal void SetDefaults(ColorTheme defaultTheme)
	{
		foreach (PropertyInfo propertyInfo in this.GetType().GetProperties())
		{
			propertyInfo.SetValue(this, propertyInfo.GetValue(defaultTheme));
		}
	}

	#endregion

}
