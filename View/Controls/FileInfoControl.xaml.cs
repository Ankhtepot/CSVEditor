using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for FileInfoControl.xaml
    /// </summary>
    public partial class FileInfoControl : UserControl
    {


        public string SelectedText
        {
            get { return (string)GetValue(SelectedTextProperty); }
            set { SetValue(SelectedTextProperty, value); }
        }

        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.Register("SelectedText", typeof(string), typeof(FileInfoControl), new PropertyMetadata("", SelectedTextChanged));

        private static void SelectedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FileInfoControl control = (FileInfoControl)d;

            if (control != null && !string.IsNullOrEmpty((string)e.NewValue))
            {
                control.SelectedTextLabel.Content = (string)e.NewValue;
            }
        }

        public string FileInfoText
        {
            get { return (string)GetValue(FileInfoTextProperty); }
            set { SetValue(FileInfoTextProperty, value); }
        }

        public static readonly DependencyProperty FileInfoTextProperty =
            DependencyProperty.Register("FileInfoText", typeof(string), typeof(FileInfoControl), new PropertyMetadata("", FileInfoTextChanged));

        private static void FileInfoTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FileInfoControl control = (FileInfoControl)d;

            if (control != null && !string.IsNullOrEmpty((string)e.NewValue))
            {
                control.FileInfoTextBox.Text = (string)e.NewValue; 
            }
        }

        public FileInfoControl()
        {
            InitializeComponent();
        }
    }
}
