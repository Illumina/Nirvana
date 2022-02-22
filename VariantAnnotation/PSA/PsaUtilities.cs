using System;

namespace VariantAnnotation.PSA;

public static class PsaUtilities
{
    private const int MaxIntScore = 1000;

    public static ushort GetUshortScore(double score) =>
        (ushort) Math.Round(score * MaxIntScore, MidpointRounding.AwayFromZero);

    public static double GetDoubleScore(ushort score) => 1.0 * score / MaxIntScore;
}