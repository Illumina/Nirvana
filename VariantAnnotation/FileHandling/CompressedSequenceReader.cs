using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.CytogeneticBands;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling
{
    public sealed class CompressedSequenceReader : IDisposable
    {
        #region members

        // buffer specific
        public const int MaxShift = 6;

        private readonly ExtendedBinaryReader _reader;
        private readonly BinaryReader _binaryReader;
        private readonly Stream _stream;

        private Dictionary<string, int> _nameToIndex;

        private List<SequenceIndexEntry> _refSeqIndex;
        private List<ReferenceMetadata> RefMetadataList { get; set; }

        private long _dataStartOffset;
        private long _indexOffset;

        // ReSharper disable once NotAccessedField.Local
        private long _maskedIntervalsOffset;

        public ICytogeneticBands CytogeneticBands { get; private set; }
        public GenomeAssembly GenomeAssembly { get; private set; }
        private ChromosomeRenamer _chromosomeRenamer;

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
                _binaryReader.Dispose();
            }

            // Free any unmanaged objects here
        }

        #endregion

        /// <summary>
        /// file constructor
        /// </summary>
        public CompressedSequenceReader(string path)
        {
            if (!File.Exists(path))
            {
                throw new UserErrorException($"The supplied compressed sequence filename ({path}) does not exist.");
            }

            _stream       = FileUtilities.GetFileStream(path);
            _binaryReader = new BinaryReader(_stream);
            _reader       = new ExtendedBinaryReader(_binaryReader);

            Initialize();
        }

        private void Initialize()
        {
            _chromosomeRenamer = AnnotationLoader.Instance.ChromosomeRenamer;
            _nameToIndex       = new Dictionary<string, int>();
            _refSeqIndex       = new List<SequenceIndexEntry>();

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
            string headerTag  = _binaryReader.ReadString();
            int headerVersion = _binaryReader.ReadInt32();

            if ((headerTag != CompressedSequenceCommon.HeaderTag) ||
                (headerVersion != CompressedSequenceCommon.HeaderVersion))
            {
                throw new UserErrorException($"The header identifiers do not match the expected values: Obs: {headerTag} {headerVersion} vs Exp: {CompressedSequenceCommon.HeaderTag} {CompressedSequenceCommon.HeaderVersion}");
            }
        }

        /// <summary>
        /// returns a 2-bit sequence corresponding to the specified name
        /// </summary>
        public void GetCompressedSequence(string name, ref ICompressedSequence compressedSequence)
        {
            var indexEntry = GetIndexEntry(name);
            if (indexEntry == null) return;

            // jump to that offset
            _stream.Position = indexEntry.Offset;

            // set the data
            int numBufferBytes = CompressedSequence.GetNumBufferBytes(indexEntry.NumBases);
            compressedSequence.Set(indexEntry.NumBases, _reader.ReadBytes(numBufferBytes), indexEntry.MaskedEntries);
        }

        /// <summary>
        /// returns the index entry that corresponds to the specified name. Returns null if the
        /// sequence doesn't exist.
        /// </summary>
        private SequenceIndexEntry GetIndexEntry(string name)
        {
            // grab the reference index
            int refIndex;
            if (!_nameToIndex.TryGetValue(name, out refIndex)) return null;
            return _refSeqIndex[refIndex];
        }

        /// <summary>
        /// loads the header
        /// </summary>
        private void LoadHeader()
        {
            // grab the index and masked intervals offsets
            _indexOffset = _binaryReader.ReadInt64();
            _maskedIntervalsOffset = _binaryReader.ReadInt64();

            // grab the creation time
            _reader.ReadLong();

            // grab the reference metadata
            RefMetadataList = new List<ReferenceMetadata>();
            int numRefSeqs = _reader.ReadInt();

            for (int i = 0; i < numRefSeqs; i++)
            {
                RefMetadataList.Add(ReferenceMetadata.Read(_reader));
            }

            // update the chromosome renamer
            var chromosomeRenamer = AnnotationLoader.Instance.ChromosomeRenamer;
            chromosomeRenamer.AddReferenceMetadata(RefMetadataList);

            // read the cytogenetic bands
            var cytogeneticBands = DataStructures.CytogeneticBands.CytogeneticBands.Read(_reader);
            CytogeneticBands = new CytogeneticBands(chromosomeRenamer.EnsemblReferenceSequenceIndex, cytogeneticBands);

            // read the genome assembly
            GenomeAssembly = (GenomeAssembly)_reader.ReadByte();

            // grab the data start tag
            ulong dataStartTag = _binaryReader.ReadUInt64();

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
            int numRefSeqs = _reader.ReadInt();

            // read the index
            for (int refIndex = 0; refIndex < numRefSeqs; refIndex++)
            {
                string refName  = _chromosomeRenamer.GetEnsemblReferenceName(_reader.ReadAsciiString());
                int numBases    = _reader.ReadInt();
                long refOffset  = _reader.ReadLong();
                
                _refSeqIndex.Add(new SequenceIndexEntry { NumBases = numBases, Offset = refOffset });
                _nameToIndex[refName] = refIndex;
            }
        }

        /// <summary>
        /// loads the masked intervals
        /// </summary>
        private void LoadMaskedIntervals()
        {
            // grab the number of reference sequences
            int numRefSeqs = _reader.ReadInt();

            // read the index
            for (int refIndex = 0; refIndex < numRefSeqs; refIndex++)
            {
                int numMaskedIntervals = _reader.ReadInt();
                var maskedIntervalTree = new IntervalTree<MaskedEntry>();

                for (int intervalIndex = 0; intervalIndex < numMaskedIntervals; intervalIndex++)
                {
                    int begin = _reader.ReadInt();
                    int end = _reader.ReadInt();

                    maskedIntervalTree.Add(new IntervalTree<MaskedEntry>.Interval(string.Empty, begin, end, new MaskedEntry(begin, end)));
                }

                _refSeqIndex[refIndex].MaskedEntries = maskedIntervalTree;
            }
        }

        /// <summary>
        /// verify our EOF tag
        /// </summary>
        private void VerifyEofTag()
        {
            // verify our EOF tag
            ulong eofTag = _binaryReader.ReadUInt64();

            if (eofTag != CompressedSequenceCommon.EofTag)
            {
                throw new UserErrorException($"The EOF tag does not match the expected values: Obs: {eofTag} vs Exp: {CompressedSequenceCommon.EofTag}");
            }
        }
    }
}
