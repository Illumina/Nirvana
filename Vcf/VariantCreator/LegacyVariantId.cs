using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Genome;
using OptimizedCore;
using Variants;

namespace Vcf.VariantCreator
{
    public static class LegacyVariantId
    {
        public static string Create(IDictionary<string, IChromosome> refNameToChromosome, VariantCategory category,
            string svType, IChromosome chromosome, int start, int end, string refAllele, string altAllele,
            string repeatUnit)
        {
            switch (category)
            {
                case VariantCategory.Reference:
                    return $"{chromosome.EnsemblName}:{start}:{end}:{refAllele}";
                case VariantCategory.SV:
                    return GetSvVid(refNameToChromosome, svType, chromosome, start, end, refAllele, altAllele);
                case VariantCategory.CNV:
                    return GetCnvVid(chromosome, start, end, altAllele);
                case VariantCategory.RepeatExpansion:
                    return GetRepeatExpansionVid(chromosome, start,end, altAllele, repeatUnit);
                case VariantCategory.ROH:
                    return $"{chromosome.EnsemblName}:{start}:{end}:ROH";
                case VariantCategory.SmallVariant:
                    var variantType = SmallVariantCreator.GetVariantType(refAllele, altAllele);
                    return GetSmallVariantVid(chromosome, start, end, refAllele, altAllele, variantType);
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }

        private static string GetSvVid(IDictionary<string, IChromosome> refNameToChromosome, string svType, IChromosome chromosome, int start, int end, string refAllele, string altAllele)
        {
            var variantType = StructuralVariantCreator.GetVariantType(altAllele, svType);

            switch (variantType)
            {
                case VariantType.insertion:
                    return $"{chromosome.EnsemblName}:{start}:{end}:INS";

                case VariantType.deletion:
                    return $"{chromosome.EnsemblName}:{start}:{end}";

                case VariantType.duplication:
                    return $"{chromosome.EnsemblName}:{start}:{end}:DUP";

                case VariantType.tandem_duplication:
                    return $"{chromosome.EnsemblName}:{start}:{end}:TDUP";

                case VariantType.translocation_breakend:
                    (IChromosome chromosome2, int position2, bool isSuffix1, bool isSuffix2) = BreakEndCreator.ParseBreakendAltAllele(refNameToChromosome, refAllele, altAllele);
                    char orientation1 = isSuffix1 ? '-' : '+';
                    char orientation2 = isSuffix2 ? '+' : '-';
                    return $"{chromosome.EnsemblName}:{start}:{orientation1}:{chromosome2.EnsemblName}:{position2}:{orientation2}";

                case VariantType.inversion:
                    return $"{chromosome.EnsemblName}:{start}:{end}:Inverse";

                case VariantType.mobile_element_insertion:
                    return $"{chromosome.EnsemblName}:{start}:{end}:MEI";

                default:
                    return $"{chromosome.EnsemblName}:{start}:{end}";
            }
        }

        private static string GetCnvVid(IChromosome chromosome, int start, int end, string altAllele)
        {
            switch (altAllele)
            {
                case "<CNV>":
                    return $"{chromosome.EnsemblName}:{start}:{end}:CNV";
                case "<DEL>":
                    return $"{chromosome.EnsemblName}:{start}:{end}:CDEL";
                case "<DUP>":
                    return $"{chromosome.EnsemblName}:{start}:{end}:CDUP";
            }

            string trimmedAltAllele = altAllele.Substring(1, altAllele.Length - 2);
            return $"{chromosome.EnsemblName}:{start}:{end}:{trimmedAltAllele}";
        }

        internal static string GetSmallVariantVid(IChromosome chromosome, int start, int end, string refAllele,
            string altAllele, VariantType variantType)
        {
            switch (variantType)
            {
                case VariantType.SNV:
                    return $"{chromosome.EnsemblName}:{start}:{altAllele}";
                case VariantType.insertion:
                    return $"{chromosome.EnsemblName}:{start}:{end}:{GetInsertedAltAllele(altAllele)}";
                case VariantType.deletion:
                    return $"{chromosome.EnsemblName}:{start}:{end}";
                case VariantType.MNV:
                case VariantType.indel:
                    return $"{chromosome.EnsemblName}:{start}:{end}:{GetInsertedAltAllele(altAllele)}";
                case VariantType.non_informative_allele:
                    return $"{chromosome.EnsemblName}:{start}:*";
                default:
                    throw new ArgumentOutOfRangeException(nameof(variantType), variantType, null);
            }
        }

        private static string GetInsertedAltAllele(string altAllele)
        {
            if (altAllele.Length <= 32) return altAllele;

            string insAltAllele;

            using (var md5Hash = MD5.Create())
            {
                var md5Builder = StringBuilderCache.Acquire();
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(altAllele));

                md5Builder.Clear();
                foreach (byte b in data) md5Builder.Append(b.ToString("x2"));

                insAltAllele = StringBuilderCache.GetStringAndRelease(md5Builder);
            }

            return insAltAllele;
        }

        private static string GetRepeatExpansionVid(IChromosome chromosome, int start, int end, string altAllele,
            string repeatUnit)
        {
            string repeatCount = altAllele.Trim('<', '>').Substring(3);
            return $"{chromosome.EnsemblName}:{start}:{end}:{repeatUnit}:{repeatCount}";
        }
    }
}