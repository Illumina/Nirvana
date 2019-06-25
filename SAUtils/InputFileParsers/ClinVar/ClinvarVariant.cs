using System.Collections.Generic;
using Genome;

namespace SAUtils.InputFileParsers.ClinVar
{
    public sealed class ClinvarVariant
    {
        public readonly IChromosome Chromosome;
        public int Start { get; }
        public readonly int Stop;
        public readonly string ReferenceAllele;
        public readonly string AltAllele;
        public string VariantType;
        public readonly List<string> AllelicOmimIds;
        public readonly int? VariantId;

        public ClinvarVariant(IChromosome chr, int start, int stop, int? variantId, string refAllele, string altAllele, List<string> allilicOmimIds =null)
        {
            Chromosome      = chr;
            Start           = start;
            Stop            = stop;
            VariantId       = variantId;
            ReferenceAllele = refAllele ?? "";
            AltAllele       = altAllele ?? "";
            AllelicOmimIds  = allilicOmimIds ?? new List<string>();
        }
    }
}