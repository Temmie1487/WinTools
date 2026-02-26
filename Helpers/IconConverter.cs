using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace WinTools.Helpers;

public class IconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string iconName && !string.IsNullOrEmpty(iconName))
        {
            try
            {
                return Enum.Parse<SymbolRegular>(iconName);
            }
            catch
            {
                return SymbolRegular.Info24;
            }
        }
        return SymbolRegular.Info24;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
