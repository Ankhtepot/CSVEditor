using System;
using System.Globalization;
using System.Windows.Data;

namespace CSVEditor.ViewModel.Converters
{
    public class AbsPathToRawTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Services.FileProcessingServices.GetRawFileText((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
}
