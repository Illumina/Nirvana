using System;
using System.Collections.Generic;
using IO;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.PSA;

public sealed class ProteinChangeScores
{
    public readonly IList<string> TranscriptIds;
    public readonly short[,]      Scores;
    public readonly int           ProteinLength;
    public readonly string        PeptideSequence;
    public          bool          HasInvalidRef { get; private set; }

    public const int    NumAminoAcids = 27;
    public const string AllAminoAcids = "ABCDEFGHIJKLMNOPQRSTUVWXYZ*";

    public ProteinChangeScores(IList<string> transcriptIds, string peptideSequence)
    {
        TranscriptIds   = transcriptIds;
        PeptideSequence = peptideSequence;
        ProteinLength   = peptideSequence.Length;

        Scores = new short [ProteinLength, NumAminoAcids];
    }

    // this private constructor is only used by the read method. No Peptide sequence available.
    private ProteinChangeScores(IList<string> transcriptIds, short[,] scores)
    {
        TranscriptIds = transcriptIds;
        ProteinLength = scores.GetLength(0);
        Scores        = scores;
    }

    public bool AddScore(IProteinSuppDataItem item)
    {
        if (item.Position < 1 || item.Position > ProteinLength) return false;
        if (PeptideSequence[item.Position - 1] != item.RefAllele)
        {
            HasInvalidRef = true;
            return false;
        }

        int index = GetIndex(item.AltAllele);
        if (index < 0) return false;
        Scores[item.Position - 1, index] = item.Score;
        return true;
    }

    public short GetScore(int position, char altAllele)
    {
        if (position < 1 || position > ProteinLength)
            throw new IndexOutOfRangeException("Protein position out of score matrix range");
        int alleleIndex = GetIndex(altAllele);
        return Scores[position - 1, alleleIndex];
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        writer.WriteOpt(TranscriptIds.Count);
        foreach (string transcriptId in TranscriptIds)
        {
            writer.WriteOptAscii(transcriptId);
        }

        writer.WriteOpt(ProteinLength);
        for (var i = 0; i < ProteinLength; i++)
        {
            for (var j = 0; j < NumAminoAcids; j++)
            {
                writer.Write(Scores[i, j]);
            }
        }
    }

    private static int GetIndex(char allele)
    {
        // standardizing to upper case letters for amino acids
        if (char.IsLower(allele)) allele = char.ToUpper(allele);

        if (allele == '*') return NumAminoAcids - 1;
        return allele - 'A';
    }

    public static ProteinChangeScores Read(ExtendedBinaryReader reader)
    {
        int count         = reader.ReadOptInt32();
        var transcriptIds = new string [count];
        for (var i = 0; i < count; i++)
        {
            transcriptIds[i] = reader.ReadAsciiString();
        }

        int length = reader.ReadOptInt32();

        var scores = new short[length, NumAminoAcids];
        for (var i = 0; i < length; i++)
        {
            for (var j = 0; j < NumAminoAcids; j++)
            {
                scores[i, j] = reader.ReadInt16();
            }
        }

        return new ProteinChangeScores(transcriptIds, scores);
    }
}