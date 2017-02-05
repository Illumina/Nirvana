using System.IO;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.FileHandling.PredictionCache;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace CacheUtils.UpdateMiniCacheFiles.Utilities
{
    public static class CacheUtilities
    {
        public static GlobalCache LoadCache(string cachePrefix)
        {
            var cachePath = CacheConstants.TranscriptPath(cachePrefix);
            if (!File.Exists(cachePath)) return null;

            GlobalCache transcriptCache;
            using (var reader = new GlobalCacheReader(FileUtilities.GetReadStream(cachePath)))
            {
                transcriptCache = reader.Read();
            }

            return transcriptCache;
        }

        public static PredictionCacheReader GetPredictionReader(string predictionPath)
        {
            return !File.Exists(predictionPath)
                ? null
                : new PredictionCacheReader(FileUtilities.GetReadStream(predictionPath));
        }

        public static IIntervalForest<Transcript> GetIntervalForest(Transcript[] transcripts, int numRefSeqs)
        {
            return transcripts == null
                ? null
                : IntervalArrayFactory.CreateIntervalForest(transcripts, numRefSeqs);
        }
    }
}
