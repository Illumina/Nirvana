using System;
using System.IO;
using VariantAnnotation.Utilities;

namespace CacheUtils.Utilities
{
    internal static class AccessionUtilities
    {
        internal static (string Id, byte Version) GetMaxVersion(string originalId, byte originalVersion)
        {
            var (pureId, idVersion) = FormatUtilities.SplitVersion(originalId);
            return (pureId, Math.Max(originalVersion, idVersion));
        }

        public static int GetAccessionNumber(string s)
        {
            if (string.IsNullOrEmpty(s)) return -1;
            return s.StartsWith("ENS") ? GetEnsemblAccessionNumber(s) : GetRefSeqAccessionNumber(s);
        }

        private static int GetRefSeqAccessionNumber(string s)
        {
            int firstUnderlinePos = s.IndexOf('_');
            if (firstUnderlinePos == -1) throw new InvalidDataException("Expected an underline in the transcript ID, but didn't find any.");

            string id = s.Substring(firstUnderlinePos + 1);
            return int.Parse(id);
        }

        private static int GetEnsemblAccessionNumber(string s)
        {
            string id = s.Substring(4);
            return int.Parse(id);
        }
    }
}
