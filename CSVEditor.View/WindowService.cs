using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Interfaces;
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

        public GitOptions OpenGitSetupWindow()
        {
            GitSetupWindow window = new(EditorVM.AppOptions.GitOptions);
            window.ShowDialog();

            return window.Canceled
                ? null
                : window.GitOptions;
        }

        public bool OpenGitPushWindow()
        {
            GitPushWindow window = new(EditorVM.AppOptions.GitOptions);
            window.ShowDialog();

            return !window.Canceled;
        }
    }
}
