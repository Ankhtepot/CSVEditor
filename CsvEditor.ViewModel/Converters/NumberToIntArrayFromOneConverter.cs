using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace CSVEditor.ViewModel.Converters
{
    public class NumberToIntArrayFromOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int number;
            List<int> result = new List<int>();

            if (int.TryParse(value.ToString(), out number))
            {
                for (int i = 1; i <= number; i++)
                {
                    result.Add(i);
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
