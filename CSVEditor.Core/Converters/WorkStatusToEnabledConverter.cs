using System;
using System.Globalization;
using System.Windows.Data;
using static CSVEditor.Core.HelperClasses.Enums;

namespace CSVEditor.Core.Converters
{
    public class WorkStatusToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return true;
            }

            var status = (WorkStatus)value;
            return status != WorkStatus.Working;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
