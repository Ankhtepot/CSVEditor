using CSVEditor.Model.HelperClasses;
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
    /// Interaction logic for SaveWindow.xaml
    /// </summary>
    public partial class SaveWindow : Window
    {
        public SaveOptions SetOptions { get; set; }
        public SaveWindow(SaveOptions saveOptions)
        {
            InitializeComponent();

            (DataContext as SaveVM).SaveOptions = saveOptions;
            SetOptions = saveOptions;
        }
    }
}
