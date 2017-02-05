using System;
using VariantAnnotation.DataStructures;

namespace VariantAnnotation.Utilities
{
    public static class FormatUtilities
    {
        public static string CombineIdAndVersion(CompactId id, byte version) => id + "." + version;

        public static Tuple<string, byte> SplitVersion(string id)
        {
            if (id == null) return null;
            int lastPeriod = id.LastIndexOf('.');
            return lastPeriod == -1
                ? new Tuple<string, byte>(id, 0)
                : new Tuple<string, byte>(id.Substring(0, lastPeriod), byte.Parse(id.Substring(lastPeriod + 1)));
        }
    }
}
