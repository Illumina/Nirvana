namespace VariantAnnotation.Utilities
{
    public static class FormatUtilities
    {
        /// <summary>
        /// returns the ID with the version number if one is not already present
        /// </summary>
        public static string GetVersion(string id, byte version)
        {
            return HasVersion(id) ? id : id + "." + version;
        }

        /// <summary>
        /// returns true if the stableId has a version
        /// </summary>
        private static bool HasVersion(string id)
        {
            int lastPeriod = id.LastIndexOf('.');
            if (lastPeriod == -1) return false;

            for (int pos = lastPeriod + 1; pos < id.Length; pos++)
            {
                if (!char.IsDigit(id[pos])) return false;
            }

            return true;
        }
    }
}
