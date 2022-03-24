using System;
using Genome;
using VariantAnnotation.Interface.SA;

namespace SAUtils.GenericScore.GenericScoreParser
{
    public sealed class GenericScoreItem : ISupplementaryDataItem
    {
        public Chromosome Chromosome { get; }
        public int         Position   { get; set; }
        public string      RefAllele  { get; set; }
        public string      AltAllele  { get; set; }

        public readonly double Score;

        public GenericScoreItem(Chromosome chromosome, int position, string refAllele, string altAllele, double score)
        {
            Chromosome = chromosome;
            Position   = position;
            RefAllele  = refAllele;
            AltAllele  = altAllele;
            Score      = score;
        }

        [Obsolete]
        public string GetJsonString() => $"\"score\":{Score}";

        public string InputLine { get; }
    }
}