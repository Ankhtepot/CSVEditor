using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace CSVEditor.ViewModel.Converters
{
    public class IndexToOneBasedNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int number;
            int.TryParse((string)value, out number);

            return (parseValue(value) + 1).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            return (parseValue(value) - 1).ToString();
        }

        private int parseValue(object value)
        {
            int number;
            int.TryParse((string)value, out number);

            return number;
        }
    }
}
