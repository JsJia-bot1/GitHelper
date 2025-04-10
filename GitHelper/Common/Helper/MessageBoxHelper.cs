using System.Windows;

namespace GitHelper.Common.Helper
{
    public static class MessageBoxHelper
    {
        public static void ShowIfTrue(bool predicate, string? message = null)
        {
            if (predicate)
            {
                MessageBox.Show(message);
            }
        }
    }
}
