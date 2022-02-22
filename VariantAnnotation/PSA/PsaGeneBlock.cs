using System;
using System.Collections.Generic;
using System.IO;
using Compression.Algorithms;
using IO;
using OptimizedCore;
using VariantAnnotation.SA;

namespace VariantAnnotation.PSA;

public sealed class PsaGeneBlock : IDisposable
{
    public readonly  string                                  GeneName;
    public readonly  int                                     Start;
    public readonly  int                                     End;
    private readonly Dictionary<string, ProteinChangeScores> _proteinScoresMap; // used for building the blocks

    private readonly Dictionary<string, ProteinChangeScores>
        _transcriptScoresMap; // used to query by transcript. Populated only for reading

    private readonly ICompressionAlgorithm _compressionAlgorithm;
    private readonly ExtendedBinaryWriter  _writer;

    private readonly MemoryStream _writeStream;

    public PsaGeneBlock(string geneName, int start, int end)
    {
        GeneName = geneName;
        Start    = start;
        End      = end;

        _writeStream = new MemoryStream();
        _writer      = new ExtendedBinaryWriter(_writeStream);

        _compressionAlgorithm = new Zstandard();
        _proteinScoresMap     = new Dictionary<string, ProteinChangeScores>();
        _transcriptScoresMap  = new Dictionary<string, ProteinChangeScores>();
    }

    private PsaGeneBlock(string geneName, int start, int end,
        Dictionary<string, ProteinChangeScores> transcriptScoresMap)
    {
        GeneName = geneName;
        Start    = start;
        End      = end;

        _transcriptScoresMap = transcriptScoresMap;
    }

    public static PsaGeneBlock Read(ExtendedBinaryReader reader, ICompressionAlgorithm compressionAlgorithm)
    {
        int    uncompressedLength = reader.ReadOptInt32();
        byte[] uncompressedBlock  = ExpandableArray<byte>.Rent(uncompressedLength);

        int    compressedLength = reader.ReadOptInt32();
        byte[] compressedBlock  = ExpandableArray<byte>.Rent(compressedLength);
        reader.Read(compressedBlock, 0, compressedLength);


        int observedUncompressedLength = compressionAlgorithm.Decompress(compressedBlock, compressedLength,
            uncompressedBlock, uncompressedLength);

        if (observedUncompressedLength != uncompressedLength)
            throw new DataMisalignedException(
                $"Expected uncompressed block length:{uncompressedLength}, observed length: {observedUncompressedLength} ");


        var blockReader = new ExtendedBinaryReader(new MemoryStream(uncompressedBlock, 0, uncompressedLength));
        PsaUtilities.CheckGuardInt(blockReader, "start of PsaGeneBlock");

        string geneName          = blockReader.ReadAsciiString();
        int    start             = blockReader.ReadOptInt32();
        int    end               = blockReader.ReadOptInt32();
        int    proteinScoreCount = blockReader.ReadOptInt32();

        var transcriptScoresMap = new Dictionary<string, ProteinChangeScores>(proteinScoreCount);
        for (var i = 0; i < proteinScoreCount; i++)
        {
            var changeScore = ProteinChangeScores.Read(blockReader);
            foreach (string id in changeScore.TranscriptIds)
            {
                transcriptScoresMap.TryAdd(id, changeScore);
            }
        }


        ExpandableArray<byte>.Return(uncompressedBlock);
        ExpandableArray<byte>.Return(compressedBlock);

        return new PsaGeneBlock(geneName, start, end, transcriptScoresMap);
    }

    public IEnumerable<string> GetTranscriptIds()
    {
        if (_transcriptScoresMap != null) return _transcriptScoresMap.Keys;
        var transcriptIds = new List<string>();
        foreach (var changeScore in _proteinScoresMap.Values)
        {
            transcriptIds.AddRange(changeScore.TranscriptIds);
        }

        return transcriptIds;
    }

    public int Write(ExtendedBinaryWriter writer)
    {
        byte[] uncompressedBytes;
        int    count = _proteinScoresMap.Count;
        using (var blockStream = new MemoryStream())
        using (var blockWriter = new ExtendedBinaryWriter(blockStream))
        {
            blockWriter.Write(SaCommon.GuardInt);
            blockWriter.WriteOptAscii(GeneName);
            blockWriter.WriteOpt(Start);
            blockWriter.WriteOpt(End);
            blockWriter.WriteOpt(_proteinScoresMap.Count);

            foreach (var changeScores in _proteinScoresMap.Values)
            {
                changeScores.Write(blockWriter);
            }

            blockWriter.Flush();
            uncompressedBytes = blockStream.GetBuffer();
        }

        int    compressedBlockLength = _compressionAlgorithm.GetCompressedBufferBounds(uncompressedBytes.Length);
        byte[] compressedBlock       = ExpandableArray<byte>.Rent(compressedBlockLength);
        int compressedLength = _compressionAlgorithm.Compress(uncompressedBytes, uncompressedBytes.Length,
            compressedBlock, compressedBlock.Length);

        writer.WriteOpt(uncompressedBytes.Length);
        writer.WriteOpt(compressedLength);
        writer.Write(compressedBlock, 0, compressedLength);

        ExpandableArray<byte>.Return(compressedBlock);
        return count;
    }


    public bool TryAddProteinScores(string geneName, ProteinChangeScores scores)
    {
        if (geneName != GeneName) return false;
        foreach (string transcriptId in scores.TranscriptIds)
        {
            _transcriptScoresMap.TryAdd(transcriptId, scores);
        }

        return _proteinScoresMap.TryAdd(scores.PeptideSequence, scores);
    }

    public ProteinChangeScores GetTranscriptScores(string transcriptId)
    {
        return _transcriptScoresMap.TryGetValue(transcriptId, out var scores) ? scores : null;
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _writeStream?.Dispose();
    }
}