using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.MitoHeteroplasmy
{
    public sealed class MitoHeteroplasmyItem:ISupplementaryDataItem
    {
        private AlleleStats _stats;
        public MitoHeteroplasmyItem(IChromosome chromosome, int position, string refAllele, string altAllele, AlleleStats stats)
        {
            Chromosome = chromosome;
            Position   = position;
            RefAllele  = refAllele;
            AltAllele  = altAllele;
            _stats = stats;
        }

        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }
        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);
            jsonObject.AddDoubleValue("vrfMean", _stats.vrf_stats.mean, "0.######");
            jsonObject.AddDoubleValue("vrfStdev", _stats.vrf_stats.stdev, "0.######");
            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}