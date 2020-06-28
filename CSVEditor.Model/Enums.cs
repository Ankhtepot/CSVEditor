using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model
{
    public class Enums
    {
        public enum FieldType
        {
            TextBox,
            TextArea,
            Image,
            URI,
            Select,
            Date
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
