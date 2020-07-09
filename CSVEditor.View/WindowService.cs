using CSVEditor.Model.HelperClasses;
using CSVEditor.Model.Interfaces;
using CSVEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.View
{
    public class WindowService : IWindowService
    {
        public SaveOptions OpenSaveWindow(SaveOptions saveOptions, string rootRepositoryPath)
        {
            var window = new SaveWindow(saveOptions, rootRepositoryPath);

            window.ShowDialog();

            return window.SaveOptions;
        }
    }
}
