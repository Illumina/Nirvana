using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures.CytogeneticBands;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling.Reference
{
    public sealed class CompressedSequenceWriter : IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;
        private readonly FileStream _stream;
        private List<SequenceIndexEntry> _refSeqIndex;
        private List<List<MaskedEntry>> _refSeqMaskedIntervals;
        private TwoBitSequence _twoBitSequence;
        private SequenceCompressor _sequenceCompressor;
        private long _headerDataOffset;
        private long _indexOffset;
        private long _maskedIntervalsOffset;

        /// <summary>
        /// constructor
        /// </summary>
        public CompressedSequenceWriter(string path, List<ReferenceMetadata> referenceMetadataList,
            ICytogeneticBands genomeCytobands, GenomeAssembly genomeAssembly)
        {
            _stream                      = FileUtilities.GetCreateStream(path);
            _writer                      = new ExtendedBinaryWriter(_stream);
            _refSeqIndex                 = new List<SequenceIndexEntry>();
            _twoBitSequence              = new TwoBitSequence();
            _sequenceCompressor = new SequenceCompressor();
            _refSeqMaskedIntervals       = new List<List<MaskedEntry>>();

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

            _refSeqIndex                 = null;
            _refSeqMaskedIntervals       = null;
            _twoBitSequence              = null;
            _sequenceCompressor = null;

            _headerDataOffset            = 0L;
            _indexOffset                 = 0L;
            _maskedIntervalsOffset       = 0L;
        }

        public void Write(string name, string bases, int seqOffset = 0)
        {
            _writer.Flush();
            long position = _stream.Position;

            _refSeqIndex.Add(new SequenceIndexEntry
            {
                Name           = name,
                NumBases       = bases.Length,
                FileOffset     = position,
                SequenceOffset = seqOffset
            });

            _sequenceCompressor.Compress(bases, _twoBitSequence);

            // sort the masked intervals
            var sortedMaskedIntervals =
                _twoBitSequence.MaskedIntervals.OrderBy(x => x.Begin).ThenBy(x => x.End).ToList();

            _refSeqMaskedIntervals.Add(sortedMaskedIntervals);
            _writer.Write(_twoBitSequence.Buffer, 0, _twoBitSequence.NumBufferBytes);
        }

        private void WriteReferenceSequenceIndex()
        {
            _writer.Flush();
            _indexOffset = _stream.Position;

            _writer.WriteOpt(_refSeqIndex.Count);

            foreach (var sequenceIndexEntry in _refSeqIndex)
            {
                _writer.WriteOptAscii(sequenceIndexEntry.Name);
                _writer.WriteOpt(EncodeNumBases(sequenceIndexEntry.NumBases, sequenceIndexEntry.SequenceOffset));
                _writer.WriteOpt(sequenceIndexEntry.FileOffset);
                if (sequenceIndexEntry.SequenceOffset != 0) _writer.WriteOpt(sequenceIndexEntry.SequenceOffset);
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

        private void WriteHeader(List<ReferenceMetadata> referenceMetadataList, ICytogeneticBands genomeCytobands, GenomeAssembly genomeAssembly)
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
