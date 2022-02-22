using System;
using System.Buffers;
using System.IO;
using System.Text;
using Cache.Data;
using Compression.Utilities;
using ErrorHandling;
using IO;

namespace Cache.IO;

public sealed class GeneSymbolReader : IDisposable
{
    private readonly ExtendedBinaryReader _reader;

    public const ushort SupportedFileFormatVersion = 1;

    public GeneSymbolReader(Stream stream)
    {
        _reader = new ExtendedBinaryReader(stream, Encoding.UTF8);
        Header header = Header.Read(_reader);
        CheckHeader(header.FileType, header.FileFormatVersion);
    }

    public static void CheckHeader(FileType fileType, ushort fileFormatVersion)
    {
        if (fileType != FileType.GeneSymbol)
            throw new InvalidDataException(
                    $"Found an invalid file type ({fileType}) while reading the gene symbols file.")
                .MakeUserError();

        if (fileFormatVersion != SupportedFileFormatVersion)
            throw new InvalidDataException(
                    $"The gene symbol reader currently supports v{SupportedFileFormatVersion} files, but found v{fileFormatVersion} instead.")
                .MakeUserError();
    }

    public HgncGeneSymbol[] GetHgncGeneSymbols()
    {
        ArrayPool<byte>    bytePool = ArrayPool<byte>.Shared;
        byte[]             bytes    = _reader.ReadCompressedByteArray(bytePool);
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();

        int numGeneSymbols = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        var geneSymbols    = new HgncGeneSymbol[numGeneSymbols];

        var prevHgncId = 0;
        for (var i = 0; i < numGeneSymbols; i++)
        {
            int deltaHgncId = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
            int hgncId      = prevHgncId + deltaHgncId;
            prevHgncId = hgncId;

            int                len            = SpanBufferBinaryReader.ReadByte(ref byteSpan);
            ReadOnlySpan<byte> geneSymbolSpan = SpanBufferBinaryReader.ReadBytes(ref byteSpan, len);
            string             geneSymbol     = Encoding.ASCII.GetString(geneSymbolSpan);
            geneSymbols[i] = new HgncGeneSymbol(hgncId, geneSymbol);
        }

        bytePool.Return(bytes);
        return geneSymbols;
    }

    public void Dispose() => _reader.Dispose();
}