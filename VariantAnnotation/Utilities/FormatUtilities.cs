namespace VariantAnnotation.Utilities
{
	public static class FormatUtilities
	{
        public static (string Id, byte Version) SplitVersion(string s)
		{
		    if (s == null) return (null, 0);

			int lastPeriodPos = s.LastIndexOf('.');
		    if (lastPeriodPos == -1) return (s, 0);

            string id        = s.Substring(0, lastPeriodPos);
            string remaining = s.Substring(lastPeriodPos + 1);

            return !byte.TryParse(remaining, out byte version) ? (s, (byte)1) : (id, version);
        }
    }
}