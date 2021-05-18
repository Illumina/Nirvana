using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Compression.Utilities;
using IO;
using IO.v2;
using VariantAnnotation.GeneFusions.IO;
using VariantAnnotation.Interface.Providers;

namespace SAUtils.CosmicGeneFusions
{
    public sealed class GeneFusionJsonWriter : IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;

        public GeneFusionJsonWriter(Stream stream, IDataSourceVersion version, bool leaveOpen = false)
        {
            _writer = new ExtendedBinaryWriter(stream, Encoding.UTF8, leaveOpen);
            WriteHeader();
            version.Write(_writer);
        }

        private void WriteHeader()
        {
            var header = new Header(FileType.FusionCatcher, GeneFusionSourceReader.SupportedFileFormatVersion);
            header.Write(_writer);
        }

        public void Write(Dictionary<ulong, string[]> geneKeyToJson)
        {
            using var ms = new MemoryStream();
            using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
            {
                writer.WriteOpt(geneKeyToJson.Count);

                foreach ((ulong geneKey, string[] jsonArray) in geneKeyToJson)
                {
                    writer.Write(geneKey);
                    writer.WriteOpt(jsonArray.Length);
                    foreach (string json in jsonArray) writer.Write(json);
                }
            }

            byte[] bytes = ms.ToArray();
            _writer.WriteCompressedByteArray(bytes, bytes.Length);
        }

        public void Dispose() => _writer.Dispose();
    }
}