using CSVEditor.ViewModel;
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
using System.Windows.Shapes;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for DateGuideWindow.xaml
    /// </summary>
    public partial class DateGuideWindow : Window
    {
        private DateGuideVM VM;
        public DateGuideWindow(string inputUriText)
        {
            InitializeComponent();
            VM = new DateGuideVM(inputUriText);
            TopContainer.DataContext = VM;
        }
    }
}
