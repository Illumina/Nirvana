using System;
using System.Collections.Generic;
using IO;

namespace VariantAnnotation.PSA;

public sealed class ProteinChangeScores
{
    public readonly string         TranscriptId;
    public readonly List<ushort[]> Scores;
    public readonly List<byte[]>   PredictionBytes;
    public          int            ProteinLength; // needed for early termination of GetScore while reading

    public static int    NumAminoAcids => AllAminoAcids.Length;
    public const  string AllAminoAcids = "ABCDEFGHIJKLMNOPQRSTUVWXYZ*";
    public const  ushort NullScore     = 1001;

    public static readonly Dictionary<string, byte> PredictionToByte = new()
    {
        {"benign", 0},
        {"possibly damaging", 1},
        {"probably damaging", 2},
        {"deleterious", 3},
        {"deleterious - low confidence", 4},
        {"tolerated", 5},
        {"tolerated - low confidence", 6}
    };

    public static readonly string[] BytesToPredictions =
    {
        "benign", "possibly damaging", "probably damaging", "deleterious",
        "deleterious - low confidence", "tolerated", "tolerated - low confidence"
    };

    public ProteinChangeScores(string transcriptId)
    {
        TranscriptId    = transcriptId;
        Scores          = new List<ushort[]>();
        PredictionBytes = new List<byte[]>();
    }

    // this private constructor is only used by the read method. No Peptide sequence available.
    private ProteinChangeScores(string transcriptId, List<ushort[]> scores, List<byte[]> predictionBytes)
    {
        TranscriptId    = transcriptId;
        Scores          = scores;
        PredictionBytes = predictionBytes;
        ProteinLength   = scores.Count;
    }

    private static ushort[] GetFilledArray(int length, ushort fillValue)
    {
        var array = new ushort[length];
        Array.Fill(array, fillValue);

        return array;
    }

    public bool AddScore(PsaDataItem item)
    {
        if (item.Position < 1) return false;

        var index = GetIndex(item.AltAllele);
        if (index < 0) return false;

        while (Scores.Count < item.Position)
        {
            Scores.Add(GetFilledArray(NumAminoAcids, NullScore));
            PredictionBytes.Add(new byte[NumAminoAcids]);
        }

        var i = item.Position - 1;
        Scores[i][index] = item.Score;
        var predictionByte = PredictionToByte[item.Prediction];
        PredictionBytes[i][index] = predictionByte;
        return true;
    }

    public (ushort, string)? GetScoreAndPrediction(int position, char altAllele)
    {
        if (position < 1 || position > ProteinLength) return null;

        var alleleIndex = GetIndex(altAllele);
        var score       = Scores[position - 1][alleleIndex];
        if (score == NullScore) return null;
        var predictionByte = PredictionBytes[position - 1][alleleIndex];
        return (score, BytesToPredictions[predictionByte]);
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        writer.WriteOptAscii(TranscriptId);
        ProteinLength = Scores.Count;
        writer.WriteOpt(ProteinLength);

        for (var i = 0; i < ProteinLength; i++)
        {
            for (int j = 0; j < NumAminoAcids; j++)
            {
                writer.WriteOpt(Scores[i][j]);
            }
        }

        //write predictions
        for (var i = 0; i < ProteinLength; i++)
        {
            for (int j = 0; j < NumAminoAcids; j++)
            {
                writer.Write(PredictionBytes[i][j]);
            }
        }
    }

    private int GetIndex(char allele)
    {
        // standardizing to upper case letters for amino acids
        if (char.IsLower(allele)) allele = Char.ToUpper(allele);

        if (allele == '*') return NumAminoAcids - 1;
        return allele - 'A';
    }

    public static ProteinChangeScores Read(ExtendedBinaryReader reader)
    {
        var transcriptId = reader.ReadAsciiString();

        var length = reader.ReadOptInt32();

        var scores = new List<ushort[]>(length);
        for (var i = 0; i < length; i++)
        {
            scores.Add(GetFilledArray(NumAminoAcids, NullScore));
            for (int j = 0; j < NumAminoAcids; j++)
            {
                scores[i][j] = reader.ReadOptUInt16();
            }
        }

        var predictionBytes = new List<byte[]>(length);
        for (var i = 0; i < length; i++)
        {
            predictionBytes.Add(new byte[NumAminoAcids]);
            for (int j = 0; j < NumAminoAcids; j++)
            {
                predictionBytes[i][j] = reader.ReadByte();
            }
        }

        return new ProteinChangeScores(transcriptId, scores, predictionBytes);
    }
}