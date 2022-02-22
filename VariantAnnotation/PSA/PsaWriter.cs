using System;
using System.Collections.Generic;
using System.IO;
using IO;
using VariantAnnotation.SA;

namespace VariantAnnotation.PSA;

public sealed class PsaWriter : IDisposable
{
    private readonly ExtendedBinaryWriter                   _writer;
    private readonly ExtendedBinaryWriter                   _indexWriter;
    private readonly Stream                                 _stream;
    private readonly Stream                                 _indexStream;
    private readonly PsaIndex                               _index;
    private readonly SaHeader                               _header;
    private readonly SaSignature                            _signature;
    private readonly Dictionary<ushort, List<PsaGeneBlock>> _chromGeneBlocks;

    public PsaWriter(Stream stream, Stream indexStream, SaHeader header)
    {
        _header          = header;
        _stream          = stream;
        _indexStream     = indexStream;
        _writer          = new ExtendedBinaryWriter(_stream);
        _indexWriter     = new ExtendedBinaryWriter(_indexStream);
        _signature       = SaSignature.Generate(SaCommon.PsaIdentifier);
        _chromGeneBlocks = new Dictionary<ushort, List<PsaGeneBlock>>();

        _index = new PsaIndex(header, _signature);

        _signature.Write(_writer);
        _header.Write(_writer);
    }

    public int AddGeneBlocks(ushort chromIndex, IEnumerable<PsaGeneBlock> geneBlocks)
    {
        var count = 0;
        if (!_chromGeneBlocks.ContainsKey(chromIndex)) _chromGeneBlocks.Add(chromIndex, new List<PsaGeneBlock>());
        foreach (PsaGeneBlock geneBlock in geneBlocks)
        {
            _index.AddGeneBlock(chromIndex, geneBlock.GeneName, geneBlock.Start, geneBlock.End, _stream.Position);
            geneBlock.Write(_writer);
            count++;
        }

        return count;
    }

    public void Write()
    {
        _index.Write(_indexWriter);
        Flush();
    }

    public void Dispose()
    {
        // write out the index before closing everything
        _index.Write(_indexWriter);

        _writer?.Dispose();
        _indexWriter?.Dispose();
        _stream?.Dispose();
        _indexStream?.Dispose();
    }

    private void Flush()
    {
        _writer.Flush();
        _indexWriter.Flush();
    }
}