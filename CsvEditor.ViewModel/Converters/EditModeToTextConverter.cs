using CSVEditor.Model.HelperClasses;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CSVEditor.ViewModel.Converters
{
    public class EditModeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Constants.EDIT_LINES : Constants.EDIT_COLUMN_CONFIGURATIONS;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
