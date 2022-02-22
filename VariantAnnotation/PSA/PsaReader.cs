using System;
using System.Collections.Generic;
using System.IO;
using Compression.Algorithms;
using IO;
using VariantAnnotation.SA;

namespace VariantAnnotation.PSA;

public sealed class PsaReader : IDisposable
{
    private readonly ExtendedBinaryReader  _reader;
    private readonly ExtendedBinaryReader  _indexReader;
    private readonly Stream                _stream;
    private readonly Stream                _indexStream;
    private readonly ICompressionAlgorithm _compressionAlgorithm = new Zstandard();
    private readonly PsaIndex              _index;

    private readonly Dictionary<long, PsaGeneBlock> _blocksCache;

    public readonly SaHeader Header;

    public PsaReader(Stream tsaStream, Stream indexStream)
    {
        _stream      = tsaStream;
        _indexStream = indexStream;
        _reader      = new ExtendedBinaryReader(_stream);
        _indexReader = new ExtendedBinaryReader(_indexStream);
        _blocksCache = new Dictionary<long, PsaGeneBlock>();

        _index = PsaIndex.Read(_indexReader);
        SaSignature signature = SaSignature.Read(_reader);
        Header = SaHeader.Read(_reader);

        _index.ValidateSignature(signature);
    }

    public double? GetScore(ushort chromIndex, string geneName, string transcriptId, int proteinPos, char allele)
    {
        // todo: remove blocks that are no longer required
        // this is very basic. need a LRU cache like structure
        if (_blocksCache.Count > 15) _blocksCache.Clear();

        long blockPosition = _index.GetGeneBlockPosition(chromIndex, geneName);
        if (blockPosition == -1) return null;

        PsaGeneBlock geneBlock;
        //block positions in the file can uniquely identify a block.
        if (!_blocksCache.ContainsKey(blockPosition))
        {
            _stream.Position = blockPosition;
            geneBlock        = PsaGeneBlock.Read(_reader, _compressionAlgorithm);
            _blocksCache.Add(blockPosition, geneBlock);
        }
        else geneBlock = _blocksCache[blockPosition];

        var proteinChangeScores = geneBlock.GetTranscriptScores(transcriptId);
        if (proteinChangeScores == null || proteinPos > proteinChangeScores.ProteinLength) return null;
        short score = proteinChangeScores.GetScore(proteinPos, allele);

        return PsaUtilities.GetDoubleScore(score);
    }

    public void Dispose()
    {
        _reader?.Dispose();
        _indexReader?.Dispose();
        _stream?.Dispose();
        _indexStream?.Dispose();
    }
}