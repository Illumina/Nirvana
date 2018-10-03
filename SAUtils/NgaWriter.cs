using System;
using System.Collections.Generic;
using System.IO;
using Compression.Algorithms;
using IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Providers;

namespace SAUtils
{
    public sealed class NgaWriter:IDisposable
    {
        private readonly Stream _nsaStream;
        private readonly DataSourceVersion _version;
        private readonly string _jsonKey;
        private readonly ushort _schemaVersion;
        private readonly bool _isArray;

        public NgaWriter(Stream nsaStream, DataSourceVersion version, string jsonKey, ushort schemaVersion, bool isArray)
        {
            _nsaStream     = nsaStream;
            _version       = version;
            _jsonKey       = jsonKey;
            _schemaVersion = schemaVersion;
            _isArray       = isArray;
        }



        public void Dispose()
        {
            _nsaStream?.Dispose();
        }

        public void Write(Dictionary<string, List<ISuppGeneItem>> geneToEntries)
        {
            using (var memStream = new MemoryStream())
            using (var memWriter = new ExtendedBinaryWriter(memStream))
            using (var writer = new BinaryWriter(_nsaStream))
            {
                _version.Write(memWriter);
                memWriter.WriteOptAscii(_jsonKey);
                memWriter.Write(_isArray);
                memWriter.WriteOpt(_schemaVersion);

                memWriter.WriteOpt(geneToEntries.Count);
                foreach ((string geneSymbol, var entries) in geneToEntries)
                {
                    memWriter.WriteOptAscii(geneSymbol);
                    memWriter.WriteOpt(entries.Count);
                    foreach (ISuppGeneItem geneItem in entries)
                    {
                        memWriter.Write(geneItem.GetJsonString());
                    }
                }

                var uncompressedBytes = memStream.ToArray();
                var compressedBytes = new byte[uncompressedBytes.Length + 32];

                var compressor = new Zstandard();
                var compressSize = compressor.Compress(uncompressedBytes, uncompressedBytes.Length, compressedBytes,
                    compressedBytes.Length);

                writer.Write(compressedBytes, 0, compressSize);
                Console.WriteLine("Number of gene entries written:"+ geneToEntries.Count);

            }

        }

        
    }
}