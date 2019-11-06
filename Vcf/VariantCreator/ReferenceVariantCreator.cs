using Genome;
using Variants;

namespace Vcf.VariantCreator
{
    public static class ReferenceVariantCreator
    {
        public static IVariant[] Create(ISequence sequence, IChromosome chromosome, int start, int end, string refAllele, string altAllele,
            string globalMajorAllele)
        {
            bool isRefMinor = end == start && globalMajorAllele != null;
            if (!isRefMinor) return null;

            string vid = VariantId.Create(sequence, VariantCategory.SmallVariant, null, chromosome, start, end, refAllele, altAllele);

            return new[]
            {
                SmallVariantCreator.Create(chromosome, start, globalMajorAllele, refAllele, false, false, null, vid, true)
            };
        }
    }
}