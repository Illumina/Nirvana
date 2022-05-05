using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;
using SAUtils.ParseUtils;
using Variants;

namespace SAUtils.gnomAD;

public sealed class GnomadSvTsvParser : GnomadSvParser
{
    public GnomadSvTsvParser(
        StreamReader reader,
        Dictionary<string, Chromosome> refNameDict
    ) : base(reader, refNameDict)
    {
        TsvIndices = new TsvIndices()
        {
            Chromosome = 7,
            Start      = 10,
            End        = 13,
            VariantId  = 1,
            SvType     = 2,

            AllAlleleCount     = 33,
            AllAlleleFrequency = 34,
            AllAlleleNumber    = 35
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

        start += 1; // +1 for padding base
        if (start > end)
        {
            (start, end) = (end, start);
        }

        // 'allele_count': 'AC=21,AFR_AC=10,AMR_AC=9,EAS_AC=0,EUR_AC=2,OTH_AC=0'
        // 'allele_frequency': 'AF=0.038889,AFR_AF=0.044643,AMR_AF=0.03913,EAS_AF=0,EUR_AF=0.023256,OTH_AF=0'
        // 'allele_number': 'AN=540,AFR_AN=224,AMR_AN=230,EAS_AN=0,EUR_AN=86,OTH_AN=0'
        Dictionary<string, int?>    countDict     = ParseValues(splitLine.GetString(TsvIndices.AllAlleleCount),     "AC", SplitLine.ParseInteger);
        Dictionary<string, double?> frequencyDict = ParseValues(splitLine.GetString(TsvIndices.AllAlleleFrequency), "AF", SplitLine.ParseDouble);
        Dictionary<string, int?>    numberDict    = ParseValues(splitLine.GetString(TsvIndices.AllAlleleNumber),    "AN", SplitLine.ParseInteger);

        return new GnomadSvItem(chromosome, inputLine)
        {
            Start     = (int) start,
            End       = (int) end,
            VariantId = splitLine.GetString(TsvIndices.VariantId),
            SvType    = svType,

            AllAlleleNumber    = numberDict["ALL"],
            AllAlleleCount     = countDict["ALL"],
            AllAlleleFrequency = frequencyDict["ALL"],

            AfrAlleleNumber    = numberDict["AFR"],
            AfrAlleleCount     = countDict["AFR"],
            AfrAlleleFrequency = frequencyDict["AFR"],

            AmrAlleleNumber    = numberDict["AMR"],
            AmrAlleleCount     = countDict["AMR"],
            AmrAlleleFrequency = frequencyDict["AMR"],

            EasAlleleNumber    = numberDict["EAS"],
            EasAlleleCount     = countDict["EAS"],
            EasAlleleFrequency = frequencyDict["EAS"],

            EurAlleleNumber    = numberDict["EUR"],
            EurAlleleCount     = countDict["EUR"],
            EurAlleleFrequency = frequencyDict["EUR"],

            OthAlleleNumber    = numberDict["OTH"],
            OthAlleleCount     = countDict["OTH"],
            OthAlleleFrequency = frequencyDict["OTH"]
        };
    }

    private static Dictionary<string, T> ParseValues<T>(string subString, string keyType, Func<string, T> parsingFunction)
    {
        // 'allele_count': 'AC=21,AFR_AC=10,AMR_AC=9,EAS_AC=0,EUR_AC=2,OTH_AC=0'
        string[]              splitValues = subString.OptimizedSplit(',');
        Dictionary<string, T> parsedDict  = new();

        foreach (string splitValue in splitValues)
        {
            (string key, string value) = splitValue.OptimizedKeyValue();
            if (!key.Equals(keyType))
            {
                string dictKey = key.Replace($"_{keyType}", "");
                parsedDict[dictKey] = parsingFunction(value);
                continue;
            }

            parsedDict["ALL"] = parsingFunction(value);
        }

        return parsedDict;
    }

    private static VariantType SvTypeMapper(string svType)
    {
        // https://www.ncbi.nlm.nih.gov/dbvar/content/var_summary/#nstd166
        // All possible values found in data (with counts):
        //      alu insertion: 61351
        //      copy number variation: 11383
        //      deletion: 161218
        //      duplication: 44560
        //      insertion: 26038
        //      inversion: 727
        //      line1 insertion: 10017
        //      mobile element insertion: 655
        //      sequence alteration: 4733
        //      sva insertion: 6547
        //      Total: 327229
        return svType switch
        {
            "alu insertion"            => VariantType.mobile_element_insertion,
            "copy number variation"    => VariantType.copy_number_variation,
            "deletion"                 => VariantType.deletion,
            "duplication"              => VariantType.duplication,
            "insertion"                => VariantType.insertion,
            "inversion"                => VariantType.inversion,
            "line1 insertion"          => VariantType.mobile_element_insertion,
            "mobile element insertion" => VariantType.mobile_element_insertion,
            "sequence alteration"      => VariantType.structural_alteration,
            "sva insertion"            => VariantType.mobile_element_insertion,
            _                          => throw new InvalidDataException("unknown svType")
        };
    }
}