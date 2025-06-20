namespace GitHelper.Common.Helper
{
    public static class StringExtension
    {
        public static bool ContainsJiraNo(this string message, string jiraNo)
        {
            int startIndex = message.IndexOf(jiraNo);
            if (startIndex == -1)
            {
                return false;
            }

            // Check next char
            if (char.IsDigit(message[startIndex + jiraNo.Length]))
            {
                return false;
            }
            return true;
        }
    }
}
