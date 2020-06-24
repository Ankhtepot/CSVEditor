using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for LineNavigationBarControl.xaml
    /// </summary>
    public partial class LineNavigationBarControl : UserControl
    {
        //public LineNavigationBarControlViewModel VM { get; set; }

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

            //VM = new LineNavigationBarControlViewModel(this);
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
            //TODO: Continue here
        }
    }
}
