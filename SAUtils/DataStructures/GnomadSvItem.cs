using System.Text;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using Variants;

namespace SAUtils.DataStructures;

public sealed record GnomadSvItem(Chromosome Chromosome, string InputLine) : ISuppIntervalItem
{
    public int         Start            { get; init; }
    public int         End              { get; init; }
    public bool        HasFailedFilters { get; init; }
    public VariantType SvType           { get; init; }
    public string      VariantId        { get; init; }

    public double? AllAlleleFrequency    { get; init; }
    public double? AfrAlleleFrequency    { get; init; }
    public double? AmrAlleleFrequency    { get; init; }
    public double? EasAlleleFrequency    { get; init; }
    public double? EurAlleleFrequency    { get; init; }
    public double? OthAlleleFrequency    { get; init; }
    public double? FemaleAlleleFrequency { get; init; }
    public double? MaleAlleleFrequency   { get; init; }

    public int? AllAlleleCount    { get; init; }
    public int? AfrAlleleCount    { get; init; }
    public int? AmrAlleleCount    { get; init; }
    public int? EasAlleleCount    { get; init; }
    public int? EurAlleleCount    { get; init; }
    public int? OthAlleleCount    { get; init; }
    public int? FemaleAlleleCount { get; init; }
    public int? MaleAlleleCount   { get; init; }

    public int? AllAlleleNumber    { get; init; }
    public int? AfrAlleleNumber    { get; init; }
    public int? AmrAlleleNumber    { get; init; }
    public int? EasAlleleNumber    { get; init; }
    public int? EurAlleleNumber    { get; init; }
    public int? OthAlleleNumber    { get; init; }
    public int? FemaleAlleleNumber { get; init; }
    public int? MaleAlleleNumber   { get; init; }

    public int? AllHomCount    { get; init; }
    public int? AfrHomCount    { get; init; }
    public int? AmrHomCount    { get; init; }
    public int? EasHomCount    { get; init; }
    public int? EurHomCount    { get; init; }
    public int? OthHomCount    { get; init; }
    public int? FemaleHomCount { get; init; }
    public int? MaleHomCount   { get; init; }


    public string GetJsonString()
    {
        int start = Start;
        int end   = End;
        
        // swap bengin and end if variant is an insertion
        if (SvType == VariantType.insertion)
        {
            (start, end) = (end, start);
        }

        StringBuilder sb         = StringBuilderPool.Get();
        var           jsonObject = new JsonObject(sb);

        jsonObject.AddStringValue(JsonCommon.Chromosome, Chromosome.EnsemblName);
        jsonObject.AddIntValue(JsonCommon.Begin, start);
        jsonObject.AddIntValue(JsonCommon.End,   end);

        jsonObject.AddStringValue(JsonCommon.VariantId,   VariantId);
        jsonObject.AddStringValue(JsonCommon.VariantType, SvType.ToString());
        if (HasFailedFilters) jsonObject.AddBoolValue(JsonCommon.FailedFilter, true);

        jsonObject.AddDoubleValue(JsonCommon.AllAlleleFrequency,    AllAlleleFrequency,    JsonCommon.FrequencyRoundingFormat);
        jsonObject.AddDoubleValue(JsonCommon.AfrAlleleFrequency,    AfrAlleleFrequency,    JsonCommon.FrequencyRoundingFormat);
        jsonObject.AddDoubleValue(JsonCommon.AmrAlleleFrequency,    AmrAlleleFrequency,    JsonCommon.FrequencyRoundingFormat);
        jsonObject.AddDoubleValue(JsonCommon.EasAlleleFrequency,    EasAlleleFrequency,    JsonCommon.FrequencyRoundingFormat);
        jsonObject.AddDoubleValue(JsonCommon.EurAlleleFrequency,    EurAlleleFrequency,    JsonCommon.FrequencyRoundingFormat);
        jsonObject.AddDoubleValue(JsonCommon.OthAlleleFrequency,    OthAlleleFrequency,    JsonCommon.FrequencyRoundingFormat);
        jsonObject.AddDoubleValue(JsonCommon.FemaleAlleleFrequency, FemaleAlleleFrequency, JsonCommon.FrequencyRoundingFormat);
        jsonObject.AddDoubleValue(JsonCommon.MaleAlleleFrequency,   MaleAlleleFrequency,   JsonCommon.FrequencyRoundingFormat);

        jsonObject.AddIntValue(JsonCommon.AllAlleleCount,    AllAlleleCount);
        jsonObject.AddIntValue(JsonCommon.AfrAlleleCount,    AfrAlleleCount);
        jsonObject.AddIntValue(JsonCommon.AmrAlleleCount,    AmrAlleleCount);
        jsonObject.AddIntValue(JsonCommon.EasAlleleCount,    EasAlleleCount);
        jsonObject.AddIntValue(JsonCommon.EurAlleleCount,    EurAlleleCount);
        jsonObject.AddIntValue(JsonCommon.OthAlleleCount,    OthAlleleCount);
        jsonObject.AddIntValue(JsonCommon.FemaleAlleleCount, FemaleAlleleCount);
        jsonObject.AddIntValue(JsonCommon.MaleAlleleCount,   MaleAlleleCount);

        jsonObject.AddIntValue(JsonCommon.AllAlleleNumber,    AllAlleleNumber);
        jsonObject.AddIntValue(JsonCommon.AfrAlleleNumber,    AfrAlleleNumber);
        jsonObject.AddIntValue(JsonCommon.AmrAlleleNumber,    AmrAlleleNumber);
        jsonObject.AddIntValue(JsonCommon.EasAlleleNumber,    EasAlleleNumber);
        jsonObject.AddIntValue(JsonCommon.EurAlleleNumber,    EurAlleleNumber);
        jsonObject.AddIntValue(JsonCommon.OthAlleleNumber,    OthAlleleNumber);
        jsonObject.AddIntValue(JsonCommon.FemaleAlleleNumber, FemaleAlleleNumber);
        jsonObject.AddIntValue(JsonCommon.MaleAlleleNumber,   MaleAlleleNumber);

        jsonObject.AddIntValue(JsonCommon.AllHomCount,    AllHomCount);
        jsonObject.AddIntValue(JsonCommon.AfrHomCount,    AfrHomCount);
        jsonObject.AddIntValue(JsonCommon.AmrHomCount,    AmrHomCount);
        jsonObject.AddIntValue(JsonCommon.EasHomCount,    EasHomCount);
        jsonObject.AddIntValue(JsonCommon.EurHomCount,    EurHomCount);
        jsonObject.AddIntValue(JsonCommon.OthHomCount,    OthHomCount);
        jsonObject.AddIntValue(JsonCommon.FemaleHomCount, FemaleHomCount);
        jsonObject.AddIntValue(JsonCommon.MaleHomCount,   MaleHomCount);

        return StringBuilderPool.GetStringAndReturn(sb);
    }
}