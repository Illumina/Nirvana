using System;
using System.Linq;
using OptimizedCore;

namespace SAUtils.DbSnpRemapper
{
    public static class Utilities
    {
        public static long[] GetRsids(string idField)
        {
            var ids = (from idStr in idField.OptimizedSplit(';') where idStr.StartsWith("rs") select Int64.Parse(idStr.Substring(2))).ToArray();

            return ids.Length == 0 ? null : ids;
        }
    }
}