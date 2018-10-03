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
    public sealed class NsaReader:INsaReader
    {
        private readonly ExtendedBinaryReader _reader;
        public GenomeAssembly Assembly { get; }
        private readonly ChunkedIndex _index;
        public IDataSourceVersion Version { get; }

        private readonly SaReadBlock _block;
        
        public string JsonKey { get; }
        public bool MatchByAllele { get; }
        public bool IsArray { get; }
        public bool IsPositional { get; }
        public readonly int SchemaVersion;

        private Dictionary<int, List<(string, string, string)>> _annotations;

        public NsaReader(ExtendedBinaryReader reader, Stream indexStream, int blockSize = SaCommon.DefaultBlockSize)
        {
            _reader       = reader;
            _block        = new SaReadBlock(new Zstandard(), blockSize);
            
            _index        = new ChunkedIndex(indexStream);
            Assembly      = _index.Assembly;
            Version       = _index.Version;
            JsonKey       = _index.JsonKey;
            MatchByAllele = _index.MatchByAllele;
            IsArray       = _index.IsArray;
            SchemaVersion = _index.SchemaVersion;
            IsPositional  = _index.IsPositional;

            if (SchemaVersion != SaCommon.SchemaVersion)
                throw new UserErrorException($"ERROR!! SA schema version mismatch. Expected{SaCommon.SchemaVersion}, observed {SchemaVersion} for {JsonKey}");
            
        }

        private ChromosomeInterval _cacheInterval;
        
        public void PreLoad(IChromosome chrom, List<int> positions)
        {
            if (positions == null || positions.Count == 0) return;

            int count = positions.Count;
            int firstPosition = positions[0];
            int lastPosition = positions[count - 1];

            _cacheInterval = new ChromosomeInterval(chrom, firstPosition, lastPosition);

            (long startLocation, long endLocation, int blockCount) = _index.GetFileRange(chrom.Index, firstPosition, lastPosition);
            if (startLocation == -1) return;
            _reader.BaseStream.Position = startLocation;
            var buffer = _reader.ReadBytes((int) (endLocation - startLocation));

            _annotations = new Dictionary<int, List<(string refAllele, string altAllele, string jsonString)>>(positions.Count);
            var posIndex = 0;
            using (var memStream = new MemoryStream(buffer))
            {
                for (var i = 0; i < blockCount; i++)
                {
                    _block.Read(memStream);
                    
                    while (posIndex < positions.Count)
                    {
                        var position = positions[posIndex];

                        if (position < _block.FirstPosition)
                        {
                            posIndex++;
                            continue;
                        }

                        if (_block.LastPosition < position) break;
                        posIndex++;

                        var reader = _block.GetAnnotationReader(position);
                        if (reader !=null) _annotations.TryAdd(position, GetAnnotations(reader));//todo: why TryAdd
                        
                    }
                }
            }
            
        }

        private List<(string, string, string)> GetAnnotations(ExtendedBinaryReader reader)
        {
            if (IsPositional)
            {
                var positionalAnno = reader.ReadString();
                return new List<(string, string, string)>{(null, null, positionalAnno)};
            }

            var count = reader.ReadOptInt32();
            var annotations = new List<(string, string, string)>();
            for (var i = 0; i < count; i++)
            {
                string refAllele = reader.ReadAsciiString();
                string altAllele = reader.ReadAsciiString();
                string annotation = reader.ReadString();
                annotations.Add((refAllele ?? "", altAllele ?? "", annotation));
            }

            return annotations;
        }
        private long _fileOffset = -1;

        public IEnumerable<(string refAllele, string altAllele, string annotation)> GetAnnotation(IChromosome chrom, int position)
        {
            if ( _cacheInterval!=null && _cacheInterval.Overlaps(chrom, position, position))
            {
                if (_annotations == null) return null;
                return _annotations.TryGetValue(position, out var annotations) ? annotations : null;
            }

            //no caching performed, proceed with regular query
            (long startLocation, long _, int _) = _index.GetFileRange(chrom.Index, position, position);
            if (startLocation < 0) return null;

            if (startLocation != _fileOffset) SetFileOffset(startLocation);
            var reader = _block.GetAnnotationReader(position);

            return reader != null ? GetAnnotations(reader) : null;
            
        }

        private void SetFileOffset(long fileOffset)
        {
            _reader.BaseStream.Position = fileOffset;
            _fileOffset = fileOffset;
            _block.Read(_reader.BaseStream);
        }

    }
}