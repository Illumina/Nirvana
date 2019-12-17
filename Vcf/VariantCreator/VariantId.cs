using Genome;
using VariantAnnotation.Interface;

namespace Vcf.VariantCreator
{
    public sealed class VariantId : IVariantIdCreator
    {
        public string Create(ISequence sequence, VariantCategory category, string svType, IChromosome chromosome, int start, int end,
            string refAllele, string altAllele, string repeatUnit)
        {
            if (altAllele == ".") altAllele = refAllele;

            // fix N reference
            if (refAllele == "N")
            {
                refAllele = sequence.Substring(start - 1, 1);
            }

            // add padding bases
            if (string.IsNullOrEmpty(refAllele) || string.IsNullOrEmpty(altAllele))
            {
                start--;
                string paddingBase = sequence.Substring(start - 1, 1);
                refAllele = paddingBase + refAllele;
                altAllele = paddingBase + altAllele;
            }

            if (category == VariantCategory.SmallVariant ||
                category == VariantCategory.Reference ||
                svType   == "BND")
            {
                return GetVid(chromosome.EnsemblName, start, refAllele, altAllele);
            }

            if (category == VariantCategory.RepeatExpansion) svType = "STR";
            return GetLongVid(chromosome.EnsemblName, start, end, refAllele, altAllele, svType);
        }

        private static string GetVid(string chromosomeName, int paddedPosition, string paddedRefAllele,
            string paddedAltAllele) =>
            chromosomeName + '-' + paddedPosition + '-' + paddedRefAllele + '-' + paddedAltAllele;

        private static string GetLongVid(string chromosomeName, int paddedPosition, int endPosition,
            string paddedRefAllele, string paddedAltAllele, string svType) =>
            chromosomeName + '-' + paddedPosition + '-' + endPosition + '-' + paddedRefAllele + '-' + paddedAltAllele +
            '-' + svType;
    }
}
