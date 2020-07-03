using CSVEditor.Model.HelperClasses;
using System;
using System.Globalization;
using System.Windows.Data;
using static CSVEditor.Model.HelperClasses.Enums;

namespace CSVEditor.ViewModel.Converters
{
    public class WorkStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (WorkStatus)value;

            switch (status)
            {
                case WorkStatus.Idle: return Constants.IDLE_WORK_STATUS;
                case WorkStatus.Working: return Constants.WORKING_WORK_STATUS;                
                case WorkStatus.Canceled: return Constants.CANCELED_WORK_STATUS;
                case WorkStatus.Error: return Constants.ERROR_WORK_STATUS;
                case WorkStatus.Done: return Constants.COMPLETED_WORK_STATUS;
                default: return Constants.UNKNOWN_STATE;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
