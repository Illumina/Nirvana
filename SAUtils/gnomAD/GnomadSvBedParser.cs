using System.Collections.Generic;
using System.IO;
using Genome;
using SAUtils.DataStructures;
using SAUtils.ParseUtils;
using Variants;

namespace SAUtils.gnomAD;

public sealed class GnomadSvBedParser : GnomadSvParser
{
    public GnomadSvBedParser(
        StreamReader reader,
        Dictionary<string, Chromosome> refNameDict
    ) : base(reader, refNameDict)
    {
        TsvIndices = new TsvIndices
        {
            Chromosome = 0,
            Start      = 1,
            End        = 2,
            VariantId  = 3,
            SvType     = 4,
            Filters    = 241,

            AllAlleleNumber    = 35,
            AllAlleleCount     = 36,
            AllAlleleFrequency = 37,
            AllHomCount        = 41,

            MaleAlleleNumber    = 45,
            MaleAlleleCount     = 46,
            MaleAlleleFrequency = 47,
            MaleHomCount        = 51,

            FemaleAlleleNumber    = 60,
            FemaleAlleleCount     = 61,
            FemaleAlleleFrequency = 62,
            FemaleHomCount        = 66,

            AfrAlleleNumber    = 71,
            AfrAlleleCount     = 72,
            AfrAlleleFrequency = 73,
            AfrHomCount        = 77,

            AmrAlleleNumber    = 105,
            AmrAlleleCount     = 106,
            AmrAlleleFrequency = 107,
            AmrHomCount        = 111,

            EasAlleleNumber    = 139,
            EasAlleleCount     = 140,
            EasAlleleFrequency = 141,
            EasHomCount        = 145,

            EurAlleleNumber    = 173,
            EurAlleleCount     = 174,
            EurAlleleFrequency = 175,
            EurHomCount        = 179,

            OthAlleleNumber    = 207,
            OthAlleleCount     = 208,
            OthAlleleFrequency = 209,
            OthHomCount        = 211,
        };
    }

