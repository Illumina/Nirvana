using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Compression.Utilities;
using ErrorHandling;
using Genome;
using IO;
using IO.v2;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Providers;

namespace VariantAnnotation.GeneFusions.IO
{
    public sealed class GeneFusionJsonReader : IGeneFusionSaReader
    {
        public const ushort SupportedFileFormatVersion = 1;

        public GenomeAssembly     Assembly { get; }
        public IDataSourceVersion Version  { get; }
        public string             JsonKey  { get; }

        private readonly ExtendedBinaryReader _reader;

        public GeneFusionJsonReader(Stream stream)
        {
            _reader = new ExtendedBinaryReader(stream, Encoding.UTF8);
            // ReSharper disable once UseDeconstruction
            Header header = Header.Read(_reader);
            CheckHeader(header.FileType, header.FileFormatVersion);
            Version = DataSourceVersion.Read(_reader);
        }

        public static void CheckHeader(FileType fileType, ushort fileFormatVersion)
        {
            if (fileType != FileType.FusionCatcher)
                throw new InvalidDataException(
                        $"Found an invalid file type ({fileType}) while reading the gene fusions file.")
                    .MakeUserError();

            if (fileFormatVersion != SupportedFileFormatVersion)
                throw new InvalidDataException(
                        $"The gene fusion reader currently supports v{SupportedFileFormatVersion} files, but found v{fileFormatVersion} instead.")
                    .MakeUserError();
        }

        internal Dictionary<ulong, string[]> GetGeneFusions()
        {
            ArrayPool<byte>    bytePool = ArrayPool<byte>.Shared;
            byte[]             bytes    = _reader.ReadCompressedByteArray(bytePool);
            ReadOnlySpan<byte> byteSpan = bytes.AsSpan();

            int numGeneFusionPairs = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
            var geneKeyToFusion    = new Dictionary<ulong, string[]>(numGeneFusionPairs);

            for (var i = 0; i < numGeneFusionPairs; i++)
            {
                ulong geneKey        = SpanBufferBinaryReader.ReadUInt64(ref byteSpan);
                int   numJsonEntries = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
                var   jsonArray      = new string[numJsonEntries];

                for (var j = 0; j < numJsonEntries; j++) jsonArray[j] = SpanBufferBinaryReader.ReadUtf8String(ref byteSpan);
                geneKeyToFusion[geneKey] = jsonArray;
            }

            bytePool.Return(bytes);
            return geneKeyToFusion;
        }

        public void LoadAnnotations()
        {
            throw new NotImplementedException();
        }

        public void AddAnnotations(IGeneFusionPair[] fusionPairs, IList<ISupplementaryAnnotation> supplementaryAnnotations)
        {
            throw new NotImplementedException();
        }

        public void Dispose() => _reader.Dispose();
    }
}