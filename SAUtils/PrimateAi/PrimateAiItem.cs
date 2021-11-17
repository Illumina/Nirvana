using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.PrimateAi
{
    public sealed class PrimateAiItem : ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }
        private string Hgnc { get; }
        public double ScorePercentile { get; }
        public PrimateAiItem(IChromosome chromosome, int position, string refAllele, string altAllele, string hgnc,
            double percentile)
        {
            Chromosome      = chromosome;
            Position        = position;
            RefAllele       = refAllele;
            AltAllele       = altAllele;
            Hgnc            = hgnc;
            ScorePercentile = percentile;
        }
        public string GetJsonString()
        {
            var sb = StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);
            jsonObject.AddStringValue("hgnc", Hgnc);
            jsonObject.AddDoubleValue("scorePercentile", ScorePercentile, "0.##");
            return StringBuilderPool.GetStringAndReturn(sb);
        }

        public string InputLine { get; set; }
    }
}