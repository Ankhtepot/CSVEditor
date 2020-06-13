using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model
{
    public class Enums
    {
        public enum FieldType
        {
            TextBox = 0,
            TextArea = 1,
            Image = 2,
            URI = 3,
            Select = 4
        }

        public enum WorkStatus 
        {
            Idle,
            Working,
            Canceled,
            Error,
            Done
        }
    }
}
