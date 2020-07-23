using System;
using System.Globalization;
using System.Windows.Data;

namespace CSVEditor.ViewModel.Converters
{
    public class DateFormatToFormatedDateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string prefix = "";
            string sufix = "";

            if (parameter is string parameterString && !string.IsNullOrEmpty(parameterString))
            {
                var fixes = parameterString.Split('|');
                prefix = fixes[0];
                sufix = !string.IsNullOrEmpty(fixes[1]) ? fixes[1] : "";
            }

            if (values[0] is string dateFormat && values[1] is DateTime dateString)
            {
                return $"{prefix}{dateString.ToString(dateFormat)}{sufix}";
            }

            return "bad date format";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
