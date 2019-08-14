using System;
using System.Collections.Generic;
using System.IO;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace VariantAnnotation.NSA
{
    public sealed class NgaReader:IDisposable
    {
        private readonly Stream _nsaStream;
        private readonly MemoryStream _memStream;
        private readonly ExtendedBinaryReader _reader;
        public readonly IDataSourceVersion Version;
        public readonly string JsonKey;
        private readonly bool _isArray;
        private Dictionary<string, List<string>> _geneAnnotations;

        public NgaReader(Stream stream)
        {
            _nsaStream = stream;
            // read the whole file. Currently they are well under 2MB
            var compressedBytes = new byte[2 * 1024 * 1024];
            var decompressedBytes = new byte[20 * 1024 * 1024];
            var compressedSize = _nsaStream.Read(compressedBytes, 0, compressedBytes.Length);

            var zstd = new Zstandard();
            var decompressedSize= zstd.Decompress(compressedBytes, compressedSize, decompressedBytes, decompressedBytes.Length);

            _memStream = new MemoryStream(decompressedBytes, 0, decompressedSize);
            _reader = new ExtendedBinaryReader(_memStream);

            Version= DataSourceVersion.Read(_reader);
            JsonKey = _reader.ReadAsciiString();
            _isArray = _reader.ReadBoolean();
            ushort schemaVersion = _reader.ReadOptUInt16();

            if (schemaVersion != SaCommon.SchemaVersion)
                throw new UserErrorException($"Expected schema version: {SaCommon.SchemaVersion}, observed: {schemaVersion} for {JsonKey}");

        }

        private void PreLoad()
        {
            var geneCount = _reader.ReadOptInt32();
            _geneAnnotations = new Dictionary<string, List<string>>(geneCount);
            for (var i = 0; i < geneCount; i++)
            {
                var geneSymbol = _reader.ReadAsciiString();
                var annoCount = _reader.ReadOptInt32();
                _geneAnnotations.Add(geneSymbol, new List<string>(annoCount));

                for (var j = 0; j < annoCount; j++)
                    _geneAnnotations[geneSymbol].Add(_reader.ReadString());

            }
        }

        public string GetAnnotation(string geneName)
        {
            if (_geneAnnotations==null) PreLoad();

            return _geneAnnotations.TryGetValue(geneName, out List<string> annotations) ? GetJsonString(annotations): null;
        }

        private string GetJsonString(List<string> annotations)
        {
            if (_isArray) return "[" + string.Join(',', annotations) + "]";
            return annotations[0];
        }

        public void Dispose()
        {
            _nsaStream?.Dispose();
            _reader?.Dispose();
            _memStream?.Dispose();
        }
    }
}