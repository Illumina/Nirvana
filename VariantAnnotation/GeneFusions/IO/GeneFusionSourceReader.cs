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
using VariantAnnotation.GeneFusions.SA;
using VariantAnnotation.GeneFusions.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;

namespace VariantAnnotation.GeneFusions.IO
{
    public sealed class GeneFusionSourceReader : IGeneFusionSaReader
    {
        public const ushort SupportedFileFormatVersion = 1;

        private readonly ExtendedBinaryReader _reader;

        public GenomeAssembly     Assembly => GenomeAssembly.Unknown;
        public IDataSourceVersion Version  { get; }
        public string             JsonKey  { get; }

        internal GeneFusionSourceCollection[] Index;
        internal GeneFusionIndexEntry[]       IndexEntries;

        public GeneFusionSourceReader(Stream stream)
        {
            _reader = new ExtendedBinaryReader(stream, Encoding.UTF8);
            // ReSharper disable once UseDeconstruction
            Header header = Header.Read(_reader);
            JsonKey = _reader.ReadString();
            CheckHeader(header.FileType, header.FileFormatVersion);
            Version = DataSourceVersion.Read(_reader);
        }

        internal static void CheckHeader(FileType fileType, ushort fileFormatVersion)
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

        public void LoadAnnotations()
        {
            ArrayPool<byte>    bytePool = ArrayPool<byte>.Shared;
            byte[]             bytes    = _reader.ReadCompressedByteArray(bytePool);
            ReadOnlySpan<byte> byteSpan = bytes.AsSpan();

            int indexLength = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
            Index = new GeneFusionSourceCollection[indexLength];
            for (var i = 0; i < indexLength; i++) Index[i] = GeneFusionSourceCollection.Read(ref byteSpan);

            int numIndexEntries = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
            IndexEntries = new GeneFusionIndexEntry[numIndexEntries];
            for (var i = 0; i < numIndexEntries; i++) IndexEntries[i] = GeneFusionIndexEntry.Read(ref byteSpan);

            bytePool.Return(bytes);
        }

        public void AddAnnotations(IGeneFusionPair[] fusionPairs, IList<ISupplementaryAnnotation> supplementaryAnnotations)
        {
            var jsonEntries = new List<string>();

            foreach (IGeneFusionPair fusionPair in fusionPairs)
            {
                ushort? index = IndexEntries.GetIndex(fusionPair.GeneKey);
                if (index == null) continue;
                jsonEntries.Add(Index[index.Value].GetJsonEntry(fusionPair.GeneSymbols));
            }

            if (jsonEntries.Count == 0) return;

            var sa = new SupplementaryAnnotation(JsonKey, true, false, null, jsonEntries);
            supplementaryAnnotations.Add(sa);
        }

        public void Dispose() => _reader.Dispose();
    }
}