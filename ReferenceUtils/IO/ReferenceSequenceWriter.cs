using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
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

        public void Write(List<CompressionBlock> blocks)
        {
            _writer.Flush();

            long indexOffset = _stream.Position;
            int indexSize    = 8 + IndexEntry.Size * blocks.Count;
            var index        = CreateIndex(blocks, indexOffset, indexSize);

            WriteIndex(index);
            _writer.Flush();

            WriteBlocks(blocks);
        }

        private static IndexEntry[] CreateIndex(IReadOnlyCollection<CompressionBlock> blocks, long indexOffset, int indexSize)
        {
            var indexEntries     = new IndexEntry[blocks.Count];
            long referenceOffset = indexOffset + indexSize;

            var index = 0;
            foreach (var block in blocks)
            {
                indexEntries[index] = new IndexEntry(block.RefIndex, referenceOffset);

                int blockSize = block.BufferSize + 12;
                referenceOffset += blockSize;
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

        private void WriteBlocks(IEnumerable<CompressionBlock> blocks)
        {
            foreach (var block in blocks)
            {
                _writer.Write(ReferenceSequenceCommon.ReferenceStartTag);
                _writer.Write(block.UncompressedBufferSize);
                _writer.Write(block.CompressedBufferSize);
                _writer.Write(block.Buffer, 0, block.BufferSize);
            }
        }
    }
}