using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model
{
    public class Enums
    {
        public enum FieldType
        {
            TextArea,
            TextBox,
            Image,
            URI,
            Select
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
