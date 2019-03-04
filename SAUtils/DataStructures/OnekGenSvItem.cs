using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using Variants;

namespace SAUtils.DataStructures
{
    public sealed class OnekGenSvItem: ISuppIntervalItem
    {
        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; }
        private VariantType VariantType { get; }

        private readonly int? _allAlleleNumber;
        private readonly int? _allAlleleCount;
        private readonly double? _allAlleleFrequency;
        private readonly double? _afrAlleleFrequency;
        private readonly double? _amrAlleleFrequency;
        private readonly double? _easAlleleFrequency;
        private readonly double? _eurAlleleFrequency;
        private readonly double? _sasAlleleFrequency;

        public OnekGenSvItem(IChromosome chromosome, int start, int end, VariantType variantType, string id, int? allAlleleNumber, int? allAlleleCount, double? allAlleleFrequency, double? afrAlleleFrequency, double? amrAlleleFrequency, double? easAlleleFrequency, double? eurAlleleFrequency, double? sasAlleleFrequency)
        {
            Chromosome = chromosome;
            Start = start;
            End = end;
            VariantType = variantType;
            Id = id;
            _allAlleleNumber = allAlleleNumber;
            _allAlleleCount = allAlleleCount;
            _allAlleleFrequency = allAlleleFrequency;
            _afrAlleleFrequency = afrAlleleFrequency;
            _amrAlleleFrequency = amrAlleleFrequency;
            _easAlleleFrequency = easAlleleFrequency;
            _eurAlleleFrequency = eurAlleleFrequency;
            _sasAlleleFrequency = sasAlleleFrequency;
        }

        
        private string Id { get; }
        
        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("chromosome", Chromosome.EnsemblName);
            jsonObject.AddIntValue("begin", Start);
            jsonObject.AddIntValue("end", End);
            jsonObject.AddStringValue("variantType", VariantType.ToString());

            jsonObject.AddStringValue("id", Id);
            jsonObject.AddIntValue("allAn", _allAlleleNumber);
            jsonObject.AddIntValue("allAc", _allAlleleCount);
            jsonObject.AddDoubleValue("allAf", _allAlleleFrequency, "0.######");
            jsonObject.AddDoubleValue("afrAf", _afrAlleleFrequency, "0.######");
            jsonObject.AddDoubleValue("amrAf", _amrAlleleFrequency, "0.######");
            jsonObject.AddDoubleValue("eurAf", _eurAlleleFrequency, "0.######");
            jsonObject.AddDoubleValue("easAf", _easAlleleFrequency, "0.######");
            jsonObject.AddDoubleValue("sasAf", _sasAlleleFrequency, "0.######");

            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}