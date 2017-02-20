using System;

namespace VariantAnnotation.Utilities
{
    public static class Date
    {
        public static string GetTimeStamp => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public static string GetDate(long ticks) => new DateTime(ticks).ToString("yyyy-MM-dd");
    }
}
