using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TirkxDownloader.ViewModels;

namespace TirkxDownloader.Views.Converter
{
    public class DialogTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (DialogType)Enum.Parse(typeof(DialogType), value.ToString());
            var btnType = parameter.ToString();

            if (btnType == "NO")
            {
                return type == DialogType.Confirmation ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return type == DialogType.Input ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
