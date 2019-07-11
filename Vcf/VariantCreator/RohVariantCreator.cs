using System;
using Genome;
using VariantAnnotation.Interface.Positions;
using Variants;

namespace Vcf.VariantCreator
{
    public static class RohVariantCreator
    {
        public static IVariant Create(IChromosome chromosome, int start, string refAllele, string altAllele, IInfoData infoData)
        {
            start++;
            int end = infoData?.End ?? start;

            if (altAllele != "<ROH>") throw new ArgumentException("The only allowed ALT allele is <ROH>.");

            string vid = $"{chromosome.EnsemblName}:{start}:{end}:ROH";
            return CreateRohVariant(chromosome, start, end, refAllele, altAllele, vid);
        }

        private static IVariant CreateRohVariant(IChromosome chromosome, int start, int end, string refAllele, string altAllele, string vid)
        {
            return new Variant(chromosome, start, end, refAllele, altAllele, VariantType.run_of_homozygosity, vid, false, false, false,
                null, null, AnnotationBehavior.RohBehavior);
        }
    }
}