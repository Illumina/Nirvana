using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.PredictionCache;
using VariantAnnotation.Interface;

namespace CacheUtils.UpdateMiniCacheFiles.DataStructures
{
    /// <summary>
    /// the bundle of cache and reference data objects that correspond to a 
    /// specific genome assembly and transcript data source
    /// </summary>
    public class DataBundle
    {
        public CompressedSequence Sequence;
        public CompressedSequenceReader SequenceReader;
        public PredictionCacheReader SiftReader;
        public PredictionCacheReader PolyPhenReader;

        public PredictionCache SiftCache;
        public PredictionCache PolyPhenCache;
        public GlobalCache Cache;

        public IIntervalForest<Transcript> TranscriptForest;

        public ushort CurrentRefIndex = ushort.MaxValue;

        public void Load(ushort refIndex)
        {
            if (refIndex == CurrentRefIndex) return;

            SequenceReader.GetCompressedSequence(Sequence.Renamer.EnsemblReferenceNames[refIndex]);
            SiftCache     = SiftReader.Read(refIndex);
            PolyPhenCache = PolyPhenReader.Read(refIndex);

            CurrentRefIndex = refIndex;
        }
    }
}
