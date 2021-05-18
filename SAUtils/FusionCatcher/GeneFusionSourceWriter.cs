using System;
using System.IO;
using System.Text;
using Compression.Utilities;
using IO;
using IO.v2;
using VariantAnnotation.GeneFusions.IO;
using VariantAnnotation.GeneFusions.SA;
using VariantAnnotation.Interface.Providers;

namespace SAUtils.FusionCatcher
{
    public sealed class GeneFusionSourceWriter : IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;

        public GeneFusionSourceWriter(Stream stream, string jsonKey, IDataSourceVersion version, bool leaveOpen = false)
        {
            _writer = new ExtendedBinaryWriter(stream, Encoding.UTF8, leaveOpen);
            WriteHeader();
            _writer.Write(jsonKey);
            version.Write(_writer);
        }

        private void WriteHeader()
        {
            var header = new Header(FileType.FusionCatcher, GeneFusionSourceReader.SupportedFileFormatVersion);
            header.Write(_writer);
        }

        public void Write(GeneFusionSourceCollection[] index, GeneFusionIndexEntry[] indexEntries)
        {
            using var ms = new MemoryStream();
            using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
            {
                writer.WriteOpt(index.Length);
                foreach (GeneFusionSourceCollection sourceCollection in index) sourceCollection.Write(writer);

                writer.WriteOpt(indexEntries.Length);
                foreach (GeneFusionIndexEntry indexEntry in indexEntries) indexEntry.Write(writer);
            }

            byte[] bytes = ms.ToArray();
            _writer.WriteCompressedByteArray(bytes, bytes.Length);
        }

        public void Dispose() => _writer.Dispose();
    }
}