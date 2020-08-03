using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CSVEditor.View.Controls.EditGridCellElements
{
    /// <summary>
    /// Interaction logic for SelectElementControl.xaml
    /// </summary>
    public partial class SelectElementControl
    {
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SelectElementControl), new PropertyMetadata("", TextChanged));        

        public List<string> ComboBoxSource
        {
            get => (List<string>)GetValue(ComboBoxSourceProperty);
            set => SetValue(ComboBoxSourceProperty, value);
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

            if (foundIndex == -1)
            {
                return;
            }

            control.SourceComboBox.SelectionChanged -= control.SourceComboBox_SelectionChanged;
            control.SourceComboBox.SelectedIndex = foundIndex;
            control.SourceComboBox.SelectionChanged += control.SourceComboBox_SelectionChanged;
        }

        private void SourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Text = e.AddedItems[0] as string;            
        }

        private void SourceComboBox_DropDownClosed(object sender, EventArgs e)
        {
            OnEdited?.Invoke();
        }

        private void ContentTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            OnEdited?.Invoke();
        }
    }
}
