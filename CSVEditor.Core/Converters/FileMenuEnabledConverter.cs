using CSVEditor.Core;
using System;
using System.Globalization;
using System.Windows.Data;
using static CSVEditor.Core.HelperClasses.Enums;

namespace CSVEditor.Core.Converters
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
