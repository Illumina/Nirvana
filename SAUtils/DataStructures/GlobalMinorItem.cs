using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class GlobalMinorItem:ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        private readonly string _allele;
        private readonly double _frequency;

        public GlobalMinorItem(IChromosome chromosome, int position, string allele, double frequency)
        {
            Chromosome = chromosome;
            Position   = position;
            _allele    = allele;
            _frequency = frequency;
        }

        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("globalMinorAllele", _allele);
            jsonObject.AddDoubleValue("globalMinorAlleleFrequency", _frequency, "0.#######");
            sb.Append(JsonObject.CloseBrace);

            return StringBuilderCache.GetStringAndRelease(sb);
        }
        
    }
}