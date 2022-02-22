using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace Vcf.VariantCreator
{
    public sealed class VariantFactory
    {
        private readonly Dictionary<string, Chromosome> _refNameToChromosome;
        private readonly ISequenceProvider _sequenceProvider;
        private const string StrPrefix = "<STR";

        public VariantFactory(ISequenceProvider sequenceProvider)
        {
            _sequenceProvider = sequenceProvider;
            _refNameToChromosome = sequenceProvider.RefNameToChromosome;
        }

        private static VariantCategory GetVariantCategory(string firstAltAllele, bool isReference, bool isSymbolicAllele, VariantType svType)
        {
            if (isReference)                          return VariantCategory.Reference;
            if (IsBreakend(firstAltAllele))           return VariantCategory.SV;
            if (!isSymbolicAllele)                    return VariantCategory.SmallVariant;
            if (firstAltAllele.StartsWith(StrPrefix)) return VariantCategory.RepeatExpansion;
            return svType == VariantType.copy_number_variation ? VariantCategory.CNV : VariantCategory.SV;
        }

        private static bool IsBreakend(string altAllele) => altAllele.Contains("[") || altAllele.Contains("]");

        private static bool IsSymbolicAllele(string altAllele) =>
            altAllele.OptimizedStartsWith('<') && altAllele.OptimizedEndsWith('>') &&
            !VcfCommon.NonInformativeAltAllele.Contains(altAllele);

        public IVariant[] CreateVariants(Chromosome chromosome, int start, int end, string refAllele,
            string[] altAlleles, IInfoData infoData, bool[] isDecomposed, bool isRecomposed, string globalMajorAllele)
        {
            string firstAltAllele = altAlleles[0];
            bool isReference      = globalMajorAllele != null;
            bool isSymbolicAllele = IsSymbolicAllele(firstAltAllele);
            var variantCategory   = GetVariantCategory(firstAltAllele, isReference, isSymbolicAllele, infoData.SvType);

            if (isReference) return new[] { GetVariant(chromosome, start, end, refAllele, firstAltAllele, infoData, variantCategory, isDecomposed[0], isRecomposed, globalMajorAllele) };

            var informativeAltAlleles = GetInformativeAltAlleles(altAlleles);
            if (informativeAltAlleles.Count == 0) return null;

            var variants = new IVariant[informativeAltAlleles.Count];

            _sequenceProvider.LoadChromosome(chromosome);

            for (var i = 0; i < informativeAltAlleles.Count; i++)
            {
                bool isDecomposedVar = isDecomposed[i];
                (int shiftedStart, string shiftedRef, string shiftedAlt) =
                    VariantUtils.TrimAndLeftAlign(start, refAllele, informativeAltAlleles[i], _sequenceProvider.Sequence);

                variants[i] = GetVariant(chromosome, shiftedStart, end - (start- shiftedStart), shiftedRef, shiftedAlt, infoData, variantCategory, isDecomposedVar, isRecomposed, null);
            }

            return variants;
        }

        private static List<string> GetInformativeAltAlleles(string[] altAlleles)
        {
            var informativeAltAlleles = new List<string>(altAlleles.Length);

            foreach (string altAllele in altAlleles)
            {
                if (VcfCommon.NonInformativeAltAllele.Contains(altAllele)) continue;
                informativeAltAlleles.Add(altAllele);
            }

            return informativeAltAlleles;
        }

        private IVariant GetVariant(Chromosome chromosome, int start, int end, string refAllele, string altAllele,
            IInfoData infoData, VariantCategory category, bool isDecomposedVar, bool isRecomposed, string globalMajorAllele)
        {
            switch (category)
            {
                case VariantCategory.Reference:
                    return ReferenceVariantCreator.Create(chromosome, start, end, refAllele, altAllele, globalMajorAllele);
                case VariantCategory.SmallVariant:
                    return SmallVariantCreator.Create(chromosome, start, refAllele, altAllele, isDecomposedVar, isRecomposed);
                case VariantCategory.SV:
                    var svBreakEnds = infoData.SvType == VariantType.translocation_breakend ?
                        GetTranslocationBreakends(chromosome, refAllele, altAllele, start)
                        : GetSvBreakEnds(chromosome.EnsemblName, start, infoData.SvType, infoData.End, infoData.IsInv3, infoData.IsInv5);
                    return StructuralVariantCreator.Create(chromosome, start, refAllele, altAllele, svBreakEnds, infoData);
                case VariantCategory.CNV:
                    return CnvCreator.Create(chromosome, start, refAllele, altAllele, infoData);
                case VariantCategory.RepeatExpansion:
                    return RepeatExpansionCreator.Create(chromosome, start, refAllele, altAllele, infoData);
                default:
                    throw new NotImplementedException("Unrecognized variant category.");
            }
        }

        internal IBreakEnd[] GetTranslocationBreakends(Chromosome chromosome1, string refAllele, string altAllele, int position1)
        {
            var breakendInfo = ParseBreakendAltAllele(refAllele, altAllele);
            return new IBreakEnd[] { new BreakEnd(chromosome1, breakendInfo.Chromosome2, position1, breakendInfo.Position2, breakendInfo.IsSuffix1, breakendInfo.IsSuffix2) };
        }

        internal IBreakEnd[] GetSvBreakEnds(string ensemblName, int start, VariantType svType, int? svEnd, bool isInv3, bool isInv5)
        {
            if (svEnd == null) return null;

            int end        = svEnd.Value;
            var breakEnds  = new IBreakEnd[2];
            var chromosome = ReferenceNameUtilities.GetChromosome(_refNameToChromosome, ensemblName);

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (svType)
            {
                case VariantType.deletion:
                    breakEnds[0] = new BreakEnd(chromosome, chromosome, start, end + 1, false, true);
                    breakEnds[1] = new BreakEnd(chromosome, chromosome, end + 1, start, true, false);
                    break;

                case VariantType.tandem_duplication:
                case VariantType.duplication:
                    breakEnds[0] = new BreakEnd(chromosome, chromosome, end, start, false, true);
                    breakEnds[1] = new BreakEnd(chromosome, chromosome, start, end, true, false);
                    break;

                case VariantType.inversion:
                    if (isInv3)
                    {
                        breakEnds[0] = new BreakEnd(chromosome, chromosome, start, end, false, false);
                        breakEnds[1] = new BreakEnd(chromosome, chromosome, end, start, false, false);
                        break;
                    }
                    if (isInv5)
                    {
                        breakEnds[0] = new BreakEnd(chromosome, chromosome, start + 1, end + 1, true, true);
                        breakEnds[1] = new BreakEnd(chromosome, chromosome, end + 1, start + 1, true, true);
                        break;
                    }

                    breakEnds[0] = new BreakEnd(chromosome, chromosome, start, end, false, false);
                    breakEnds[1] = new BreakEnd(chromosome, chromosome, end + 1, start + 1, true, true);
                    break;

                default:
                    return null;
            }

            return breakEnds;
        }

        private const string ForwardBreakEnd = "[";

        /// <summary>
		/// parses the alternate allele
		/// </summary>
		private (Chromosome Chromosome2, int Position2, bool IsSuffix1, bool IsSuffix2) ParseBreakendAltAllele(string refAllele, string altAllele)
        {
            string referenceName2;
            int position2;
            bool isSuffix2;

            // (\w+)([\[\]])([^:]+):(\d+)([\[\]])
            // ([\[\]])([^:]+):(\d+)([\[\]])(\w+)
            if (altAllele.StartsWith(refAllele))
            {
                var forwardRegex = new Regex(@"\w+([\[\]])([^:]+):(\d+)([\[\]])", RegexOptions.Compiled);
                var match        = forwardRegex.Match(altAllele);

                if (!match.Success)
                    throw new InvalidDataException(
                        "Unable to successfully parse the complex rearrangements for the following allele: " + altAllele);

                isSuffix2      = match.Groups[4].Value == ForwardBreakEnd;
                position2      = Convert.ToInt32(match.Groups[3].Value);
                referenceName2 = match.Groups[2].Value;

                return (ReferenceNameUtilities.GetChromosome(_refNameToChromosome, referenceName2), position2, false, isSuffix2);
            }
            else
            {
                var reverseRegex = new Regex(@"([\[\]])([^:]+):(\d+)([\[\]])\w+", RegexOptions.Compiled);
                var match        = reverseRegex.Match(altAllele);

                if (!match.Success)
                    throw new InvalidDataException(
                        "Unable to successfully parse the complex rearrangements for the following allele: " + altAllele);

                isSuffix2      = match.Groups[1].Value == ForwardBreakEnd;
                position2      = Convert.ToInt32(match.Groups[3].Value);
                referenceName2 = match.Groups[2].Value;

                return (ReferenceNameUtilities.GetChromosome(_refNameToChromosome, referenceName2), position2, true, isSuffix2);
            }
        }

        public enum VariantCategory
        {
            Reference,
            SmallVariant,
            SV,
            CNV,
            RepeatExpansion
        }
    }
}