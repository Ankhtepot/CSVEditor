using CSVEditor.Core.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CSVEditor.Core.Extensions;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for UriTextBoxControl.xaml
    /// </summary>
    public partial class UriTextBoxControl : UserControl, INotifyPropertyChanged
    {
        public string OpenLinkHintText { get; set; } = "You can also double click on the text to open the link.";

        private bool isTextValidUri;
        public bool IsTextValidUri
        {
            get => isTextValidUri;
            set 
            {
                isTextValidUri = value;
                OnPropertyChanged();
            }
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(UriTextBoxControl), new PropertyMetadata("", TextChanged));        

        public UriTextBoxControl()
        {            
            InitializeComponent();
            IsTextValidUri = false;
            TopContainer.DataContext = this;
        }

        private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (UriTextBoxControl)d;
            var newText = (string)e.NewValue;

            if (control == null || string.IsNullOrEmpty(newText))
            {
                return;
            }

            control.IsTextValidUri = newText.IsValidUrl();
        }
        
        private void UriTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenUriLink();
        }

        private void OpenUriButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUriLink();
        }

        private void OpenUriLink()
        {
            if (IsTextValidUri)
            {
                var linkText = UriTextBox.Text;

                try
                {
                    Process.Start(new ProcessStartInfo(linkText) { UseShellExecute = true });
                }
                catch (Exception error)
                {
                    Console.WriteLine($@"Error opening link: ""{linkText}"" | Error message: {error.Message}");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
