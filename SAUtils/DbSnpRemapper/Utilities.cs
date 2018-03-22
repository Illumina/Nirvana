using System;
using System.Linq;

namespace SAUtils.DbSnpRemapper
{
    public static class Utilities
    {
        public static long[] GetRsids(string idField)
        {
            var ids = (from idStr in idField.Split(';') where idStr.StartsWith("rs") select Int64.Parse(idStr.Substring(2))).ToArray();

            return ids.Length == 0 ? null : ids;
        }
    }
}