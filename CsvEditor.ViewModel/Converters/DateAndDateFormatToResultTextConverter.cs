using System;
using System.Globalization;
using System.Windows.Data;

namespace CSVEditor.ViewModel.Converters
{
    public class DateAndDateFormatToResultTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string dateFormat && values[1] is DateTime date)
            {
                return $"Date: \"{date.ToShortDateString()}\" with date format of \"{dateFormat}\" results to:";
            }

            return "Something went wrong :( .";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
