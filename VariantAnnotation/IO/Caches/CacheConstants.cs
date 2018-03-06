namespace VariantAnnotation.IO.Caches
{
	public static class CacheConstants
	{
		public const uint GuardInt     = 4041327495; // 87c3e1f0
		public const string Identifier = "NirvanaDB";

		// increment the schema version when the file structures are updated
		// N.B. we only need to regenerate unit tests when the schema version is incremented
		// e.g. adding a new feature like regulatory elements
		public const ushort SchemaVersion = 21;

		// increment the data version when the contents are updated
		// e.g. a bug is fixed in SIFT parsing or if transcripts are filtered differently
		public const ushort DataVersion = 26;

		public static string TranscriptPath(string prefix) => Combine(prefix, ".transcripts.ndb");
		public static string SiftPath(string prefix)       => Combine(prefix, ".sift.ndb");
        public static string PolyPhenPath(string prefix)   => Combine(prefix, ".polyphen.ndb");
	    public static string BasesPath(string prefix)      => Combine(prefix, ".bases");

        private static string Combine(string prefix, string suffix) => prefix == null ? null : prefix + suffix;
	}
}