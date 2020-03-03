using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using ReferenceUtils.Common;
using VariantAnnotation.Sequence;

namespace ReferenceUtils.IO
{
    public sealed class ReferenceSequenceWriter : IDisposable
    {
        private readonly Stream _stream;
        private readonly ExtendedBinaryWriter _writer;

        public ReferenceSequenceWriter(Stream stream, IReadOnlyCollection<IChromosome> chromosomes, GenomeAssembly genomeAssembly, byte patchLevel)
        {
            _stream = stream;
            _writer = new ExtendedBinaryWriter(stream);
            WriteHeader(genomeAssembly, patchLevel, chromosomes);
        }

        public void Dispose() => _writer.Dispose();

        private void WriteHeader(GenomeAssembly genomeAssembly, byte patchLevel, IReadOnlyCollection<IChromosome> chromosomes)
        {
            _writer.Write(ReferenceSequenceCommon.HeaderTag);
            _writer.Write(ReferenceSequenceCommon.HeaderVersion);
            _writer.Write((byte)genomeAssembly);
            _writer.Write(patchLevel);

            _writer.WriteOpt(chromosomes.Count);
            foreach (var chromosome in chromosomes) chromosome.Write(_writer);
        }

        public void Write(List<ReferenceSequence> referenceSequences)
        {
            _writer.Flush();

            long indexOffset = _stream.Position;
            int  indexSize   = 8 + IndexEntry.Size * referenceSequences.Count;

            var    buffers  = new List<ReferenceBuffer>(referenceSequences.Count);
            ushort refIndex = 0;

            foreach (var referenceSequence in referenceSequences)
            {
                buffers.Add(referenceSequence.GetReferenceBuffer(refIndex));
                refIndex++;
            }

            var index = CreateIndex(buffers, indexOffset, indexSize);

            WriteIndex(index);
            WriteReferenceBuffers(buffers);
        }

        private static IndexEntry[] CreateIndex(List<ReferenceBuffer> referenceBuffers, long indexOffset, int indexSize)
        {
            var indexEntries     = new IndexEntry[referenceBuffers.Count];
            long referenceOffset = indexOffset + indexSize;

            var index = 0;
            foreach (var block in referenceBuffers)
            {
                indexEntries[index] = new IndexEntry(block.RefIndex, referenceOffset);
                referenceOffset += block.BufferSize;
                index++;
            }

            return indexEntries;
        }

        private void WriteIndex(IReadOnlyCollection<IndexEntry> indexEntries)
        {
            _writer.Write(ReferenceSequenceCommon.IndexStartTag);
            _writer.Write(indexEntries.Count);

            foreach (var indexEntry in indexEntries)
            {
                _writer.Write(indexEntry.RefIndex);
                _writer.Write(indexEntry.FileOffset);
            }
        }

        private void WriteReferenceBuffers(IEnumerable<ReferenceBuffer> referenceBuffers)
        {
            foreach (var referenceBuffer in referenceBuffers)
            {
                _writer.Write(referenceBuffer.Buffer, 0, referenceBuffer.BufferSize);
            }
        }
    }
}