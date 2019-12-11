using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using Variants;
using Vcf.Sample;

namespace Vcf.VariantCreator
{
    public sealed class VariantFactory
    {
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        private readonly ISequence _sequence;
        public readonly FormatIndices FormatIndices = new FormatIndices();
        private readonly bool _useBroadVids;

        public VariantFactory(ISequence sequence, IDictionary<string, IChromosome> refNameToChromosome, bool useLegacyVids)
        {
            _sequence            = sequence;
            _refNameToChromosome = refNameToChromosome;
            _useBroadVids        = !useLegacyVids;
        }

        public IVariant[] CreateVariants(IChromosome chromosome, int start, int end, string refAllele,
            string[] altAlleles, IInfoData infoData, bool[] isDecomposedByAllele, bool isRecomposed, List<string>[] linkedVids, string globalMajorAllele)
        {
            bool isReference = globalMajorAllele != null;

            if (isReference)
                return ReferenceVariantCreator.Create(_sequence, chromosome, start, end, refAllele,
                    altAlleles[0], globalMajorAllele);

            var variantCategory = GetVariantCategory(altAlleles[0], infoData.SvType);

            var variants = new List<IVariant>(altAlleles.Length);

            for (var i = 0; i < altAlleles.Length; i++)
            {
#if (!NI_ALLELE)
                if (VcfCommon.IsNonInformativeAltAllele(altAlleles[i])) continue;
#endif
                string altAllele = altAlleles[i];

                bool isDecomposed = isDecomposedByAllele[i];
                if (isDecomposed && isRecomposed) throw new InvalidDataException("A variant can't be both decomposed and recomposed");

                (int shiftedStart, string shiftedRef, string shiftedAlt) =
                    VariantUtils.TrimAndLeftAlign(start, refAllele, altAllele, _sequence);

                variants.Add(GetVariant(chromosome, shiftedStart, end - (start- shiftedStart), shiftedRef, shiftedAlt, infoData, variantCategory, isDecomposed, isRecomposed, linkedVids?[i]?.ToArray()));
            }

            return variants.Count == 0 ? null : variants.ToArray();
        }

        private static VariantCategory GetVariantCategory(string firstAltAllele, string svType)
        {
            bool isSymbolicAllele = IsSymbolicAllele(firstAltAllele);

            if (IsBreakend(firstAltAllele)) return VariantCategory.SV;
            if (!isSymbolicAllele) return VariantCategory.SmallVariant;
            if (firstAltAllele == "<ROH>") return VariantCategory.ROH;
            if (firstAltAllele.StartsWith("<STR")) return VariantCategory.RepeatExpansion;
            return svType == "CNV" ? VariantCategory.CNV : VariantCategory.SV;
        }

        private static bool IsBreakend(string altAllele) => altAllele.Contains("[") || altAllele.Contains("]");

        private static bool IsSymbolicAllele(string altAllele) =>
            altAllele.OptimizedStartsWith('<') && altAllele.OptimizedEndsWith('>') && !VcfCommon.IsNonInformativeAltAllele(altAllele);

        private IVariant GetVariant(IChromosome chromosome, int start, int end, string refAllele, string altAllele,
            IInfoData infoData, VariantCategory category, bool isDecomposed, bool isRecomposed, string[] linkedVids)
        {
            string vid;
            if (_useBroadVids)
            {
                vid = VariantId.Create(_sequence, category, infoData.SvType, chromosome, start, end, refAllele,
                    altAllele);
            }
            else
            {
                vid = LegacyVariantId.Create(_refNameToChromosome, category, infoData.SvType, chromosome, start, end,
                    refAllele, altAllele, infoData.RepeatUnit);
            }

            int svEnd  = infoData.End ?? start;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (category)
            {
                case VariantCategory.SmallVariant:
                    return SmallVariantCreator.Create(chromosome, start, refAllele, altAllele, isDecomposed,
                        isRecomposed, linkedVids, vid, false);

                case VariantCategory.ROH:
                    return RohVariantCreator.Create(chromosome, start, svEnd, refAllele, altAllele, vid);

                case VariantCategory.SV:
                    IBreakEnd[] svBreakEnds = infoData.SvType == "BND"
                        ? BreakEndCreator.CreateFromTranslocations(_refNameToChromosome, chromosome, refAllele, altAllele, start)
                        : BreakEndCreator.CreateFromDelDupInv(_refNameToChromosome, chromosome.EnsemblName, start, infoData.SvType, infoData.End);
                    return StructuralVariantCreator.Create(chromosome, start, svEnd, refAllele, altAllele, infoData.SvType, svBreakEnds, vid);

                case VariantCategory.CNV:
                    return CnvCreator.Create(chromosome, start, svEnd, refAllele, altAllele, vid);

                case VariantCategory.RepeatExpansion:
                    return RepeatExpansionCreator.Create(chromosome, start, svEnd, refAllele, altAllele, infoData.RefRepeatCount, vid);

                default:
                    throw new NotImplementedException($"Unrecognized variant category: {category}");
            }
        }
    }
}