using System.IO;
using System.IO.Compression;
using CacheUtils.MiniCache;
using Compression.Algorithms;
using Compression.FileHandling;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.IO.Caches;

namespace CacheUtils.PredictionCache
{
    public sealed class PredictionCacheStaging : IStaging
    {
        private readonly Prediction.Entry[] _lookupTable;
        private readonly Prediction[][] _predictionsPerRef;
        private readonly CacheHeader _header;

        internal PredictionCacheStaging(CacheHeader header, Prediction.Entry[] lut, Prediction[][] predictionsPerRef)
        {
            _header            = header;
            _lookupTable       = lut;
            _predictionsPerRef = predictionsPerRef;
        }

        public void Write(Stream stream)
        {
            using (var blockStream = new BlockStream(new Zstandard(), stream, CompressionMode.Compress))
            using (var writer      = new PredictionCacheWriter(blockStream, _header))
            {
                writer.Write(_lookupTable, _predictionsPerRef);
            }
        }
    }
}
