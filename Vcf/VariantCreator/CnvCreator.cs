using Genome;
using VariantAnnotation.Interface.Positions;
using Variants;

namespace Vcf.VariantCreator
{
    public static class CnvCreator
    {
        public static IVariant Create(IChromosome chromosome, int start, string refAllele, string altAllele, IInfoData infoData)
        {
            start++;
            int end = infoData.End ?? start;
            string vid;

            switch (altAllele)
            {
                case "<CNV>":
                    vid = $"{chromosome.EnsemblName}:{start}:{end}:CNV";
                    return CreateCNV(chromosome, start, end, refAllele, altAllele, VariantType.copy_number_variation, vid);
                case "<DEL>":
                    vid = $"{chromosome.EnsemblName}:{start}:{end}:CDEL";
                    return CreateCNV(chromosome, start, end, refAllele, altAllele, VariantType.copy_number_loss, vid);
                case "<DUP>":
                    vid = $"{chromosome.EnsemblName}:{start}:{end}:CDUP";
                    return CreateCNV(chromosome, start, end, refAllele, altAllele, VariantType.copy_number_gain, vid);
            }

            // the remaining cases are either <CNV> or <CN1>, <CN4>, etc.
            // do not try to determine the overall copy number gain or loss
            // - for allele-specific you'll probably introduce inconsistency
            // - for normal <CNV>, you'll probably get type wrong for MT, sex chromosomes, etc.
            string trimmedAltAllele = altAllele.Substring(1, altAllele.Length - 2);
            vid = $"{chromosome.EnsemblName}:{start}:{end}:{trimmedAltAllele}";
            return CreateCNV(chromosome, start, end, refAllele, altAllele, VariantType.copy_number_variation, vid);
        }

        private static IVariant CreateCNV(IChromosome chromosome, int start, int end, string refAllele,
            string altAllele, VariantType variantType, string vid)
        {
            return new Variant(chromosome, start, end, refAllele, altAllele, variantType, vid, false, false, false,
                null, null, AnnotationBehavior.CnvBehavior);
        }
    }
}