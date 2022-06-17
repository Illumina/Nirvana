using ErrorHandling.Exceptions;
using Genome;
using OptimizedCore;
using Variants;

namespace Vcf.VariantCreator
{
    public static class RepeatExpansionCreator
    {
        public static IVariant Create(Chromosome chromosome, int start, int end, string refAllele, string altAllele, int? refRepeatCount, string vid)
        {
            (int repeatCount, bool foundError) = altAllele.Trim('<', '>').Substring(3).OptimizedParseInt32();
            if (foundError) throw new UserErrorException($"Invalid alt allele ({altAllele}) found at {chromosome.UcscName}:{start}");

            start++;

            return new RepeatExpansion(chromosome, start, end, refAllele, altAllele, vid, repeatCount, refRepeatCount);
        }
    }
}