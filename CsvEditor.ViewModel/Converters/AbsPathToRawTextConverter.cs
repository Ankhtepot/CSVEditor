using CSVEditor.Model.HelperClasses;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using CSVEditor.Model.Services;

namespace CSVEditor.ViewModel.Converters
{
    public class AbsPathToRawTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = (string)value;

            if (File.Exists(path))
            {
                return FileProcessingServices.GetRawFileText(path); 
            }

            return Constants.NO_FILE_SELECTED;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
}
