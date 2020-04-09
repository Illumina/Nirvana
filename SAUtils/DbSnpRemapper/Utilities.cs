using System.Linq;
using OptimizedCore;

namespace SAUtils.DbSnpRemapper
{
    public static class Utilities
    {
        public static long[] GetRsids(string idField)
        {
            var ids = idField.OptimizedSplit(',')
                .Where(idStr => idStr.StartsWith("rs"))
                .Select(idStr => long.Parse(idStr.Substring(2))).ToArray();

            return ids.Length == 0 ? null : ids;
        }
    }
}