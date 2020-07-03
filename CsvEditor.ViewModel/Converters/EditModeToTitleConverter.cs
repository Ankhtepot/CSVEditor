using CSVEditor.Model.HelperClasses;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CSVEditor.ViewModel.Converters
{
    public class EditModeToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Constants.CONFIGURATION_EDITOR_TITLE : Constants.LINE_EDITOR_TITLE;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
