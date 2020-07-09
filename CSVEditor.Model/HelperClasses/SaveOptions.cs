using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model.HelperClasses
{
    public class SaveOptions
    {
        public bool RememberSetting { get; set; }
        public string AlternativePath { get; set; }
        public bool CommitOnSave { get; set; }
        public bool PushOnSave { get; set; }
        public SaveMode SaveModeSetting { get; set; }

        public enum SaveMode
        {
            Overwrite,
            AlternativePath,
        }

        public SaveOptions()
        {
            RememberSetting = false;
            CommitOnSave = false;
            PushOnSave = false;
            AlternativePath = AppDomain.CurrentDomain.BaseDirectory;
            SaveModeSetting = SaveMode.Overwrite;
        }
    }
}
