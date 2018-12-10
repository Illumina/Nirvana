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

    public sealed class NsaReader :INsaReader
    {
        private readonly ExtendedBinaryReader _reader;
        public GenomeAssembly Assembly { get; }
        private readonly ChunkedIndex _index;
        public IDataSourceVersion Version { get; }

        private readonly NsaBlock _block;

        public string JsonKey { get; }
        public bool MatchByAllele { get; }
        public bool IsArray { get; }
        public bool IsPositional { get; }

        private AnnotationItem[] _annotations;
        private int _annotationsCount;

        public NsaReader(ExtendedBinaryReader reader, Stream indexStream, int blockSize = SaCommon.DefaultBlockSize)
        {
            _reader = reader;
            _block  = new NsaBlock(new Zstandard(), blockSize);

            _index         = new ChunkedIndex(indexStream);
            Assembly       = _index.Assembly;
            Version        = _index.Version;
            JsonKey        = _index.JsonKey;
            MatchByAllele  = _index.MatchByAllele;
            IsArray        = _index.IsArray;
            IsPositional   = _index.IsPositional;

            if (_index.SchemaVersion != SaCommon.SchemaVersion) throw new UserErrorException($"SA schema version mismatch. Expected {SaCommon.SchemaVersion}, observed {_index.SchemaVersion} for {JsonKey}");
        }

        public void PreLoad(IChromosome chrom, List<int> positions)
        {
            if (positions == null || positions.Count == 0) return;

            int readCount = 0;

            _annotations = new AnnotationItem[positions.Count];
            _annotationsCount = 0;
            (long start, _, int blockCount) = _index.GetFileRange(chrom.Index, positions[0], positions[positions.Count-1]);
            if (start == -1) return;
            _reader.BaseStream.Position = start;

            for (int i = 0; i < positions.Count && blockCount >0; blockCount--)
            {
                readCount+= _block.Read(_reader);
                //if (positions[i] < _block.FirstPosition || _block.LastPosition < positions[i]) continue;
                
                foreach ((int position, byte[] data) annotation in _block.GetAnnotations())
                {
                    if (annotation.position < positions[i]) continue;

                    while (i < positions.Count && positions[i] < annotation.position) i++;
                    if (i >= positions.Count) break;

                    var position = positions[i];

                    if (position != annotation.position) continue;

                    _annotations[_annotationsCount++] = new AnnotationItem(position, annotation.data);
                }
            }
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

                var count = reader.ReadOptInt32();
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

        public IEnumerable<(string refAllele, string altAllele, string annotation)> GetAnnotation(IChromosome chrom, int position)
        {
            if (_annotations == null) return null;
            var index = BinarySearch(position);
            return index < 0 ? null : ExtractAnnotations(_annotations[index].Data);
        }

        private int BinarySearch(int position)
        {
            var begin = 0;
            int end = _annotationsCount - 1;

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

    }
}