    protected override GnomadSvItem ParseLine(string inputLine)
    {
        var    splitLine      = new SplitLine(in inputLine, in Delimiter);
        string chromosomeName = splitLine.GetString(TsvIndices.Chromosome);
        if (!RefNameDict.ContainsKey(chromosomeName))
            return null;

        Chromosome chromosome = RefNameDict[chromosomeName];
        int?       start      = splitLine.ParseInteger(TsvIndices.Start);
        int?       end        = splitLine.ParseInteger(TsvIndices.End);
        if (start == null || end == null)
            throw new InvalidDataException($"Invalid Data on Line {inputLine}");
        
        VariantType svType = SvTypeMapper(splitLine.GetString(TsvIndices.SvType));

        // Ignoring BND for now
        if (svType == VariantType.translocation_breakend)
            return null;
        
        // For some reason the in the source file, the end position is +1 for insertions
        if (svType == VariantType.insertion)
            end--;

        start += 2; // +1 start is 0-based in BED format, also +1 for padding base
        if (start > end)
            (start, end) = (end, start);
        
        string filters          = splitLine.GetString(TsvIndices.Filters);
        bool   hasFailedFilters = SaUtilsCommon.HasFailedFilters(filters);
        return new GnomadSvItem(chromosome, inputLine)
        {
            Start     = (int) start,
            End       = (int) end,
            VariantId = splitLine.GetString(TsvIndices.VariantId),
            SvType    = SvTypeMapper(splitLine.GetString(TsvIndices.SvType)),

            AllAlleleNumber    = splitLine.ParseInteger(TsvIndices.AllAlleleNumber),
            AllAlleleCount     = splitLine.ParseInteger(TsvIndices.AllAlleleCount),
            AllAlleleFrequency = splitLine.ParseDouble(TsvIndices.AllAlleleFrequency),
            AllHomCount        = splitLine.ParseInteger(TsvIndices.AllHomCount),

            MaleAlleleNumber    = splitLine.ParseInteger(TsvIndices.MaleAlleleNumber),
            MaleAlleleCount     = splitLine.ParseInteger(TsvIndices.MaleAlleleCount),
            MaleAlleleFrequency = splitLine.ParseDouble(TsvIndices.MaleAlleleFrequency),
            MaleHomCount        = splitLine.ParseInteger(TsvIndices.MaleHomCount),

            FemaleAlleleNumber    = splitLine.ParseInteger(TsvIndices.FemaleAlleleNumber),
            FemaleAlleleCount     = splitLine.ParseInteger(TsvIndices.FemaleAlleleCount),
            FemaleAlleleFrequency = splitLine.ParseDouble(TsvIndices.FemaleAlleleFrequency),
            FemaleHomCount        = splitLine.ParseInteger(TsvIndices.FemaleHomCount),

            AfrAlleleNumber    = splitLine.ParseInteger(TsvIndices.AfrAlleleNumber),
            AfrAlleleCount     = splitLine.ParseInteger(TsvIndices.AfrAlleleCount),
            AfrAlleleFrequency = splitLine.ParseDouble(TsvIndices.AfrAlleleFrequency),
            AfrHomCount        = splitLine.ParseInteger(TsvIndices.AfrHomCount),

            AmrAlleleNumber    = splitLine.ParseInteger(TsvIndices.AmrAlleleNumber),
            AmrAlleleCount     = splitLine.ParseInteger(TsvIndices.AmrAlleleCount),
            AmrAlleleFrequency = splitLine.ParseDouble(TsvIndices.AmrAlleleFrequency),
            AmrHomCount        = splitLine.ParseInteger(TsvIndices.AmrHomCount),

            EasAlleleNumber    = splitLine.ParseInteger(TsvIndices.EasAlleleNumber),
            EasAlleleCount     = splitLine.ParseInteger(TsvIndices.EasAlleleCount),
            EasAlleleFrequency = splitLine.ParseDouble(TsvIndices.EasAlleleFrequency),
            EasHomCount        = splitLine.ParseInteger(TsvIndices.EasHomCount),

            EurAlleleNumber    = splitLine.ParseInteger(TsvIndices.EurAlleleNumber),
            EurAlleleCount     = splitLine.ParseInteger(TsvIndices.EurAlleleCount),
            EurAlleleFrequency = splitLine.ParseDouble(TsvIndices.EurAlleleFrequency),
            EurHomCount        = splitLine.ParseInteger(TsvIndices.EurHomCount),

            OthAlleleNumber    = splitLine.ParseInteger(TsvIndices.OthAlleleNumber),
            OthAlleleCount     = splitLine.ParseInteger(TsvIndices.OthAlleleCount),
            OthAlleleFrequency = splitLine.ParseDouble(TsvIndices.OthAlleleFrequency),
            OthHomCount        = splitLine.ParseInteger(TsvIndices.OthHomCount),

            HasFailedFilters = hasFailedFilters
        };
    }

    private static VariantType SvTypeMapper(string svType)
    {
        // All possible values found in data (with counts):
        //      BND: 52604
        //      CN=0: 1108
        //      CPX: 4778
        //      CTX: 8
        //      DEL: 169635
        //      DUP: 49571
        //      INS: 31443
        //      INS:ME: 672
        //      INS:ME:ALU: 60475
        //      INS:ME:LINE1: 10018
        //      INS:ME:SVA: 6417
        //      INV: 748
        //      Total: 387477
        return svType switch
        {
            "BND"          => VariantType.translocation_breakend,
            "CN=0"         => VariantType.deletion,
            "CPX"          => VariantType.complex_structural_alteration,
            "CTX"          => VariantType.translocation_breakend,
            "DEL"          => VariantType.deletion,
            "DUP"          => VariantType.duplication,
            "INS"          => VariantType.insertion,
            "INS:ME"       => VariantType.mobile_element_insertion,
            "INS:ME:ALU"   => VariantType.mobile_element_insertion,
            "INS:ME:LINE1" => VariantType.mobile_element_insertion,
            "INS:ME:SVA"   => VariantType.mobile_element_insertion,
            "INV"          => VariantType.inversion,
            _              => throw new InvalidDataException("unknown svType")
        };
    }
}