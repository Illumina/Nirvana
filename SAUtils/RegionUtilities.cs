using System;
using Genome;
using Intervals;
using VariantAnnotation.Interface.SA;

namespace SAUtils
{
    public static class RegionUtilities
    {
        public static IInterval Grch37Par1 = new Interval(10_001, 2_649_520);
        public static IInterval Grch37Par2 = new Interval(59034050, 59363566);
        
        public static IInterval Grch38Par1 = new Interval(10_001, 2781479);
        public static IInterval Grch38Par2 = new Interval(56887903, 57217415);
        

        public static bool OverlapsParRegion(ISupplementaryDataItem variant, GenomeAssembly assembly)
        {
            if (variant.Chromosome.UcscName != "chrY") return false;

            var start = variant.Position;
            var end   = variant.Position + Math.Max(variant.AltAllele.Length, variant.RefAllele.Length);
            switch (assembly)
            {
                case GenomeAssembly.hg19:
                case GenomeAssembly.GRCh37:
                    return Grch37Par1.Overlaps(start, end) || Grch37Par2.Overlaps(start, end);
                case GenomeAssembly.GRCh38:
                    return Grch38Par1.Overlaps(start, end) || Grch38Par2.Overlaps(start, end);
                default:
                    return false;
            }
            
        }
    }
}