using CSVEditor.Model.HelperClasses;
using CSVEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.View
{
    public class WindowService
    {
        public SaveOptions ShowSaveWindow(SaveOptions saveOptions)
        {
            var window = new SaveWindow(saveOptions);

            window.ShowDialog();

            return window.SetOptions;
        }
    }
}
