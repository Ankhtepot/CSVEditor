using CSVEditor.Model.HelperClasses;
using CSVEditor.ViewModel;
using System.Windows;

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
