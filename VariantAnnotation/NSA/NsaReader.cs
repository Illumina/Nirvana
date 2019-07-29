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
        private readonly ChunkedIndex _index;
        public IDataSourceVersion Version { get; }

        private readonly NsaBlock _block;

        public string JsonKey { get; }
        public bool MatchByAllele { get; }
        public bool IsArray { get; }
        public bool IsPositional { get; }

        private readonly List<AnnotationItem> _annotations;
        
        public NsaReader(Stream dataStream, Stream indexStream, int blockSize = SaCommon.DefaultBlockSize)
        {
            _stream = dataStream;
            _reader = new ExtendedBinaryReader(_stream);
            _block = new NsaBlock(new Zstandard(), blockSize);

            _index = new ChunkedIndex(indexStream);
            Assembly = _index.Assembly;
            Version = _index.Version;
            JsonKey = _index.JsonKey;
            MatchByAllele = _index.MatchByAllele;
            IsArray = _index.IsArray;
            IsPositional = _index.IsPositional;

            if (_index.SchemaVersion != SaCommon.SchemaVersion) throw new UserErrorException($"SA schema version mismatch. Expected {SaCommon.SchemaVersion}, observed {_index.SchemaVersion} for {JsonKey}");

            _annotations = new List<AnnotationItem>(64 * 1024);
        }

        public void PreLoad(IChromosome chrom, List<int> positions)
        {
            if (positions == null || positions.Count == 0) return;

            _annotations.Clear();
            for (var i = 0; i < positions.Count; i++)
            {
                int position = positions[i];
                long fileLocation = _index.GetFileLocation(chrom.Index, position);
                if (fileLocation == -1) continue;

                //only reconnect if necessary
                if (_reader.BaseStream.Position != fileLocation)
                    _reader.BaseStream.Position = fileLocation;
                _block.Read(_reader);
                int lastLoadedPositionIndex = LoadAnnotations(positions, i);
                //if there were any positions in the block, the index will move ahead.
                // we need to decrease it by 1 since the loop will increment it.
                if (lastLoadedPositionIndex > i) i = lastLoadedPositionIndex - 1;
            }

        }

        private int LoadAnnotations(List<int> positions, int i)
        {
            foreach (var annotation in _block.GetAnnotations())
            {
                if (annotation.position < positions[i]) continue;

                while (i < positions.Count && positions[i] < annotation.position) i++;
                if (i >= positions.Count) break;

                int position = positions[i];

                if (position != annotation.position) continue;

                _annotations.Add(new AnnotationItem(position, annotation.data));
            }
            return i;
        }

        private IEnumerable<(string refAllele, string altAllele, string jsonString)> ExtractAnnotations(byte[] data)
        {
            using (var reader = new ExtendedBinaryReader(new MemoryStream(data)))
            {
                if (IsPositional)
                {
                    var positionalAnno = reader.ReadString();
                    return new List<(string, string, string)> { (null, null, positionalAnno) };
                }

                int count = reader.ReadOptInt32();
                var annotations = new (string, string, string)[count];
                for (var i = 0; i < count; i++)
                {
                    string refAllele = reader.ReadAsciiString();
                    string altAllele = reader.ReadAsciiString();
                    string annotation = reader.ReadString();
                    annotations[i] = (refAllele ?? "", altAllele ?? "", annotation);
                }

                return annotations;
            }
        }

        public IEnumerable<(string refAllele, string altAllele, string annotation)> GetAnnotation(int position)
        {
            int index = BinarySearch(position);
            return index < 0 ? null : ExtractAnnotations(_annotations[index].Data);
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
        }
    }
}