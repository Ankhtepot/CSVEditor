using System.Windows;
using System.Windows.Controls;

namespace CSVEditor.View.Controls.EditGridCellElements
{
    /// <summary>
    /// Interaction logic for LabeledTextBoxControl.xaml
    /// </summary>
    public partial class LabeledTextBoxControl : UserControl
    {
        public string LabelContent
        {
            get => (string)GetValue(LabelContentProperty);
            set => SetValue(LabelContentProperty, value);
        }
        public static readonly DependencyProperty LabelContentProperty =
            DependencyProperty.Register(nameof(LabelContent), typeof(string), typeof(LabeledTextBoxControl), new PropertyMetadata(""));


        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(LabeledTextBoxControl), new PropertyMetadata(""));

        public double TextBoxWidth
        {
            get => (double)GetValue(TextBoxWidthProperty);
            set => SetValue(TextBoxWidthProperty, value);
        }
        public static readonly DependencyProperty TextBoxWidthProperty =
            DependencyProperty.Register(nameof(TextBoxWidth), typeof(double), typeof(LabeledTextBoxControl), new PropertyMetadata(100d));

        public bool AcceptsReturn
        {
            get => (bool)GetValue(AcceptsReturnProperty);
            set => SetValue(AcceptsReturnProperty, value);
        }
        public static readonly DependencyProperty AcceptsReturnProperty =
            DependencyProperty.Register(nameof(AcceptsReturn), typeof(bool), typeof(LabeledTextBoxControl), new PropertyMetadata(false));

        public double TextBoxHeight
        {
            get => (double)GetValue(TextBoxHeightProperty);
            set => SetValue(TextBoxHeightProperty, value);
        }
        public static readonly DependencyProperty TextBoxHeightProperty =
            DependencyProperty.Register(nameof(TextBoxHeight), typeof(double), typeof(LabeledTextBoxControl), new PropertyMetadata(20d));

        public LabeledTextBoxControl()
        {
            InitializeComponent();
            TopContainer.DataContext = this;
        }
    }
}
