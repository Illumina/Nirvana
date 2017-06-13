using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using VariantAnnotation.DataStructures.ProteinFunction;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling.PredictionCache
{
    public sealed class PredictionCacheReader : IDisposable
    {
        #region members

        private readonly BinaryReader _reader;
        private readonly BlockStream _blockStream;
        public readonly PredictionCacheHeader FileHeader;

        private readonly Prediction.Entry[] _lut;

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
            if (disposing) _reader.Dispose();
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public PredictionCacheReader(Stream fs)
        {
            _blockStream = new BlockStream(new Zstandard(), fs, CompressionMode.Decompress);
            _reader      = new BinaryReader(_blockStream, Encoding.UTF8, true);

            // read the header
            var headerType = PredictionCacheHeader.GetHeader(0, GenomeAssembly.Unknown, 0);
            FileHeader     = (PredictionCacheHeader)_blockStream.ReadHeader(headerType);

            // read the LUT
            _lut = ReadLookupTable(_reader);
        }

        private static Prediction.Entry[] ReadLookupTable(BinaryReader reader)
        {
            var numEntries = reader.ReadInt32();
            var lut = new Prediction.Entry[numEntries];
            for (int i = 0; i < numEntries; i++) lut[i] = Prediction.Entry.Read(reader);
            return lut;
        }

        /// <summary>
        /// parses the database cache file and populates the specified lists and interval trees
        /// </summary>
        public DataStructures.Transcript.PredictionCache Read(ushort refIndex)
        {
            var indexEntry = GetIndexEntry(refIndex);
            var bp = new BlockStream.BlockPosition { FileOffset = indexEntry.FileOffset };
            _blockStream.SetBlockPosition(bp);
            return DataStructures.Transcript.PredictionCache.Read(_reader, _lut, indexEntry, FileHeader);
        }

	    /// <summary>
        /// returns the file offset for the predictions occurring on the specified reference sequence
        /// </summary>
        private IndexEntry GetIndexEntry(ushort refIndex)
        {
            return FileHeader.Index.Entries[refIndex];
        }
    }
}
