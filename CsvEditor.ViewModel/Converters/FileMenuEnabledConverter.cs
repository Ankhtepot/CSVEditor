using CSVEditor.Model;
using System;
using System.Globalization;
using System.Windows.Data;
using static CSVEditor.Model.HelperClasses.Enums;

namespace CSVEditor.ViewModel.Converters
{
    public class FileMenuEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is WorkStatus workStatus && values[1] is CsvFile selectedCsvFile)
            {
                return workStatus != WorkStatus.Working && selectedCsvFile != null;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
