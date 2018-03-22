using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ErrorHandling.Exceptions;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace VariantAnnotation.Sequence
{
    public sealed class CompressedSequenceReader : IDisposable
    {
        public readonly Dictionary<string, IChromosome> RefNameToChromosome;
        public readonly Dictionary<ushort, IChromosome> RefIndexToChromosome;

        public Band[][] CytogeneticBands { get; private set; }

        // buffer specific
        public const int MaxShift = 6;

        private readonly ExtendedBinaryReader _reader;
        private readonly Stream _stream;
        public readonly CompressedSequence Sequence;
        public GenomeAssembly Assembly => Sequence.GenomeAssembly;

        private readonly Dictionary<string, int> _nameToIndex         = new Dictionary<string, int>();
        private readonly List<SequenceIndexEntry> _refSeqIndex        = new List<SequenceIndexEntry>();
        public readonly List<ReferenceMetadata> ReferenceMetadataList = new List<ReferenceMetadata>();

        private long _dataStartOffset;
        private long _indexOffset;
        public ushort NumRefSeqs { get; private set; }
        public const ushort UnknownReferenceIndex = ushort.MaxValue;

        // ReSharper disable once NotAccessedField.Local
        private long _maskedIntervalsOffset;

        public CompressedSequenceReader(Stream stream)
        {
            _stream              = stream;
            _reader              = new ExtendedBinaryReader(stream);
            Sequence             = new CompressedSequence();
            RefNameToChromosome  = new Dictionary<string, IChromosome>();
            RefIndexToChromosome = new Dictionary<ushort, IChromosome>();

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

        public void Dispose()
        {
            _reader.Dispose();
            _stream.Dispose();
        }

        /// <summary>
        /// checks the header version
        /// </summary>
        private void CheckHeaderVersion()
        {
            string headerTag  = _reader.ReadString();
            int headerVersion = _reader.ReadInt32();

            if (headerTag != CompressedSequenceCommon.HeaderTag ||
                headerVersion != CompressedSequenceCommon.HeaderVersion)
            {
                throw new InvalidFileFormatException($"The header identifiers do not match the expected values: Obs: {headerTag} {headerVersion} vs Exp: {CompressedSequenceCommon.HeaderTag} {CompressedSequenceCommon.HeaderVersion}");
            }
        }

        /// <summary>
        /// returns a 2-bit sequence corresponding to the specified name
        /// </summary>
        public void GetCompressedSequence(IChromosome chromosome)
        {
            var indexEntry = GetIndexEntry(chromosome.EnsemblName);
            if (indexEntry == null) return;

            // jump to that offset
            _stream.Position = indexEntry.FileOffset;

            // set the data
            var numBufferBytes = CompressedSequence.GetNumBufferBytes(indexEntry.NumBases);
            Sequence.Set(indexEntry.NumBases, _reader.ReadBytes(numBufferBytes), indexEntry.MaskedEntries, indexEntry.SequenceOffset);
        }

        /// <summary>
        /// returns the index entry that corresponds to the specified name. Returns null if the
        /// sequence doesn't exist.
        /// </summary>
        private SequenceIndexEntry GetIndexEntry(string ensemblReferenceName)
        {
            return !_nameToIndex.TryGetValue(ensemblReferenceName, out var refIndex) ? null : _refSeqIndex[refIndex];
        }

        /// <summary>
        /// loads the header
        /// </summary>
        private void LoadHeader()
        {
            // grab the index and masked intervals offsets
            _indexOffset = _reader.ReadInt64();
            _maskedIntervalsOffset = _reader.ReadInt64();

            // skip the creation time
            _reader.ReadOptInt64();

            // grab the reference metadata
            NumRefSeqs = (ushort)_reader.ReadOptInt32();

            for (ushort i = 0; i < NumRefSeqs; i++)
            {
                var metadata = ReferenceMetadata.Read(_reader);
                ReferenceMetadataList.Add(metadata);
                AddReferenceName(metadata.EnsemblName, metadata.UcscName, i);
            }

            // read the cytogenetic bands
            CytogeneticBands = VariantAnnotation.Sequence.CytogeneticBands.Read(_reader);

            // read the genome assembly
            Sequence.GenomeAssembly = (GenomeAssembly)_reader.ReadByte();

            // grab the data start tag
            ulong dataStartTag = _reader.ReadUInt64();

            if (dataStartTag != CompressedSequenceCommon.DataStartTag)
            {
                throw new InvalidFileFormatException($"The data start tag does not match the expected values: Obs: {dataStartTag} vs Exp: {CompressedSequenceCommon.DataStartTag}");
            }

            _dataStartOffset = _stream.Position;
        }

        /// <summary>
        /// adds a Ensembl/UCSC reference name pair to the current dictionary
        /// </summary>
        private void AddReferenceName(string ensemblReferenceName, string ucscReferenceName, ushort refIndex)
        {
            var isUcscEmpty    = string.IsNullOrEmpty(ucscReferenceName);
            var isEnsemblEmpty = string.IsNullOrEmpty(ensemblReferenceName);

            // sanity check: make sure we have at least one reference name
            if (isUcscEmpty && isEnsemblEmpty) return;

            if (isUcscEmpty) ucscReferenceName = ensemblReferenceName;
            if (isEnsemblEmpty) ensemblReferenceName = ucscReferenceName;

            var chromosome = new Chromosome(ucscReferenceName, ensemblReferenceName, refIndex);

            RefNameToChromosome[ucscReferenceName]    = chromosome;
            RefNameToChromosome[ensemblReferenceName] = chromosome;
            RefIndexToChromosome[refIndex]            = chromosome;
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
                string name         = _reader.ReadAsciiString();
                int encodedNumBases = _reader.ReadOptInt32();
                int numBases        = encodedNumBases & CompressedSequenceCommon.NumBasesMask;
                long fileOffset     = _reader.ReadOptInt64();
                var sequenceOffset  = CompressedSequenceCommon.HasSequenceOffset(encodedNumBases) ? _reader.ReadOptInt32() : 0;

                _refSeqIndex.Add(new SequenceIndexEntry { NumBases = numBases, FileOffset = fileOffset, SequenceOffset = sequenceOffset });
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
                var maskedIntervals    = new List<Interval<MaskedEntry>>();

                for (int intervalIndex = 0; intervalIndex < numMaskedIntervals; intervalIndex++)
                {
                    int begin = _reader.ReadOptInt32();
                    int end   = _reader.ReadOptInt32();

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
                throw new InvalidFilterCriteriaException($"The EOF tag does not match the expected values: Obs: {eofTag} vs Exp: {CompressedSequenceCommon.EofTag}");
            }
        }
    }
}