using Genome;
using VariantAnnotation.Interface.SA;

namespace SAUtils.DataStructures
{
    public sealed class AncestralAlleleItem: ISupplementaryDataItem
    {
        public          Chromosome Chromosome { get; }
        public          int         Position   { get; set; }
        public          string      RefAllele  { get; set; }
        public          string      AltAllele  { get; set; }
        public          string      InputLine  { get; }
        public readonly string      AncestralAllele;

        public AncestralAlleleItem(Chromosome chromosome, int position, string refAllele, string altAllele, string ancestralAllele, string inputLine)
        {
            Chromosome      = chromosome;
            Position        = position;
            RefAllele       = refAllele;
            AltAllele       = altAllele;
            AncestralAllele = ancestralAllele;
            InputLine       = inputLine;
        }

        public string GetJsonString()
        {
            return $"\"{AncestralAllele}\"";
        }
    }
}