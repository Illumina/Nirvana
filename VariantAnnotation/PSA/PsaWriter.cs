using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.SA;

namespace VariantAnnotation.PSA;

public sealed class PsaWriter : IDisposable
{
    private readonly ExtendedBinaryWriter           _writer;
    private readonly ExtendedBinaryWriter           _indexWriter;
    private readonly Stream                         _stream;
    private readonly Stream                         _indexStream;
    private readonly PsaIndex                       _index;
    private readonly SaHeader                       _header;
    private readonly SaSignature                    _signature;
    private readonly Dictionary<string, Chromosome> _refNameToChromosome;

    public PsaWriter(Stream stream, Stream indexStream, SaHeader header,
        Dictionary<string, Chromosome> refNameToChromosome)
    {
        _header              = header;
        _stream              = stream;
        _indexStream         = indexStream;
        _writer              = new ExtendedBinaryWriter(_stream);
        _indexWriter         = new ExtendedBinaryWriter(_indexStream);
        _refNameToChromosome = refNameToChromosome;
        _signature           = SaSignature.Generate(SaCommon.PsaIdentifier);

        _index = new PsaIndex(header, _signature);

        _signature.Write(_writer);
        _header.Write(_writer);
    }


    public void Write(IEnumerable<PsaDataItem> items)
    {
        var                 transcriptCount  = 0;
        ProteinChangeScores transcriptScores = null;
        var                 chrIndex         = ushort.MaxValue;
        foreach (PsaDataItem item in items)
        {
            if (transcriptScores == null)
            {
                chrIndex         = _refNameToChromosome[item.ChromName].Index;
                transcriptScores = new ProteinChangeScores(item.TranscriptId);
            }

            if (transcriptScores.TranscriptId != item.TranscriptId)
            {
                transcriptCount++;
                // write the old object
                _index.Add(chrIndex, transcriptScores.TranscriptId, _writer.BaseStream.Position);
                transcriptScores.Write(_writer);
                //start new scores object
                transcriptScores = new ProteinChangeScores(item.TranscriptId);
            }

            transcriptScores.AddScore(item);
            chrIndex = _refNameToChromosome[item.ChromName].Index;
        }

        // write the last object
        if (transcriptScores != null)
        {
            _index.Add(chrIndex, transcriptScores.TranscriptId, _writer.BaseStream.Position);
            transcriptScores.Write(_writer);
        }

        Console.WriteLine($"found {_header.JsonKey} scores for {transcriptCount} transcripts.");
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