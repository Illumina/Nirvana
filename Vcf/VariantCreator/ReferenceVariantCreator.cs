using Genome;
using VariantAnnotation.Interface;
using Variants;

namespace Vcf.VariantCreator
{
    public static class ReferenceVariantCreator
    {
        public static IVariant[] Create(IVariantIdCreator vidCreator, ISequence sequence, IChromosome chromosome, int start, int end,
            string refAllele, string altAllele, string globalMajorAllele)
        {
            bool isRefMinor = end == start && globalMajorAllele != null;
            if (!isRefMinor) return null;

            string vid = vidCreator.Create(sequence, VariantCategory.SmallVariant, null, chromosome, start, end, refAllele, altAllele, null);

            return new[]
            {
                SmallVariantCreator.Create(chromosome, start, end, globalMajorAllele, refAllele, false, false, null, vid, true)
            };
        }
    }
}