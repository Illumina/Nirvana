using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using Variants;

namespace SAUtils.MitoMap
{
    public sealed class MitoMapSvItem : ISuppIntervalItem
    {
        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; }
        private VariantType VariantType { get; }

        public MitoMapSvItem(IChromosome chromosome, int start, int end, VariantType variantType)
        {
            Chromosome = chromosome;
            Start = start;
            End = end;
            VariantType = variantType;
        }
        
        public string GetJsonString()
        {
            var sb= StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            // data section
            jsonObject.AddStringValue("chromosome", Chromosome.EnsemblName);
            jsonObject.AddIntValue("begin", Start);
            jsonObject.AddIntValue("end", End);
            jsonObject.AddStringValue("variantType", VariantType.ToString());

            return StringBuilderPool.GetStringAndReturn(sb);
        }

    }
}