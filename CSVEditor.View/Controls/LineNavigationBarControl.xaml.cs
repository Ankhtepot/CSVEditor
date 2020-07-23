using CSVEditor.Model.Services;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for LineNavigationBarControl.xaml
    /// </summary>
    public partial class LineNavigationBarControl : UserControl
    {
        public int InputIndex
        {
            get { return (int)GetValue(InputIndexProperty); }
            set { SetValue(InputIndexProperty, value); }
        }
        public static readonly DependencyProperty InputIndexProperty =
            DependencyProperty.Register("InputIndex", typeof(int), typeof(LineNavigationBarControl), new PropertyMetadata(0, InputIndexChanged));

        public int LinesCount
        {
            get { return (int)GetValue(LinesCountProperty); }
            set { SetValue(LinesCountProperty, value); }
        }
        public static readonly DependencyProperty LinesCountProperty =
            DependencyProperty.Register("LinesCount", typeof(int), typeof(LineNavigationBarControl), new PropertyMetadata(0, LinesCountChanged));

        public LineNavigationBarControl()
        {
            InitializeComponent();

            NumberTextBox.Text = 1.ToString();
        }

        private static void InputIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LineNavigationBarControl)d;
            var index = (int)e.NewValue;

            if (control == null)
            {
                return;
            }

            if(index == -1 )
            {
                index = 0;
            }

            control.NumberTextBox.Text = (index + 1).ToString();
        }

        private static void LinesCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LineNavigationBarControl)d;
            var linesCount = (int)e.NewValue;

            if (control == null)
            {
                return;
            }

            control.LinesCountTextBlock.Text = linesCount.ToString();
        }

        private void NumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ToBeginningButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputIndex > 0)
            {
                InputIndex = 0; 
            }
        }

        private void OneBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputIndex > 0)
            {
                InputIndex -= 1;
            }
        }

        private void OneForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputIndex < LinesCount - 1)
            {
                InputIndex += 1;
            }
        }

        private void LastLineButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputIndex < LinesCount - 1)
            {
                InputIndex = LinesCount - 1; 
            }
        }

        private void NumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int newInput;

            if(int.TryParse(NumberTextBox.Text, out newInput))
            {
                InputIndex = (newInput - 1).Clamp(0, LinesCount - 1);
            }
        }
    }
}
