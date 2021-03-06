﻿using CSVEditor.Model.HelperClasses;

namespace CSVEditor.Model.Interfaces
{
    public interface IWindowService
    {
        SaveOptions OpenSaveWindow(SaveOptions saveOptions, string csvFileText, string csvFilePath);
        GitOptions OpenGitSetupWindow();
    }
}
