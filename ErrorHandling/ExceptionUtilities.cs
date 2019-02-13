using System;

namespace ErrorHandling
{
    public static class ExceptionUtilities
    {
        public static bool HasException<T>(Exception e)
        {
            if (e == null) return false;
            return e is T || HasException<T>(e.InnerException);
        }

        public static Exception GetInnermostException(Exception e)
        {
            while (e.InnerException != null) e = e.InnerException;
            return e;
        }
    }
}