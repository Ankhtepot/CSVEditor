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

namespace CSVEditor.View.Controls.EditGridCellElements
{
    /// <summary>
    /// Interaction logic for LabeledTextBoxControl.xaml
    /// </summary>
    public partial class LabeledTextBoxControl : UserControl
    {
        public string LabelContent
        {
            get { return (string)GetValue(LabelContentProperty); }
            set { SetValue(LabelContentProperty, value); }
        }
        public static readonly DependencyProperty LabelContentProperty =
            DependencyProperty.Register("LabelContent", typeof(string), typeof(LabeledTextBoxControl), new PropertyMetadata(""));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(LabeledTextBoxControl), new PropertyMetadata(""));

        public LabeledTextBoxControl()
        {
            InitializeComponent();
            TopContainer.DataContext = this;
        }
    }
}
