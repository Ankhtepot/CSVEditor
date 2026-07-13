using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CSVEditor.View.Controls.DataCellElements
{
    /// <summary>
    /// Interaction logic for DateElementControl.xaml
    /// </summary>
    public partial class DateElementControl : UserControl
    {
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(DateElementControl), new PropertyMetadata(""));

        public string DateFormat
        {
            get => (string)GetValue(DateFormatProperty);
            set => SetValue(DateFormatProperty, value);
        }
        public static readonly DependencyProperty DateFormatProperty =
            DependencyProperty.Register(nameof(DateFormat), typeof(string), typeof(DateElementControl), new PropertyMetadata(""));       

        public DelegateCommand GenerateDateFromNowCommand { get; set; }

        public Action OnEdited;

        public DateElementControl()
        {
            InitializeComponent();
            TopContainer.DataContext = this;

            GenerateDateFromNowCommand = new DelegateCommand(GenerateDateFromNow);
        }

        private void GenerateDateFromNow()
        {
            GenerateDate(new DateTime((DateTime.Now).Ticks));            
        }

        private void GenerateDate(DateTime dateTime)
        {
            Text = dateTime.ToString(DateFormat);
            OnEdited?.Invoke();
        }

        private static void DateFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateElementControl control = (DateElementControl)d;
            string newText = (string)e.NewValue;

            if (control == null)
            {
                return;
            }

            if(string.IsNullOrEmpty(newText))
            {
                control.DateFormat = "MMMM dd";
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime newDate = e.AddedItems.Count > 0 ? (DateTime)e.AddedItems[0]! : default;
            GenerateDate(newDate);
        }
    }
}
