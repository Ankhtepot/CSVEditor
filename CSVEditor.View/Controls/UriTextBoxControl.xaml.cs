using CSVEditor.Model.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for UriTextBoxControl.xaml
    /// </summary>
    public partial class UriTextBoxControl : UserControl, INotifyPropertyChanged
    {
        private bool isTextValidUri;
        public bool IsTextValidUri
        {
            get { return isTextValidUri; }
            set 
            {
                isTextValidUri = value;
                OnPropertyChanged();
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(UriTextBoxControl), new PropertyMetadata("", TextChanged));        

        public UriTextBoxControl()
        {
            IsTextValidUri = false;
            Text = "";
            InitializeComponent();
        }

        private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (UriTextBoxControl)d;
            var newText = (string)e.NewValue;

            if (control == null || string.IsNullOrEmpty(newText))
            {
                return;
            }

            control.IsTextValidUri = newText.IsValidURL();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
