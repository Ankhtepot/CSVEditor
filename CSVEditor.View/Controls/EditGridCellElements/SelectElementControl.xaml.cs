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

            control.SourceComboBox.SelectedIndex = Math.Max(0, control.ComboBoxSource.FindIndex(record => record == newText));
        }

        private void SourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Text = e.AddedItems[0] as string;
        }
    }
}
