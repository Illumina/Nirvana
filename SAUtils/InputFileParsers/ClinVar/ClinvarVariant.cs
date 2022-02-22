using System.Collections.Generic;
using Genome;

namespace SAUtils.InputFileParsers.ClinVar
{
    public sealed class ClinvarVariant
    {
        public readonly Chromosome Chromosome;
        public int Start { get; }
        public readonly int Stop;
        public readonly string ReferenceAllele;
        public readonly string AltAllele;
        public string VariantType;
        public readonly List<string> AllelicOmimIds;

        public ClinvarVariant(Chromosome chr, int start, int stop, string refAllele, string altAllele, List<string> allilicOmimIds =null)
        {
            Chromosome      = chr;
            Start           = start;
            Stop            = stop;
            ReferenceAllele = refAllele ?? "";
            AltAllele       = altAllele ?? "";
            AllelicOmimIds  = allilicOmimIds ?? new List<string>();
        }
    }
}