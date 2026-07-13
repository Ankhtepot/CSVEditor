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
            get => (Thickness)GetValue(ButtonMarginProperty);
            set => SetValue(ButtonMarginProperty, value);
        }
        public static readonly DependencyProperty ButtonMarginProperty =
            DependencyProperty.Register(nameof(ButtonMargin), typeof(Thickness), typeof(ImageButtonControl), new PropertyMetadata(new Thickness(0,0,0,0)));

        public Thickness ButtonPadding
        {
            get => (Thickness)GetValue(ButtonPaddingProperty);
            set => SetValue(ButtonPaddingProperty, value);
        }
        public static readonly DependencyProperty ButtonPaddingProperty =
            DependencyProperty.Register(nameof(ButtonPadding), typeof(Thickness), typeof(ImageButtonControl), new PropertyMetadata(new Thickness(0,0,0,0)));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ImageButtonControl), new PropertyMetadata(null));

        public string CommandParameter
        {
            get => (string)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(string), typeof(ImageButtonControl), new PropertyMetadata(""));

        public BitmapImage ImageSource
        {
            get => (BitmapImage)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(BitmapImage), typeof(ImageButtonControl));

        public Stretch ImageStretch
        {
            get => (Stretch)GetValue(ImageStretchProperty);
            set => SetValue(ImageStretchProperty, value);
        }
        public static readonly DependencyProperty ImageStretchProperty =
            DependencyProperty.Register(nameof(ImageStretch), typeof(Stretch), typeof(ImageButtonControl), new PropertyMetadata(Stretch.Fill));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(ImageButtonControl), new PropertyMetadata(new CornerRadius(0)));

        public bool Enabled
        {
            get => (bool)GetValue(EnabledProperty);
            set => SetValue(EnabledProperty, value);
        }
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register(nameof(Enabled), typeof(bool), typeof(ImageButtonControl), new PropertyMetadata(true));

        public SolidColorBrush EnabledBackgroundColor
        {
            get => (SolidColorBrush)GetValue(EnabledBackgroundColorProperty);
            set => SetValue(EnabledBackgroundColorProperty, value);
        }
        public static readonly DependencyProperty EnabledBackgroundColorProperty =
            DependencyProperty.Register(nameof(EnabledBackgroundColor), typeof(SolidColorBrush), typeof(ImageButtonControl), new PropertyMetadata(new SolidColorBrush(Colors.MediumAquamarine)));

        public ImageButtonControl()
        {
            InitializeComponent();
            TopContainer.DataContext = this;
        }
    }
}
