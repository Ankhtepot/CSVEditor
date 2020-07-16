using CSVEditor.Model.HelperClasses;
using CSVEditor.Model.Interfaces;
using CSVEditor.ViewModel;

namespace CSVEditor.View
{
    public class WindowService : IWindowService
    {
        public SaveOptions OpenSaveWindow(SaveOptions saveOptions, string csvFileText, string csvFilePath)
        {
            var window = new SaveWindow(saveOptions, csvFileText, csvFilePath);

            window.ShowDialog();
            var windowContext = window.DataContext as SaveVM;

            return windowContext.SaveSuccessful ?
                (window.DataContext as SaveVM).SaveOptions
                : null;
        }

        public GitOptions OpenGitSetupWindow()
        {
            var window = new GitSetupWindow(EditorVM.AppOptions.GitOptions);
            window.ShowDialog();

            return window.Canceled
                ? null
                : window.GitOptions;
        }
    }
}
