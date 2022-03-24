using System.Collections.Generic;
using Genome;

namespace SAUtils.InputFileParsers.ClinVar
{
    public sealed class ClinvarVariant
    {
        public readonly Chromosome Chromosome;
        public int Start { get; }
        public readonly int Stop;
        public readonly string RefAllele;
        public readonly string AltAllele;
        public string VariantType;
        public readonly List<string> AllelicOmimIds;
        public readonly string VariantId;

        public ClinvarVariant(Chromosome chr, int start, int stop, string variantId, string refAllele, string altAllele, List<string> allilicOmimIds =null)
        {
            Chromosome      = chr;
            Start           = start;
            Stop            = stop;
            VariantId       = variantId;
            RefAllele       = refAllele;
            AltAllele       = altAllele;
            AllelicOmimIds  = allilicOmimIds ?? new List<string>();
        }
    }
}