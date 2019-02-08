using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using IO;
using VariantAnnotation.Sequence;

namespace CacheUtils.Sequence
{
    public sealed class CompressedSequenceWriter : IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;
        private readonly Stream _stream;
        private readonly List<string> _refSeqNames;
        private List<SequenceIndexEntry> _refSeqIndex;
        private List<List<MaskedEntry>> _refSeqMaskedIntervals;
        private TwoBitSequence _twoBitSequence;
        private SequenceCompressor _sequenceCompressor;
        private long _headerDataOffset;
        private long _indexOffset;
        private long _maskedIntervalsOffset;

        public CompressedSequenceWriter(Stream stream, IReadOnlyCollection<ReferenceMetadata> referenceMetadataList,
            ISerializable genomeCytobands, GenomeAssembly genomeAssembly)
        {
            _stream                = stream;
            _writer                = new ExtendedBinaryWriter(_stream);
            _refSeqNames           = new List<string>();
            _refSeqIndex           = new List<SequenceIndexEntry>();
            _twoBitSequence        = new TwoBitSequence();
            _sequenceCompressor    = new SequenceCompressor();
            _refSeqMaskedIntervals = new List<List<MaskedEntry>>();

            WriteHeader(referenceMetadataList, genomeCytobands, genomeAssembly);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                WriteReferenceSequenceIndex();
                WriteMaskedIntervals();
                WriteEofTag();
                _stream.Dispose();
            }

            _refSeqIndex = null;
            _refSeqMaskedIntervals = null;
            _twoBitSequence = null;
            _sequenceCompressor = null;

            _headerDataOffset = 0L;
            _indexOffset = 0L;
            _maskedIntervalsOffset = 0L;
        }

        public void Write(string name, string bases, int seqOffset = 0)
        {
            _writer.Flush();
            long position = _stream.Position;

            _refSeqNames.Add(name);
            _refSeqIndex.Add(new SequenceIndexEntry
            {
                NumBases = bases.Length,
                FileOffset = position,
                SequenceOffset = seqOffset
            });

            _sequenceCompressor.Compress(bases, _twoBitSequence);

            // sort the masked intervals
            var sortedMaskedIntervals = _twoBitSequence.MaskedIntervals.OrderBy(x => x.Begin).ThenBy(x => x.End).ToList();

            _refSeqMaskedIntervals.Add(sortedMaskedIntervals);
            _writer.Write(_twoBitSequence.Buffer, 0, _twoBitSequence.NumBufferBytes);
        }

        private void WriteReferenceSequenceIndex()
        {
            _writer.Flush();
            _indexOffset = _stream.Position;

            _writer.WriteOpt(_refSeqIndex.Count);

            for (var i = 0; i < _refSeqIndex.Count; i++)
            {
                _writer.WriteOptAscii(_refSeqNames[i]);
                var indexEntry = _refSeqIndex[i];

                _writer.WriteOpt(EncodeNumBases(indexEntry.NumBases, indexEntry.SequenceOffset));
                _writer.WriteOpt(indexEntry.FileOffset);
                if (indexEntry.SequenceOffset != 0) _writer.WriteOpt(indexEntry.SequenceOffset);
            }
        }

        private static int EncodeNumBases(int numBases, int sequenceOffset)
        {
            if (sequenceOffset == 0) return numBases;
            return numBases | CompressedSequenceCommon.SequenceOffsetBit;
        }

        private void WriteMaskedIntervals()
        {
            _writer.Flush();
            _maskedIntervalsOffset = _stream.Position;

            _writer.WriteOpt(_refSeqMaskedIntervals.Count);

            foreach (var list in _refSeqMaskedIntervals)
            {
                _writer.WriteOpt(list.Count);
                foreach (var maskedEntry in list)
                {
                    _writer.WriteOpt(maskedEntry.Begin);
                    _writer.WriteOpt(maskedEntry.End);
                }
            }
        }

        private void WriteEofTag()
        {
            _writer.Write(CompressedSequenceCommon.EofTag);
            _writer.Flush();

            // update the index and masked intervals offsets
            _stream.Position = _headerDataOffset;
            _writer.Write(_indexOffset);
            _writer.Write(_maskedIntervalsOffset);
        }

        private void WriteHeader(IReadOnlyCollection<ReferenceMetadata> referenceMetadataList, ISerializable genomeCytobands, GenomeAssembly genomeAssembly)
        {
            _writer.Write(CompressedSequenceCommon.HeaderTag);
            _writer.Write(CompressedSequenceCommon.HeaderVersion);
            _writer.Flush();

            _headerDataOffset = _stream.Position;

            // grab the index and masked intervals offsets
            _writer.Write(_indexOffset);
            _writer.Write(_maskedIntervalsOffset);

            // grab the creation time
            _writer.WriteOpt(DateTime.UtcNow.Ticks);

            // write the reference metadata
            _writer.WriteOpt(referenceMetadataList.Count);
            foreach (var refMetadata in referenceMetadataList) refMetadata.Write(_writer);

            // write the genome cytobands
            genomeCytobands.Write(_writer);

            // write the genome assembly
            _writer.Write((byte)genomeAssembly);

            // write the data start tag
            _writer.Write(CompressedSequenceCommon.DataStartTag);
        }
    }
}
