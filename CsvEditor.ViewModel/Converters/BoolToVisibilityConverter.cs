using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace CSVEditor.ViewModel.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var newValue = (bool)value;
            var newParameter = (string)parameter;

            if (!string.IsNullOrEmpty(newParameter) && Regex.IsMatch(newParameter, "[Ff]alse"))
            {
                return newValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return newValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
