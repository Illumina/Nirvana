using System.IO;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.TranscriptCache;

namespace GenerateVcfFromOmimTests
{
	public static class DataStoreUtilities
	{
		public static GlobalCache GetTranscriptCache(string cachePath)
		{
			// sanity check: make sure the path exists
			if (!File.Exists(cachePath)) return null;

            GlobalCache cache;
            using (var reader = new GlobalCacheReader(cachePath)) cache = reader.Read();
		    return cache;
		}
	}
}