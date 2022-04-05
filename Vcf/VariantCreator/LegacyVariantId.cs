using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface;
using Variants;

namespace Vcf.VariantCreator
{
    public sealed class LegacyVariantId : IVariantIdCreator
    {
        private readonly Dictionary<string, Chromosome> _refNameToChromosome;

        public LegacyVariantId(Dictionary<string, Chromosome> refNameToChromosome) => _refNameToChromosome = refNameToChromosome;

        public string Create(ISequence sequence, VariantCategory category, string svType, Chromosome chromosome, int start, int end,
            string refAllele, string altAllele, string repeatUnit)
        {
            switch (category)
            {
                case VariantCategory.Reference:
                    return $"{chromosome.EnsemblName}:{start}:{end}:{refAllele}";
                case VariantCategory.SV:
                    return GetSvVid(_refNameToChromosome, svType, chromosome, start, end, refAllele, altAllele);
                case VariantCategory.CNV:
                    return GetCnvVid(chromosome, start, end, altAllele);
                case VariantCategory.RepeatExpansion:
                    return GetRepeatExpansionVid(chromosome, start,end, altAllele, repeatUnit);
                case VariantCategory.ROH:
                    return $"{chromosome.EnsemblName}:{start + 1}:{end}:ROH";
                case VariantCategory.SmallVariant:
                    var variantType = SmallVariantCreator.GetVariantType(refAllele, altAllele);
                    return GetSmallVariantVid(chromosome, start, end, altAllele, variantType);
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }

        public (int Start, string RefAllele, string AltAllele) Normalize(ISequence sequence, int start, string refAllele, string altAllele)
        {
            if (altAllele.Contains('[') || altAllele.Contains(']')) return (start, refAllele, altAllele);
            return BiDirectionalTrimmer.Trim(start, refAllele, altAllele);
        }

        private static string GetSvVid(Dictionary<string, Chromosome> refNameToChromosome, string svType, Chromosome chromosome, int start, int end, string refAllele, string altAllele)
        {
            var variantType = StructuralVariantCreator.GetVariantType(altAllele, svType);

            switch (variantType)
            {
                case VariantType.insertion:
                    return $"{chromosome.EnsemblName}:{start + 1}:{end}:INS";

                case VariantType.deletion:
                    return $"{chromosome.EnsemblName}:{start + 1}:{end}";

                case VariantType.duplication:
                    return $"{chromosome.EnsemblName}:{start + 1}:{end}:DUP";

                case VariantType.tandem_duplication:
                    return $"{chromosome.EnsemblName}:{start + 1}:{end}:TDUP";

                case VariantType.translocation_breakend:
                    (Chromosome chromosome2, int position2, bool isSuffix1, bool isSuffix2) = ParseBreakendAltAllele(refNameToChromosome, refAllele, altAllele);
                    char orientation1 = isSuffix1 ? '-' : '+';
                    char orientation2 = isSuffix2 ? '+' : '-';
                    return $"{chromosome.EnsemblName}:{start}:{orientation1}:{chromosome2.EnsemblName}:{position2}:{orientation2}";

                case VariantType.inversion:
                    return $"{chromosome.EnsemblName}:{start + 1}:{end}:Inverse";

                case VariantType.mobile_element_insertion:
                    return $"{chromosome.EnsemblName}:{start + 1}:{end}:MEI";

                default:
                    return $"{chromosome.EnsemblName}:{start + 1}:{end}";
            }
        }

        private static (Chromosome Chromosome2, int Position2, bool IsSuffix1, bool IsSuffix2) ParseBreakendAltAllele(
            Dictionary<string, Chromosome> refNameToChromosome, string refAllele, string altAllele)
        {
            string referenceName2;
            int    position2;
            bool   isSuffix2;

            const string forwardBreakEnd = "[";

            if (altAllele.StartsWith(refAllele))
            {
                var   forwardRegex = new Regex(@"\w+([\[\]])(.+):(\d+)([\[\]])", RegexOptions.Compiled);
                Match match        = forwardRegex.Match(altAllele);

                if (!match.Success)
                    throw new InvalidDataException(
                        "Unable to successfully parse the complex rearrangements for the following allele: " + altAllele);

                isSuffix2      = match.Groups[4].Value == forwardBreakEnd;
                position2      = Convert.ToInt32(match.Groups[3].Value);
                referenceName2 = match.Groups[2].Value;

                return (ReferenceNameUtilities.GetChromosome(refNameToChromosome, referenceName2), position2, false, isSuffix2);
            }
            else
            {
                var   reverseRegex = new Regex(@"([\[\]])(.+):(\d+)([\[\]])\w+", RegexOptions.Compiled);
                Match match        = reverseRegex.Match(altAllele);

                if (!match.Success)
                    throw new InvalidDataException(
                        "Unable to successfully parse the complex rearrangements for the following allele: " + altAllele);

                isSuffix2      = match.Groups[1].Value == forwardBreakEnd;
                position2      = Convert.ToInt32(match.Groups[3].Value);
                referenceName2 = match.Groups[2].Value;

                return (ReferenceNameUtilities.GetChromosome(refNameToChromosome, referenceName2), position2, true, isSuffix2);
            }
        }

        private static string GetCnvVid(Chromosome chromosome, int start, int end, string altAllele)
        {
            start++;
            
            switch (altAllele)
            {
                case "<CNV>":
                    return $"{chromosome.EnsemblName}:{start}:{end}:CNV";
                case "<DEL>":
                    return $"{chromosome.EnsemblName}:{start}:{end}:CDEL";
                case "<DUP>":
                    return $"{chromosome.EnsemblName}:{start}:{end}:CDUP";
            }

            // ReSharper disable once PossibleNullReferenceException
            string trimmedAltAllele = altAllele.Substring(1, altAllele.Length - 2);
            return $"{chromosome.EnsemblName}:{start}:{end}:{trimmedAltAllele}";
        }

        internal static string GetSmallVariantVid(Chromosome chromosome, int start, int end, string altAllele, VariantType variantType)
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
                var md5Builder = StringBuilderPool.Get();
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(altAllele));

                md5Builder.Clear();
                foreach (byte b in data) md5Builder.Append(b.ToString("x2"));

                insAltAllele = StringBuilderPool.GetStringAndReturn(md5Builder);
            }

            return insAltAllele;
        }

        private static string GetRepeatExpansionVid(Chromosome chromosome, int start, int end, string altAllele,
            string repeatUnit)
        {
            string repeatCount = altAllele.Trim('<', '>').Substring(3);
            return $"{chromosome.EnsemblName}:{start + 1}:{end}:{repeatUnit}:{repeatCount}";
        }
    }
}