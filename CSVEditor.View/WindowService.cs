using System.Windows;
using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Interfaces;
using CSVEditor.Core.Services;
using CSVEditor.ViewModel;

namespace CSVEditor.View
{
    public class WindowService : IWindowService
    {
        public SaveOptions OpenSaveWindow(SaveOptions saveOptions, string csvFileText, string csvFilePath)
        {
            SaveWindow window = new(saveOptions, csvFileText, csvFilePath);

            window.ShowDialog();
            SaveVM windowContext = window.DataContext as SaveVM;
            
            if (windowContext == null)
                return null;

            return windowContext.SaveSuccessful ?
                windowContext.SaveOptions
                : null;
        }

        public bool OpenGitSetupWindow()
        {
            GitSetupWindow window = new()
            {
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();

            return !window.Canceled;
        }

        public bool OpenGitPushWindow()
        {
            GitPushWindow window = new(AppOptionsService.AppOptions.GitOptions);
            window.ShowDialog();

            return !window.Canceled;
        }
    }
}
