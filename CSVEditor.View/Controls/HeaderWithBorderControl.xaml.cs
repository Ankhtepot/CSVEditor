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
    /// Interaction logic for HeaderWithBorderControl.xaml
    /// </summary>
    public partial class HeaderWithBorderControl : UserControl
    {
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HeaderWithBorderControl), new PropertyMetadata(""));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(HeaderWithBorderControl), new PropertyMetadata(new CornerRadius(5)));

        public Thickness TextPadding
        {
            get => (Thickness)GetValue(TextPaddingProperty);
            set => SetValue(TextPaddingProperty, value);
        }
        public static readonly DependencyProperty TextPaddingProperty =
            DependencyProperty.Register("TextPadding", typeof(Thickness), typeof(HeaderWithBorderControl), new PropertyMetadata(new Thickness(5)));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(HeaderWithBorderControl), new PropertyMetadata(null));

        public string CommandParameter
        {
            get => (string)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(string), typeof(HeaderWithBorderControl), new PropertyMetadata(null));

        public HeaderWithBorderControl()
        {
            InitializeComponent();
            TopContainer.DataContext = this;
        }

        private void TextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Command?.Execute(CommandParameter);
        }
    }
}
