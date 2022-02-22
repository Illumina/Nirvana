using System;

namespace VariantAnnotation.Utilities
{
    public static class Date
    {
        public static string CurrentTimeStamp => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}