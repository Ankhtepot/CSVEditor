using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for SelectElementControl.xaml
    /// </summary>
    public partial class SelectElementControl : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SelectElementControl), new PropertyMetadata("", TextChanged));        

        public List<string> ComboBoxSource
        {
            get { return (List<string>)GetValue(ComboBoxSourceProperty); }
            set { SetValue(ComboBoxSourceProperty, value); }
        }
        public static readonly DependencyProperty ComboBoxSourceProperty =
            DependencyProperty.Register("ComboBoxSource", typeof(List<string>), typeof(SelectElementControl), new PropertyMetadata(new List<string>()));

        public Action OnEdited;

        public SelectElementControl()
        {
            InitializeComponent();
            TopContainer.DataContext = this;
        }

        private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SelectElementControl)d;
            var newText = (string)e.NewValue;

            if (control == null || string.IsNullOrEmpty(newText))
            {
                return;
            }

            var foundIndex = control.ComboBoxSource.FindIndex(record => record == newText);
            control.SourceComboBox.SelectedIndex = foundIndex == -1 || control.ComboBoxSource.Count == 0 ? 0 : foundIndex;
        }

        private void SourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Text = e.AddedItems[0] as string;            
        }

        private void SourceComboBox_DropDownClosed(object sender, EventArgs e)
        {
            OnEdited?.Invoke();
        }
    }
}
