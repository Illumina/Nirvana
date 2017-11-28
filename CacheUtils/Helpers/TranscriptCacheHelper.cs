using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Caches;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Utilities;

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
