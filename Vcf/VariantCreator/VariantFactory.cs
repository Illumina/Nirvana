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
using Vcf.Sample;

namespace Vcf.VariantCreator
{
    public sealed class VariantFactory
    {
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        private readonly ISequenceProvider _sequenceProvider;
        private const string StrPrefix = "<STR";
        private const string RohAltAllele = "<ROH>";

        public readonly FormatIndices FormatIndices = new FormatIndices();

        public VariantFactory(ISequenceProvider sequenceProvider)
        {
            _sequenceProvider = sequenceProvider;
            _refNameToChromosome = sequenceProvider.RefNameToChromosome;
        }

        private static VariantCategory GetVariantCategory(string firstAltAllele, bool isReference, bool isSymbolicAllele, VariantType svType)
        {
            if (isReference)                          return VariantCategory.Reference;
            if (firstAltAllele == RohAltAllele)       return VariantCategory.ROH; 
            if (IsBreakend(firstAltAllele))           return VariantCategory.SV;
            if (!isSymbolicAllele)                    return VariantCategory.SmallVariant;
            if (firstAltAllele.StartsWith(StrPrefix)) return VariantCategory.RepeatExpansion;
            return svType == VariantType.copy_number_variation ? VariantCategory.CNV : VariantCategory.SV;
        }

        private static bool IsBreakend(string altAllele) => altAllele.Contains("[") || altAllele.Contains("]");

        private static bool IsSymbolicAllele(string altAllele) =>
            altAllele.OptimizedStartsWith('<') && altAllele.OptimizedEndsWith('>') && !VcfCommon.IsNonInformativeAltAllele(altAllele);

        public IVariant[] CreateVariants(IChromosome chromosome, int start, int end, string refAllele,
            string[] altAlleles, IInfoData infoData, bool[] isDecomposed, bool isRecomposed, List<string>[] linkedVids, string globalMajorAllele)
        {
            string firstAltAllele = altAlleles[0];
            bool isReference      = globalMajorAllele != null;
            bool isSymbolicAllele = IsSymbolicAllele(firstAltAllele);
            var variantCategory   = GetVariantCategory(firstAltAllele, isReference, isSymbolicAllele, infoData.SvType);

            if (isReference) return new[] { GetVariant(chromosome, start, end, refAllele, firstAltAllele, infoData, variantCategory, isDecomposed[0], isRecomposed, linkedVids?[0]?.ToArray(), globalMajorAllele) };

            _sequenceProvider.LoadChromosome(chromosome);
            var variants = new List<IVariant>();
            for (var i = 0; i < altAlleles.Length; i++)
            {
#if (!NI_ALLELE)
                if (VcfCommon.IsNonInformativeAltAllele(altAlleles[i])) continue;
#endif
                bool isDecomposedVar = isDecomposed[i];
                (int shiftedStart, string shiftedRef, string shiftedAlt) =
                    VariantUtils.TrimAndLeftAlign(start, refAllele, altAlleles[i], _sequenceProvider.Sequence);

                variants.Add(GetVariant(chromosome, shiftedStart, end - (start- shiftedStart), shiftedRef, shiftedAlt, infoData, variantCategory, isDecomposedVar, isRecomposed, linkedVids?[i]?.ToArray(), null));
            }

            return variants.Count == 0 ? null : variants.ToArray();
        }

        private IVariant GetVariant(IChromosome chromosome, int start, int end, string refAllele, string altAllele,
            IInfoData infoData, VariantCategory category, bool isDecomposedVar, bool isRecomposed, string[] linkedVids, string globalMajorAllele)
        {
            switch (category)
            {
                case VariantCategory.Reference:
                    return ReferenceVariantCreator.Create(chromosome, start, end, refAllele, altAllele, globalMajorAllele);

                case VariantCategory.SmallVariant:
                    return SmallVariantCreator.Create(chromosome, start, refAllele, altAllele, isDecomposedVar, isRecomposed, linkedVids);

                case VariantCategory.ROH:
                    return RohVariantCreator.Create(chromosome, start, refAllele, altAllele, infoData);

                case VariantCategory.SV:
                    var svBreakEnds = infoData.SvType == VariantType.translocation_breakend ?
                        GetTranslocationBreakends(chromosome, refAllele, altAllele, start)
                        : GetSvBreakEnds(chromosome.EnsemblName, start, infoData.SvType, infoData.End);
                    return StructuralVariantCreator.Create(chromosome, start, refAllele, altAllele, svBreakEnds, infoData);

                case VariantCategory.CNV:
                    return CnvCreator.Create(chromosome, start, refAllele, altAllele, infoData);

                case VariantCategory.RepeatExpansion:
                    return RepeatExpansionCreator.Create(chromosome, start, refAllele, altAllele, infoData);
                default:
                    throw new NotImplementedException("Unrecognized variant category.");
            }
        }

        internal IBreakEnd[] GetTranslocationBreakends(IChromosome chromosome1, string refAllele, string altAllele, int position1)
        {
            (IChromosome chromosome2, int position2, bool isSuffix1, bool isSuffix2) = ParseBreakendAltAllele(refAllele, altAllele);
            return new IBreakEnd[] { new BreakEnd(chromosome1, chromosome2, position1, position2, isSuffix1, isSuffix2) };
        }

        internal IBreakEnd[] GetSvBreakEnds(string ensemblName, int start, VariantType svType, int? svEnd)
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
		private (IChromosome Chromosome2, int Position2, bool IsSuffix1, bool IsSuffix2) ParseBreakendAltAllele(string refAllele, string altAllele)
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
            RepeatExpansion,
            ROH
        }
    }
}