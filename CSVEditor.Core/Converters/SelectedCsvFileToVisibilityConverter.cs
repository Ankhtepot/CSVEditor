using CSVEditor.Core;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CSVEditor.Core.Converters
{
    public class SelectedCsvFileToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var newValue = (CsvFile)value;
            return newValue != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
