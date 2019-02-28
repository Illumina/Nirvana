using Genome;
using VariantAnnotation.Interface.SA;
using Variants;

namespace SAUtils.DataStructures
{
    public sealed class MitoMapSvItem : ISuppIntervalItem
    {
        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; }
        public VariantType VariantType { get; }

        private readonly string _jsonString;

        public MitoMapSvItem(IChromosome chromosome, int start, int end, VariantType variantType, string jsonString)
        {
            Chromosome = chromosome;
            Start = start;
            End = end;
            VariantType = variantType;
            _jsonString = jsonString;
        }

        public string GetJsonString() => _jsonString;

    }
}