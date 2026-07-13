using System.Windows;
using System.Windows.Controls;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for FileInfoControl.xaml
    /// </summary>
    public partial class FileInfoControl : UserControl
    {
        public string SelectedText
        {
            get => (string)GetValue(SelectedTextProperty);
            set => SetValue(SelectedTextProperty, value);
        }
        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.Register(nameof(SelectedText), typeof(string), typeof(FileInfoControl), new PropertyMetadata("", SelectedTextChanged));

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
            get => (string)GetValue(FileInfoTextProperty);
            set => SetValue(FileInfoTextProperty, value);
        }
        public static readonly DependencyProperty FileInfoTextProperty =
            DependencyProperty.Register(nameof(FileInfoText), typeof(string), typeof(FileInfoControl), new PropertyMetadata("", FileInfoTextChanged));

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
