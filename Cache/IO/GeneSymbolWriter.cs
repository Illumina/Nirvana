using System;
using System.IO;
using System.Linq;
using System.Text;
using Cache.Data;
using Compression.Utilities;
using IO;

namespace Cache.IO;

public sealed class GeneSymbolWriter : IDisposable
{
    private readonly ExtendedBinaryWriter _writer;

    public GeneSymbolWriter(Stream stream, bool leaveOpen = false)
    {
        _writer = new ExtendedBinaryWriter(stream, Encoding.UTF8, leaveOpen);
        WriteHeader();
    }

    private void WriteHeader()
    {
        var header = new Header(FileType.GeneSymbol, GeneSymbolReader.SupportedFileFormatVersion);
        header.Write(_writer);
    }

    public (int CompressedSize, double PercentCompression) Write(HgncGeneSymbol[] geneSymbols)
    {
        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            writer.WriteOpt(geneSymbols.Length);
            var prevHgncId = 0;

            foreach ((int hgncId, string geneSymbol) in geneSymbols.OrderBy(x => x.HgncId))
            {
                int deltaHgncId = hgncId - prevHgncId;
                writer.WriteOpt(deltaHgncId);
                writer.Write((byte) geneSymbol.Length);
                writer.Write(Encoding.ASCII.GetBytes(geneSymbol));
                prevHgncId = hgncId;
            }
        }

        byte[] bytes = ms.ToArray();
        return _writer.WriteCompressedByteArray(bytes, bytes.Length);
    }

    public void Dispose() => _writer.Dispose();
}