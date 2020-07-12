using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for ImageButtonControl.xaml
    /// </summary>
    public partial class ImageButtonControl : UserControl
    {
        public Thickness ButtonMargin
        {
            get { return (Thickness)GetValue(ButtonMarginProperty); }
            set { SetValue(ButtonMarginProperty, value); }
        }
        public static readonly DependencyProperty ButtonMarginProperty =
            DependencyProperty.Register("ButtonMargin", typeof(Thickness), typeof(ImageButtonControl), new PropertyMetadata(new Thickness(0,0,0,0)));

        public Thickness ButtonPadding
        {
            get { return (Thickness)GetValue(ButtonPaddingProperty); }
            set { SetValue(ButtonPaddingProperty, value); }
        }
        public static readonly DependencyProperty ButtonPaddingProperty =
            DependencyProperty.Register("ButtonPadding", typeof(Thickness), typeof(ImageButtonControl), new PropertyMetadata(new Thickness(0,0,0,0)));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ImageButtonControl), new PropertyMetadata(null));

        public string CommandParameter
        {
            get { return (string)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(string), typeof(ImageButtonControl), new PropertyMetadata(""));

        public BitmapImage ImageSource
        {
            get { return (BitmapImage)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(BitmapImage), typeof(ImageButtonControl));

        public Stretch ImageStretch
        {
            get { return (Stretch)GetValue(ImageStretchProperty); }
            set { SetValue(ImageStretchProperty, value); }
        }
        public static readonly DependencyProperty ImageStretchProperty =
            DependencyProperty.Register("ImageStretch", typeof(Stretch), typeof(ImageButtonControl), new PropertyMetadata(Stretch.Fill));



        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(ImageButtonControl), new PropertyMetadata(new CornerRadius(0)));

        public ImageButtonControl()
        {
            InitializeComponent();
            TopContainer.DataContext = this;
        }
    }
}
