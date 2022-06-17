using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class GlobalMinorItem:ISupplementaryDataItem
    {
        public Chromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        private readonly string _allele;
        private readonly double _frequency;

        public GlobalMinorItem(Chromosome chromosome, int position, string allele, double frequency)
        {
            Chromosome = chromosome;
            Position   = position;
            _allele    = allele;
            _frequency = frequency;
        }

        public string GetJsonString()
        {
            var sb = StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("globalMinorAllele", _allele);
            jsonObject.AddDoubleValue("globalMinorAlleleFrequency", _frequency, "0.#######");
            sb.Append(JsonObject.CloseBrace);

            return StringBuilderPool.GetStringAndReturn(sb);
        }

        public string InputLine { get; set; }
    }
}