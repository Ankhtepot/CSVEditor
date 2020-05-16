using CSVEditor.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static CSVEditor.Model.Enums;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for ProgressInfoControl.xaml
    /// </summary>
    public partial class ProgressInfoControl : UserControl
    {


        public WorkStatus WorkStatus
        {
            get { return (WorkStatus)GetValue(WorkStatusProperty); }
            set { SetValue(WorkStatusProperty, value); }
        }

        public static readonly DependencyProperty WorkStatusProperty =
            DependencyProperty.Register("WorkStatus", typeof(WorkStatus), typeof(ProgressInfoControl), new PropertyMetadata(WorkStatus.Idle, WorkStatusChanged));



        public int Progress
        {
            get { return (int)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Progress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(int), typeof(ProgressInfoControl), new PropertyMetadata(0, ProgressChanged));

        private static void ProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressInfoControl control = (ProgressInfoControl)d;
            var progressValue = (int)e.NewValue;
            if(progressValue >= 100)
            {
                control.WorkProgressBar.IsIndeterminate = true;
            }
            else
            {
                control.WorkProgressBar.IsIndeterminate = false;
                control.WorkProgressBar.Value = progressValue;
            }
        }

        private static void WorkStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressInfoControl control = (ProgressInfoControl)d;
            var status = (WorkStatus)e.NewValue;
            
            manageWorkStatus(status, control);
        }

        public ProgressInfoControl()
        {
            InitializeComponent();
            manageWorkStatus(WorkStatus.Idle, this);
        }

        private static void manageWorkStatus(WorkStatus status, ProgressInfoControl control)
        {
            switch (status)
            {
                case WorkStatus.Idle:
                    {
                        control.WorkStatusTextBlock.Text = Constants.IDLE_WORK_STATUS;
                        control.Spinner.Spin = false;
                        control.WorkProgressBar.IsIndeterminate = false;
                        control.WorkProgressBar.Value = 0;
                        control.WorkProgressBar.Visibility = Visibility.Collapsed;
                    };
                    break;
                case WorkStatus.Working:
                    {
                        control.WorkProgressBar.Visibility = Visibility.Visible;
                        control.Spinner.Spin = true;
                        control.WorkStatusTextBlock.Text = Constants.WORKING_WORK_STATUS;
                    };
                        break;
                case WorkStatus.Canceled:
                    {
                        resetSpinnerAndProgressBarToIdleStateDelayed(control, Constants.CANCELED_WORK_STATUS);
                    };
                    break;
                case WorkStatus.Error:
                    {
                        resetSpinnerAndProgressBarToIdleStateDelayed(control, Constants.ERROR_WORK_STATUS);
                    };
                    break;
                case WorkStatus.Done:
                    {
                        resetSpinnerAndProgressBarToIdleStateDelayed(control, Constants.COMPLETED_WORK_STATUS);
                    };
                    break;
            }
        }

        private static async void resetSpinnerAndProgressBarToIdleStateDelayed(ProgressInfoControl control, string fromState)
        {
            control.WorkStatus = WorkStatus.Idle;
            control.WorkStatusTextBlock.Text = fromState;
            await Task.Delay(2000);
            control.WorkStatusTextBlock.Text = Constants.IDLE_WORK_STATUS;
            control.WorkStatus = WorkStatus.Idle;
        }
    }
}
