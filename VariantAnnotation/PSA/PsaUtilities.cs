using System;
using IO;
using VariantAnnotation.SA;

namespace VariantAnnotation.PSA;

public static class PsaUtilities
{
    // public static string GetGeneId(Gene gene)
    // {
    //     string geneId = (gene.EnsemblId?.WithoutVersion ?? gene.EntrezGeneId?.WithoutVersion) ??
    //         gene.Symbol;
    //     return geneId;
    // }

    public static void CheckGuardInt(ExtendedBinaryReader reader, string fileSection)
    {
        uint guardInt = reader.ReadUInt32();
        if (guardInt != SaCommon.GuardInt)
            throw new DataMisalignedException(
                $"Failed to find GuardInt ({SaCommon.GuardInt}) at the end of {fileSection}.");
    }

    public static short GetShortScore(double score)
    {
        return (short) Math.Round(score * 1000, MidpointRounding.AwayFromZero);
    }

    public static double GetDoubleScore(short score)
    {
        return 1.0 * score / 1000;
    }


    public static string GetPrediction(string jsonKey, double score)
    {
        switch (jsonKey)
        {
            case SaCommon.SiftTag:
                return score < 0.05 ? "deleterious" : "tolerated";
            case SaCommon.PolyPhenTag:
                if (score > 0.908) return "probably damaging";
                return 0.446 < score ? "possibly damaging" : "benign";
        }

        return "unknown";
    }
}