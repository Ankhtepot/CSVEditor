using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static CSVEditor.Model.HelperClasses.Enums;

namespace CSVEditor.ViewModel.Converters
{
    public class WorkStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (WorkStatus)value;

            return status == WorkStatus.Working ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
