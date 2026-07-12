using CSVEditor.Core.HelperClasses;
using CSVEditor.ViewModel;
using System.Windows;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for SaveWindow.xaml
    /// </summary>
    public partial class SaveWindow : Window
    {
        private readonly SaveVM _dataContext;
        
        public SaveWindow(SaveOptions saveOptions, string csvFileText, string csvFilePath)
        {
            InitializeComponent();

            _dataContext = (DataContext as SaveVM)!;
            _dataContext.SaveOptions = saveOptions;
            _dataContext.CsvFilePath = csvFilePath;
            _dataContext.CsvFileText = csvFileText;
            _dataContext.SaveWindow = SaveWindowMain;
        }

        private void UncheckPushOnSave(object sender, RoutedEventArgs e)
        {
            _dataContext.SaveOptions.PushOnSave = false;
        }

        private void OpenGitSetupButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowService windowService = new();
            windowService.OpenGitSetupWindow();
        }
    }
}
