using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.Algorithms;
using Compression.FileHandling;
using IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.SA;

namespace SAUtils
{
    public sealed class NgaWriter : IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;

        public NgaWriter(Stream stream, ISerializable version, string jsonKey, ushort schemaVersion, bool isArray,
            bool leaveOpen = false)
        {
            WriteHeader(stream, version, jsonKey, schemaVersion, isArray);

            var blockStream = new BlockStream(new Zstandard(), stream, CompressionMode.Compress);
            _writer         = new ExtendedBinaryWriter(blockStream, Encoding.UTF8, leaveOpen);
        }

        private static void WriteHeader(Stream stream, ISerializable version, string jsonKey, ushort schemaVersion, bool isArray)
        {
            using (var writer = new ExtendedBinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(SaCommon.NgaIdentifier);
                version.Write(writer);
                writer.Write(jsonKey);
                writer.Write(isArray);
                writer.Write(schemaVersion);
                writer.Write(SaCommon.GuardInt);
            }
        }

        public void Dispose() => _writer.Dispose();

        public int Write(Dictionary<string, List<ISuppGeneItem>> geneToEntries)
        {
            _writer.WriteOpt(geneToEntries.Count);

            var count = 0;
            foreach ((string geneSymbol, List<ISuppGeneItem> entries) in geneToEntries)
            {
                _writer.WriteOptAscii(geneSymbol);
                _writer.WriteOpt(entries.Count);

                foreach (ISuppGeneItem geneItem in entries)
                {
                    count++;
                    _writer.Write(geneItem.GetJsonString());
                }
            }

            return count;
        }
    }
}