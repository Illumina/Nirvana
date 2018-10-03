using Genome;
using VariantAnnotation.Interface.SA;
using Variants;

namespace SAUtils.DataStructures
{
    public sealed class MitoMapSvItem : ISuppIntervalItem
    {
        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; } = new Chromosome("chrM", "MT", 24);
        public VariantType VariantType { get; }

        private readonly string _jsonString;

        public MitoMapSvItem(int start, int end, VariantType variantType, string jsonString)
        {
            Start = start;
            End = end;
            VariantType = variantType;
            _jsonString = jsonString;
        }

        public string GetJsonString() => _jsonString;

    }
}