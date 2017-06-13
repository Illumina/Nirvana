using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.CytogeneticBands;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.FileHandling.Binary;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling
{
    public sealed class CompressedSequenceReader : IDisposable
    {
        #region members

        public readonly List<ReferenceMetadata> Metadata = new List<ReferenceMetadata>();

        // buffer specific
        public const int MaxShift = 6;

        private readonly ExtendedBinaryReader _reader;
        private readonly Stream _stream;
        private readonly ICompressedSequence _compressedSequence;
	    public GenomeAssembly Assembly => _compressedSequence.GenomeAssembly;

        private readonly Dictionary<string, int> _nameToIndex  = new Dictionary<string, int>();
        private readonly List<SequenceIndexEntry> _refSeqIndex = new List<SequenceIndexEntry>();

        private long _dataStartOffset;
        private long _indexOffset;

        // ReSharper disable once NotAccessedField.Local
        private long _maskedIntervalsOffset;

        #endregion

        #region IDisposable

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free any other managed objects here
                _stream.Dispose();
            }

            // Free any unmanaged objects here
        }

        #endregion

        /// <summary>
        /// stream constructor
        /// </summary>
        public CompressedSequenceReader(Stream stream, ICompressedSequence compressedSequence)
        {
            _stream = stream;
            _reader = new ExtendedBinaryReader(stream);
            _compressedSequence = compressedSequence;

            CheckHeaderVersion();
            LoadHeader();

            // jump to the index offset
            _stream.Position = _indexOffset;

            LoadIndex();
            LoadMaskedIntervals();
            VerifyEofTag();

            // jump back to the data start position
            _stream.Position = _dataStartOffset;
			
        }

        /// <summary>
        /// checks the header version
        /// </summary>
        private void CheckHeaderVersion()
        {
            string headerTag  = _reader.ReadString();
            int headerVersion = _reader.ReadInt32();

            if (headerTag     != CompressedSequenceCommon.HeaderTag ||
                headerVersion != CompressedSequenceCommon.HeaderVersion)
            {
                throw new UserErrorException($"The header identifiers do not match the expected values: Obs: {headerTag} {headerVersion} vs Exp: {CompressedSequenceCommon.HeaderTag} {CompressedSequenceCommon.HeaderVersion}");
            }
        }

        /// <summary>
        /// returns a 2-bit sequence corresponding to the specified name
        /// </summary>
        public void GetCompressedSequence(string ensemblReferenceName)
        {
            var indexEntry = GetIndexEntry(ensemblReferenceName);
            if (indexEntry == null) return;

            // jump to that offset
            _stream.Position = indexEntry.FileOffset;

            // set the data
            var numBufferBytes = CompressedSequence.GetNumBufferBytes(indexEntry.NumBases);
            _compressedSequence.Set(indexEntry.NumBases, _reader.ReadBytes(numBufferBytes), indexEntry.MaskedEntries, indexEntry.SequenceOffset);
        }

        /// <summary>
        /// returns the index entry that corresponds to the specified name. Returns null if the
        /// sequence doesn't exist.
        /// </summary>
        private SequenceIndexEntry GetIndexEntry(string ensemblReferenceName)
        {
            int refIndex;
            if (!_nameToIndex.TryGetValue(ensemblReferenceName, out refIndex)) return null;
            return _refSeqIndex[refIndex];
        }

        /// <summary>
        /// loads the header
        /// </summary>
        private void LoadHeader()
        {
            // grab the index and masked intervals offsets
            _indexOffset           = _reader.ReadInt64();
            _maskedIntervalsOffset = _reader.ReadInt64();

            // skip the creation time
            _reader.ReadOptInt64();

            // grab the reference metadata
            int numRefSeqs = _reader.ReadOptInt32();
            for (int i = 0; i < numRefSeqs; i++) Metadata.Add(ReferenceMetadata.Read(_reader));

            // update the chromosome renamer
            _compressedSequence.Renamer.AddReferenceMetadata(Metadata);

            // read the cytogenetic bands
            _compressedSequence.CytogeneticBands = new CytogeneticBands(CytogeneticBands.Read(_reader), _compressedSequence.Renamer);

            // read the genome assembly
            _compressedSequence.GenomeAssembly = (GenomeAssembly)_reader.ReadByte();

            // grab the data start tag
            ulong dataStartTag = _reader.ReadUInt64();

            if (dataStartTag != CompressedSequenceCommon.DataStartTag)
            {
                throw new UserErrorException($"The data start tag does not match the expected values: Obs: {dataStartTag} vs Exp: {CompressedSequenceCommon.DataStartTag}");
            }

            _dataStartOffset = _stream.Position;
        }

        /// <summary>
        /// loads the reference sequence index
        /// </summary>
        private void LoadIndex()
        {
            // grab the number of reference sequences
            var numRefSeqs = _reader.ReadOptInt32();

            // read the index
            for (int refIndex = 0; refIndex < numRefSeqs; refIndex++)
            {
                string name         = _compressedSequence.Renamer.GetEnsemblReferenceName(_reader.ReadAsciiString());
                int encodedNumBases = _reader.ReadOptInt32();
                int numBases        = encodedNumBases & CompressedSequenceCommon.NumBasesMask;
                long fileOffset     = _reader.ReadOptInt64();
                var sequenceOffset  = CompressedSequenceCommon.HasSequenceOffset(encodedNumBases) ? _reader.ReadOptInt32() : 0;

                _refSeqIndex.Add(new SequenceIndexEntry { Name = name, NumBases = numBases, FileOffset = fileOffset, SequenceOffset = sequenceOffset });
                _nameToIndex[name] = refIndex;
            }
        }

        /// <summary>
        /// loads the masked intervals
        /// </summary>
        private void LoadMaskedIntervals()
        {
            // grab the number of reference sequences
            int numRefSeqs = _reader.ReadOptInt32();

            // read the index
            for (int refIndex = 0; refIndex < numRefSeqs; refIndex++)
            {
                int numMaskedIntervals = _reader.ReadOptInt32();
                var maskedIntervals = new List<Interval<MaskedEntry>>();

                for (int intervalIndex = 0; intervalIndex < numMaskedIntervals; intervalIndex++)
                {
                    int begin = _reader.ReadOptInt32();
                    int end = _reader.ReadOptInt32();

                    maskedIntervals.Add(new Interval<MaskedEntry>(begin, end, new MaskedEntry(begin, end)));
                }

                var sortedIntervals = maskedIntervals.OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray();
                _refSeqIndex[refIndex].MaskedEntries = new IntervalArray<MaskedEntry>(sortedIntervals);
            }
        }

        /// <summary>
        /// verify our EOF tag
        /// </summary>
        private void VerifyEofTag()
        {
            // verify our EOF tag
            ulong eofTag = _reader.ReadUInt64();

            if (eofTag != CompressedSequenceCommon.EofTag)
            {
                throw new UserErrorException($"The EOF tag does not match the expected values: Obs: {eofTag} vs Exp: {CompressedSequenceCommon.EofTag}");
            }
        }
		
    }
}
