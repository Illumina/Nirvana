using VariantAnnotation.DataStructures.ProteinFunction;
using VariantAnnotation.FileHandling.PredictionCache;

namespace CacheUtils.UpdateMiniCacheFiles
{
    public sealed class PredictionCacheStaging
    {
        private readonly Prediction.Entry[] _lookupTable;
        private readonly Prediction[][] _predictionsPerRef;
        private readonly PredictionCacheHeader _header;

        /// <summary>
        /// constructor
        /// </summary>
        public PredictionCacheStaging(PredictionCacheHeader header, Prediction.Entry[] lut,
            Prediction[][] predictionsPerRef)
        {
            _header            = header;
            _lookupTable       = lut;
            _predictionsPerRef = predictionsPerRef;
        }

        /// <summary>
        /// writes the prediction cache to disk
        /// </summary>
        public void Write(string cachePath)
        {
            using (var writer = new PredictionCacheWriter(cachePath, _header))
            {
                writer.Write(_lookupTable, _predictionsPerRef);
            }
        }
    }
}
