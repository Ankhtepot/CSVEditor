using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace CSVEditor.ViewModel.Convertors
{
    class AbsPathToFileNameConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var absPath = (string)value;
            var lastDelimiterIndex = absPath.LastIndexOf('\\');
            return lastDelimiterIndex > 0 ? absPath.Substring(lastDelimiterIndex + 1) : absPath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
