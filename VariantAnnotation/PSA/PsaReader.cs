using System;
using System.IO;
using IO;
using VariantAnnotation.SA;

namespace VariantAnnotation.PSA;

public sealed class PsaReader : IDisposable
{
    private readonly ExtendedBinaryReader _reader;
    private readonly ExtendedBinaryReader _indexReader;
    private readonly Stream               _stream;
    private readonly Stream               _indexStream;
    private readonly PsaIndex             _index;

    public readonly SaHeader Header;

    public PsaReader(Stream tsaStream, Stream indexStream)
    {
        _stream      = tsaStream;
        _indexStream = indexStream;
        _reader      = new ExtendedBinaryReader(_stream);
        _indexReader = new ExtendedBinaryReader(_indexStream);

        _index = PsaIndex.Read(_indexReader);
        SaSignature signature = SaSignature.Read(_reader);
        Header = SaHeader.Read(_reader);

        _index.ValidateSignature(signature);
    }

    public (double? score, string prediction) GetScore(ushort chromIndex, string transcriptId, int proteinPos,
        char allele)
    {
        var blockPosition = _index.GetFileLocation(chromIndex, transcriptId);
        if (blockPosition == -1) return (null, null);

        _reader.BaseStream.Position = blockPosition;
        var proteinChangeScores = ProteinChangeScores.Read(_reader);
        var scoreAndPrediction  = proteinChangeScores.GetScoreAndPrediction(proteinPos, allele);
        if (scoreAndPrediction == null) return (null, null);

        (ushort score, string prediction) = scoreAndPrediction.Value;
        return (PsaUtilities.GetDoubleScore(score), prediction);
    }


    public void Dispose()
    {
        _reader?.Dispose();
        _indexReader?.Dispose();
        _stream?.Dispose();
        _indexStream?.Dispose();
    }
}