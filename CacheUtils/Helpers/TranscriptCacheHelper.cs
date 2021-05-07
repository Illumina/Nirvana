using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.Caches;
using VariantAnnotation.IO.Caches;

namespace CacheUtils.Helpers
{
    public static class TranscriptCacheHelper
    {
        public static TranscriptCacheData GetCache(string cachePath,
            IDictionary<ushort, IChromosome> refIndexToChromosome)
        {
            if (!File.Exists(cachePath)) throw new FileNotFoundException($"Could not find {cachePath}");

            TranscriptCacheData cache;
            using (var reader = new TranscriptCacheReader(FileUtilities.GetReadStream(cachePath))) cache = reader.Read(refIndexToChromosome);
            return cache;
        }
    }
}
