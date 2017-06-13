using System;
using System.IO;
using System.IO.Compression;
using VariantAnnotation.DataStructures.ProteinFunction;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling.PredictionCache
{
    public sealed class PredictionCacheWriter : IDisposable
    {
        #region members

        private readonly BinaryWriter _writer;
        private readonly BlockStream _blockStream;
        private readonly PredictionCacheHeader _header;

        #endregion

        #region IDisposable

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (disposing) _writer.Dispose();
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public PredictionCacheWriter(string dbPath, PredictionCacheHeader header)
        {
            _blockStream = new BlockStream(new Zstandard(), FileUtilities.GetCreateStream(dbPath),
                CompressionMode.Compress);

            _writer = new BinaryWriter(_blockStream);
            _header = header;
        }

        /// <summary>
        /// writes the annotations to the current database file
        /// </summary>
        public void Write(Prediction.Entry[] lut, Prediction[][] predictionsPerRef)
        {
            // write the header
            _blockStream.WriteHeader(_header);

            // write the LUT
            WriteLookupTable(_writer, lut);
            _blockStream.Flush();

            // write the predictions
            WritePredictions(predictionsPerRef);
        }

        private void WritePredictions(Prediction[][] predictionsPerRef)
        {
            var indexEntries = _header.Index.Entries;
            var blockPosition = new BlockStream.BlockPosition();

            for (int i = 0; i < predictionsPerRef.Length; i++)
            {
	            var refPredictions = predictionsPerRef[i];

				_blockStream.GetBlockPosition(blockPosition);
                indexEntries[i].FileOffset = blockPosition.FileOffset;
                indexEntries[i].Count      = refPredictions.Length;

                foreach (var prediction in refPredictions) prediction.Write(_writer);
                _blockStream.Flush();
            }
        }

        private static void WriteLookupTable(BinaryWriter writer, Prediction.Entry[] lut)
        {
            writer.Write(lut.Length);
            foreach (var entry in lut) entry.Write(writer);
        }
    }
}
