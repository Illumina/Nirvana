using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CytogeneticBands;
using VariantAnnotation.FileHandling;

namespace CreateCompressedReference
{
    public sealed class ReferenceSequenceWriter : IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;
        private readonly BinaryWriter _binaryWriter;
        private readonly FileStream _stream;
        private List<ReferenceSequenceIndexEntry> _refSeqIndex;
        private List<List<MaskedEntry>> _refSeqMaskedIntervals;
        private TwoBitSequence _twoBitSequence;
        private ReferenceSequenceCompressor _referenceSequenceCompressor;
        private long _headerDataOffset;
        private long _indexOffset;
        private long _maskedIntervalsOffset;

        public ReferenceSequenceWriter(string path, List<ReferenceMetadata> referenceMetadataList, ICytogeneticBands genomeCytobands, GenomeAssembly genomeAssembly)
        {
            _stream                      = new FileStream(path, FileMode.Create);
            _binaryWriter                = new BinaryWriter(_stream);
            _writer                      = new ExtendedBinaryWriter(_binaryWriter);
            _refSeqIndex                 = new List<ReferenceSequenceIndexEntry>();
            _twoBitSequence              = new TwoBitSequence();
            _referenceSequenceCompressor = new ReferenceSequenceCompressor();
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
                _binaryWriter.Dispose();
            }

            _refSeqIndex                 = null;
            _refSeqMaskedIntervals       = null;
            _twoBitSequence              = null;
            _referenceSequenceCompressor = null;

            _headerDataOffset            = 0L;
            _indexOffset                 = 0L;
            _maskedIntervalsOffset       = 0L;
        }

        public void Write(string name, string bases)
        {
            _binaryWriter.Flush();
            long position = _stream.Position;

            _refSeqIndex.Add(new ReferenceSequenceIndexEntry
            {
                Name = name,
                NumBases = bases.Length,
                Offset = position
            });

            _referenceSequenceCompressor.Compress(bases, _twoBitSequence);

            _refSeqMaskedIntervals.Add(!_twoBitSequence.MaskedIntervalTree.IsEmpty
                ? _twoBitSequence.MaskedIntervalTree.Select(maskedEntry => maskedEntry.Key.Values[0]).ToList()
                : new List<MaskedEntry>());

            _writer.WriteBytes(_twoBitSequence.Buffer, 0, _twoBitSequence.NumBufferBytes);
        }

        private void WriteReferenceSequenceIndex()
        {
            _binaryWriter.Flush();
            _indexOffset = _stream.Position;

            _writer.WriteInt(_refSeqIndex.Count);

            foreach (ReferenceSequenceIndexEntry sequenceIndexEntry in _refSeqIndex)
            {
                _writer.WriteAsciiString(sequenceIndexEntry.Name);
                _writer.WriteInt(sequenceIndexEntry.NumBases);
                _writer.WriteLong(sequenceIndexEntry.Offset);
            }
        }

        private void WriteMaskedIntervals()
        {
            _binaryWriter.Flush();
            _maskedIntervalsOffset = _stream.Position;

            _writer.WriteInt(_refSeqMaskedIntervals.Count);

            foreach (List<MaskedEntry> list in _refSeqMaskedIntervals)
            {
                _writer.WriteInt(list.Count);
                foreach (MaskedEntry maskedEntry in list)
                {
                    _writer.WriteInt(maskedEntry.Begin);
                    _writer.WriteInt(maskedEntry.End);
                }
            }
        }

        private void WriteEofTag()
        {
            _binaryWriter.Write(CompressedSequenceCommon.EofTag);
            _binaryWriter.Flush();

            // update the index and masked intervals offsets
            _stream.Position = _headerDataOffset;
            _binaryWriter.Write(_indexOffset);
            _binaryWriter.Write(_maskedIntervalsOffset);
        }

        private void WriteHeader(List<ReferenceMetadata> referenceMetadataList, ICytogeneticBands genomeCytobands, GenomeAssembly genomeAssembly)
        {
            _binaryWriter.Write(CompressedSequenceCommon.HeaderTag);
            _binaryWriter.Write(CompressedSequenceCommon.HeaderVersion);
            _binaryWriter.Flush();

            _headerDataOffset = _stream.Position;

            // grab the index and masked intervals offsets
            _binaryWriter.Write(_indexOffset);
            _binaryWriter.Write(_maskedIntervalsOffset);

            // grab the creation time
            _writer.WriteLong(DateTime.UtcNow.Ticks);

            // write the reference metadata
            _writer.WriteInt(referenceMetadataList.Count);
            foreach (var refMetadata in referenceMetadataList) refMetadata.Write(_writer);

            // write the genome cytobands
            genomeCytobands.Write(_writer);

            // write the genome assembly
            _writer.WriteByte((byte)genomeAssembly);

            // write the data start tag
            _binaryWriter.Write(CompressedSequenceCommon.DataStartTag);
        }
    }
}
