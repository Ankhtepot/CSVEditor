namespace CSVEditor.Core.HelperClasses
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

        public enum AddLinePlacement
        {
            Above,
            Below,
            ToTheTop,
            ToTheBottom
        }

        public enum DialogPathType
        {
            None = 0,
            Save,
            Open
        }
    }
}
