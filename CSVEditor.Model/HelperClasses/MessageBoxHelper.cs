using System.Windows;

namespace CSVEditor.Model.HelperClasses
{
    public class MessageBoxHelper
    {
        public static MessageBoxResult ShowProcessErrorBox(string title, string content)
        {
            return MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static MessageBoxResult ShowQueryYesNoCancelBox(string title, string content)
        {
            return MessageBox.Show(content, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        }
    }
}
