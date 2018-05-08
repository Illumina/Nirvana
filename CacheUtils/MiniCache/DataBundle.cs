using Genome;
using IO;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Sequence;
using VC = VariantAnnotation.Caches;

namespace CacheUtils.MiniCache
{
    /// <summary>
    /// the bundle of cache and reference data objects that correspond to a 
    /// specific genome assembly and transcript data source
    /// </summary>
    public sealed class DataBundle
    {
        public readonly CompressedSequenceReader SequenceReader;
        public readonly VC.TranscriptCacheData TranscriptCacheData;
        public readonly VC.TranscriptCache TranscriptCache;

        public readonly PredictionCacheReader SiftReader;
        public readonly PredictionCacheReader PolyPhenReader;

        private IChromosome _currentChromosome = new EmptyChromosome(string.Empty);

        public Prediction[] SiftPredictions;
        public Prediction[] PolyPhenPredictions;
        public readonly Source Source;

        private DataBundle(CompressedSequenceReader sequenceReader, PredictionCacheReader siftReader,
            PredictionCacheReader polyPhenReader, VC.TranscriptCacheData cacheData, VC.TranscriptCache transcriptCache,
            Source source)
        {
            SequenceReader      = sequenceReader;
            TranscriptCacheData = cacheData;
            TranscriptCache     = transcriptCache;
            Source              = source;
            SiftReader          = siftReader;
            PolyPhenReader      = polyPhenReader;
        }

        public void Load(IChromosome chromosome)
        {
            if (_currentChromosome.Index == chromosome.Index) return;
            SequenceReader.GetCompressedSequence(chromosome);
            SiftPredictions     = SiftReader.GetPredictions(chromosome.Index);
            PolyPhenPredictions = PolyPhenReader.GetPredictions(chromosome.Index);
            _currentChromosome  = chromosome;
        }

        public static DataBundle GetDataBundle(string referencePath, string cachePrefix)
        {
            var sequenceReader = new CompressedSequenceReader(FileUtilities.GetReadStream(referencePath));
            var siftReader     = new PredictionCacheReader(FileUtilities.GetReadStream(CacheConstants.SiftPath(cachePrefix)), PredictionCacheReader.SiftDescriptions);
            var polyPhenReader = new PredictionCacheReader(FileUtilities.GetReadStream(CacheConstants.PolyPhenPath(cachePrefix)), PredictionCacheReader.PolyphenDescriptions);

            VC.TranscriptCacheData cacheData;
            VC.TranscriptCache cache;
            Source source;

            using (var transcriptReader = new TranscriptCacheReader(FileUtilities.GetReadStream(CacheConstants.TranscriptPath(cachePrefix))))
            {
                cacheData = transcriptReader.Read(sequenceReader.RefIndexToChromosome);
                cache     = cacheData.GetCache();
                source    = transcriptReader.Header.Source;
            }

            return new DataBundle(sequenceReader, siftReader, polyPhenReader, cacheData, cache, source);
        }
    }
}