using CSVEditor.Model.Services;
using CSVEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
            JsonServices.SaveAppOptions(EditorVM.AppOptions, EditorVM.BaseAppPath);
        }
    }
}
