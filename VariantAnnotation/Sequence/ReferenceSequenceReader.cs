using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using Genome;
using Intervals;
using IO;

namespace VariantAnnotation.Sequence
{
    public sealed class CompressedSequenceReader : IDisposable
    {
        public readonly Dictionary<string, IChromosome> RefNameToChromosome  = new Dictionary<string, IChromosome>();
        public readonly Dictionary<ushort, IChromosome> RefIndexToChromosome = new Dictionary<ushort, IChromosome>();
        private readonly Dictionary<ushort, int> _refIndexToIndex            = new Dictionary<ushort, int>();

        private readonly IndexEntry[] _indexEntries;
        public readonly CompressedSequence Sequence = new CompressedSequence();

        public ushort NumRefSeqs { get; private set; }

        public const int MaxShift = 6;

        private readonly ExtendedBinaryReader _reader;
        private readonly Stream _stream;

        public GenomeAssembly Assembly => Sequence.Assembly;
        public byte PatchLevel; // we'll use this in future version providers

        public CompressedSequenceReader(Stream stream)
        {
            _stream = stream;
            _reader = new ExtendedBinaryReader(stream);

            CheckHeaderVersion();
            LoadHeader();
            GetChromosomes();
            _indexEntries = LoadIndex();
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _stream?.Dispose();
        }

        private void CheckHeaderVersion()
        {
            string headerTag  = _reader.ReadString();
            int headerVersion = _reader.ReadInt32();

            if (headerTag != ReferenceSequenceCommon.HeaderTag || headerVersion != ReferenceSequenceCommon.HeaderVersion)
            {
                throw new InvalidFileFormatException($"The header identifiers do not match the expected values: Obs: {headerTag} {headerVersion} vs Exp: {ReferenceSequenceCommon.HeaderTag} {ReferenceSequenceCommon.HeaderVersion}");
            }
        }

        public void GetCompressedSequence(IChromosome chromosome)
        {
            if (chromosome.IsEmpty() || !_refIndexToIndex.TryGetValue(chromosome.Index, out int index))
            {
                Sequence.EnableNSequence();
                return;
            }

            var indexEntry   = _indexEntries[index];
            _stream.Position = indexEntry.FileOffset;

            uint tag = _reader.ReadUInt32();

            if (tag != ReferenceSequenceCommon.ReferenceStartTag)
            {
                throw new InvalidDataException($"The reference start tag does not match the expected values: Obs: {tag} vs Exp: {ReferenceSequenceCommon.ReferenceStartTag}");
            }

            int uncompressedBufferSize = _reader.ReadInt32();
            int compressedBufferSize   = _reader.ReadInt32();

            var buffer = CompressionBlock.Read(_stream, uncompressedBufferSize, compressedBufferSize);
            var reader = new MemoryBufferBinaryReader(buffer);

            var metadata                 = GetMetadata(reader);
            var twoBitBuffer             = GetTwoBitBuffer(reader);
            var maskedEntryIntervalArray = GetMaskedEntries(reader);
            var cytogeneticBands         = GetCytogeneticBands(reader);

            Sequence.Set(metadata.NumBases, metadata.SequenceOffset, twoBitBuffer, maskedEntryIntervalArray,
                cytogeneticBands);
        }

        private (int SequenceOffset, int NumBases) GetMetadata(MemoryBufferBinaryReader reader)
        {
            int sequenceOffset = reader.ReadOptInt32();
            int numBases       = reader.ReadOptInt32();
            return (sequenceOffset, numBases);
        }

        private static Band[] GetCytogeneticBands(MemoryBufferBinaryReader reader)
        {
            int numBands = reader.ReadOptInt32();
            var bands    = new Band[numBands];

            for (var i = 0; i < numBands; i++)
            {
                int begin   = reader.ReadOptInt32();
                int end     = reader.ReadOptInt32();
                string name = reader.ReadAsciiString();

                bands[i] = new Band(begin, end, name);
            }

            return bands;
        }

        private static IntervalArray<MaskedEntry> GetMaskedEntries(MemoryBufferBinaryReader reader)
        {
            int numEntries    = reader.ReadOptInt32();
            var maskedEntries = new Interval<MaskedEntry>[numEntries];

            for (var i = 0; i < numEntries; i++)
            {
                int begin = reader.ReadOptInt32();
                int end   = reader.ReadOptInt32();

                maskedEntries[i] = new Interval<MaskedEntry>(begin, end, new MaskedEntry(begin, end));
            }

            return new IntervalArray<MaskedEntry>(maskedEntries);
        }

        private static byte[] GetTwoBitBuffer(MemoryBufferBinaryReader reader)
        {
            int numBytes = reader.ReadOptInt32();
            return reader.ReadBytes(numBytes);
        }

        private void LoadHeader()
        {
            Sequence.Assembly = (GenomeAssembly)_reader.ReadByte();
            PatchLevel        = _reader.ReadByte();
            NumRefSeqs        = (ushort)_reader.ReadOptInt32();
        }

        private IChromosome[] GetChromosomes()
        {
            var chromosomes = new IChromosome[NumRefSeqs];
            for (var i = 0; i < NumRefSeqs; i++)
            {
                var chromosome = ChromosomeExtensions.Read(_reader);
                AddReferenceName(chromosome);
                chromosomes[i] = chromosome;
            }
            return chromosomes;
        }

        private IndexEntry[] LoadIndex()
        {
            uint tag = _reader.ReadUInt32();

            if (tag != ReferenceSequenceCommon.IndexStartTag)
            {
                throw new InvalidDataException($"The index start tag does not match the expected values: Obs: {tag} vs Exp: {ReferenceSequenceCommon.IndexStartTag}");
            }

            int numEntries = _reader.ReadInt32();

            var indexEntries = new IndexEntry[numEntries];

            for (var i = 0; i < numEntries; i++)
            {
                ushort refIndex = _reader.ReadUInt16();
                long fileOffset = _reader.ReadInt64();
                indexEntries[i] = new IndexEntry(refIndex, fileOffset);

                _refIndexToIndex[refIndex] = i;
            }

            return indexEntries;
        }

        private void AddReferenceName(IChromosome chromosome)
        {
            if (!string.IsNullOrEmpty(chromosome.UcscName))         RefNameToChromosome[chromosome.UcscName]         = chromosome;
            if (!string.IsNullOrEmpty(chromosome.EnsemblName))      RefNameToChromosome[chromosome.EnsemblName]      = chromosome;
            if (!string.IsNullOrEmpty(chromosome.RefSeqAccession))  RefNameToChromosome[chromosome.RefSeqAccession]  = chromosome;
            if (!string.IsNullOrEmpty(chromosome.GenBankAccession)) RefNameToChromosome[chromosome.GenBankAccession] = chromosome;
            RefIndexToChromosome[chromosome.Index] = chromosome;
        }
    }
}