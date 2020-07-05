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
            EditorVM.SaveOnExit();
        }
    }
}
