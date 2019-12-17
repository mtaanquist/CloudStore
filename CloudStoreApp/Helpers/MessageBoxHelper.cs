using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CloudStoreApp.Helpers
{
    public static class MessageBoxHelper
    {
        public static MessageBoxResult ShowErrorMessageBox(string messageBoxText, string caption)
        {
            return MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static MessageBoxResult ShowConfirmMessageBox(string messageBoxText, string caption)
        {
            return MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        }

        public static MessageBoxResult ShowMessageBox(string messageBoxText, string caption)
        {
            return MessageBox.Show(messageBoxText, caption, MessageBoxButton.OKCancel, MessageBoxImage.Information);
        }
    }
}
