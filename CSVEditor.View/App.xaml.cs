using CSVEditor.Model.Services;
using CSVEditor.ViewModel;
using System.IO;
using System.Windows;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            JsonServices.SerializeJson(EditorVM.AppOptions,
                Path.Combine(EditorVM.ConfigurationFolderPath, EditorVM.APP_OPTIONS_FILE_NAME),
                "App Options");

            JsonServices.SerializeJson(EditorVM.FileConfigurations,
                Path.Combine(EditorVM.ConfigurationFolderPath, EditorVM.CSV_CONFIGURATIONS_FILE_NAME),
                "Csv file configurations");
        }
    }
}
