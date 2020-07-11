using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CSVEditor.Model.HelperClasses
{
    public class MessageBoxHelper
    {
        public static MessageBoxResult ShowProcessErrorBox(string title, string content)
        {
            return MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static MessageBoxResult ShowQueryOKCancelBox(string title, string content)
        {
            return MessageBox.Show(content, title, MessageBoxButton.OKCancel, MessageBoxImage.Question);
        }
    }
}
