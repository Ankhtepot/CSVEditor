using CSVEditor.Core.HelperClasses;

namespace CSVEditor.Core.Interfaces
{
    public interface IWindowService
    {
        SaveOptions OpenSaveWindow(SaveOptions saveOptions, string csvFileText, string csvFilePath);
        GitOptions OpenGitSetupWindow();
        bool OpenGitPushWindow();
    }
}
