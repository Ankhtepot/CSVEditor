using CSVEditor.Model.HelperClasses;
using System.Windows;

namespace CSVEditor.Model.Interfaces
{
    public interface IWindowService
    {
        SaveOptions OpenSaveWindow(SaveOptions saveOptions, string csvFileText, string csvFilePath);
        GitOptions OpenGitSetupWindow(GitOptions gitOptions);
    }
}
