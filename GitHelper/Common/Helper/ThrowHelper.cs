using GitHelper.Common.Exceptions;

namespace GitHelper.Common.Helper
{
    public static class ThrowHelper
    {
        public static void ThrowHandledException(bool predicate, string? message = null)
        {
            if (predicate)
            {
                throw new HandledException(message);
            }
        }
    }
}
