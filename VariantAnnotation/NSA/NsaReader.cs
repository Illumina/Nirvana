using System;
using System.Collections.Generic;
using System.IO;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.SA;

namespace VariantAnnotation.NSA
{
    public sealed class AnnotationItem
    {
        public readonly int Position;
        public readonly byte[] Data;

        public AnnotationItem(int position, byte[] data)
        {
            Position = position;
            Data = data;
        }
    }

    public sealed class NsaReader : INsaReader
    {
        private readonly Stream _stream;
        private readonly ExtendedBinaryReader _reader;
        public GenomeAssembly Assembly { get; }
        private readonly NsaIndex _index;
        public IDataSourceVersion Version { get; }

        private readonly NsaBlock _block;

        public string JsonKey { get; }
        public bool MatchByAllele { get; }
        public bool IsArray { get; }
        public bool IsPositional { get; }
        public IEnumerable<ushort> ChromosomeIndices => _index.ChromosomeIndices;
        private readonly List<AnnotationItem> _annotations;
        private readonly int _blockSize;
        
        private ExtendedBinaryReader _annotationReader;
        private MemoryStream         _annotationStream;
        private byte[]               _annotationBuffer;

        
        public NsaReader(Stream dataStream, Stream indexStream, int blockSize = SaCommon.DefaultBlockSize)
        {
            _stream = dataStream;
            _blockSize = blockSize;
            _reader = new ExtendedBinaryReader(_stream);
            _block = new NsaBlock(new Zstandard(), blockSize);

            _index = new NsaIndex(indexStream);
            Assembly = _index.Assembly;
            Version = _index.Version;
            JsonKey = _index.JsonKey;
            MatchByAllele = _index.MatchByAllele;
            IsArray = _index.IsArray;
            IsPositional = _index.IsPositional;

            if (_index.SchemaVersion != SaCommon.SchemaVersion) throw new UserErrorException($"SA schema version mismatch. Expected {SaCommon.SchemaVersion}, observed {_index.SchemaVersion} for {JsonKey}");

            _annotations = new List<AnnotationItem>(64 * 1024);
            _annotationBuffer = new byte[1024*1024];
            _annotationStream = new MemoryStream(_annotationBuffer);
            _annotationReader = new ExtendedBinaryReader(_annotationStream);
        }

        public void PreLoad(IChromosome chrom, List<int> positions)
        {
            if (positions == null || positions.Count == 0) return;

            _annotations.Clear();
            for (var i = 0; i < positions.Count;)
            {
                int position = positions[i];
                long fileLocation = _index.GetFileLocation(chrom.Index, position);
                if (fileLocation == -1)
                {
                    i++;
                    continue;
                }

                //only reconnect if necessary
                if (_reader.BaseStream.Position != fileLocation)
                    _reader.BaseStream.Position = fileLocation;
                _block.Read(_reader);
                var newIndex = _block.AddAnnotations(positions, i, _annotations);
                if (newIndex == i) i++; //no positions were found in this block
                else i = newIndex;
            }
        }

        public List<NsaIndexBlock> GetIndexBlocks(ushort chromIndex) => _index.GetChromBlocks(chromIndex);

        public bool HasDataBlocks(ushort chromIndex) {
            var (location, _) = _index.GetFileRange(chromIndex, 1, int.MaxValue);
            return location != -1;
        }
        
        public IEnumerable<NsaBlock> GetCompressedBlocks(ushort chromIndex)
        {
            var (location, blockCount) = _index.GetFileRange(chromIndex, 1, int.MaxValue);
            if (location == -1) yield break;

            _reader.BaseStream.Position = location;

            for (var i = 0; i < blockCount; i++)
            {
                var block = new NsaBlock(new Zstandard(), _blockSize);
                block.ReadCompressedBytes(_reader);
                yield return block;
            }
        }

        private void ExtractAnnotations(byte[] data, List<(string refAllele, string altAllele, string annotation)> annotations)
        {
            if (_annotationBuffer.Length < data.Length)
            {
                _annotationBuffer = new byte[2 *data.Length];
                _annotationReader.Dispose();
                _annotationStream?.Dispose();
                _annotationStream = new MemoryStream(_annotationBuffer);
                _annotationReader = new ExtendedBinaryReader(_annotationStream);
            }
            Array.Copy(data, _annotationBuffer, data.Length);
            _annotationStream.Position = 0;
            if (IsPositional)
            {
                var positionalAnno = _annotationReader.ReadString();
                annotations.Add((null, null, positionalAnno));
                return;
            }

            int count       = _annotationReader.ReadOptInt32();
            for (var i = 0; i < count; i++)
            {
                string refAllele  = _annotationReader.ReadAsciiString();
                string altAllele  = _annotationReader.ReadAsciiString();
                string annotation = _annotationReader.ReadString();
                annotations.Add((refAllele ?? "", altAllele ?? "", annotation));
            }
        }

        public void GetAnnotation(int position, List<(string refAllele, string altAllele, string annotation)> annotations)
        {
            annotations.Clear();
            int index = BinarySearch(position);
            if(index < 0) return;
            ExtractAnnotations(_annotations[index].Data, annotations);
        }

        private int BinarySearch(int position)
        {
            var begin = 0;
            int end = _annotations.Count - 1;

            while (begin <= end)
            {
                int index = begin + (end - begin >> 1);

                int ret = _annotations[index].Position.CompareTo(position);
                if (ret == 0) return index;
                if (ret < 0) begin = index + 1;
                else end = index - 1;
            }

            return ~begin;
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _block?.Dispose();
            _annotationStream?.Dispose();
            _annotationReader?.Dispose();
        } 
    }
}