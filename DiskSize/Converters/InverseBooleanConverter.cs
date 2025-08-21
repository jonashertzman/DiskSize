using System.Globalization;
using System.Windows.Data;

namespace DiskSize;
public class InverseBooleanConverter : IValueConverter
{

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (targetType != typeof(bool))
			throw new InvalidOperationException("The target must be a Boolean");

		return !(bool)value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

}
