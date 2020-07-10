using CSVEditor.Model.HelperClasses;
using CSVEditor.ViewModel;
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
using System.Windows.Shapes;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for SaveWindow.xaml
    /// </summary>
    public partial class SaveWindow : Window
    {
        public SaveWindow(SaveOptions saveOptions, string csvFileText, string csvFilePath)
        {
            InitializeComponent();

            var context = DataContext as SaveVM;
            context.SaveOptions = saveOptions;
            context.CsvFilePath = csvFilePath;
            context.CsvFileText = csvFileText;
            context.SaveWindow = SaveWindowMain;
        }

        private void UncheckPushOnSave(object sender, RoutedEventArgs e)
        {
            (DataContext as SaveVM).SaveOptions.PushOnSave = false;
        }
    }
}
