using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using static CSVEditor.Model.HelperClasses.Enums;

namespace CSVEditor.ViewModel.Converters
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
