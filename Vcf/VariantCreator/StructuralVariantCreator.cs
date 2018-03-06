using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Vcf.VariantCreator
{
    public static class StructuralVariantCreator
    {
        private const string TandemDuplicationAltAllele = "<DUP:TANDEM>";

        private static readonly AnnotationBehavior StructuralVariantBehavior =
            new AnnotationBehavior(false, true, true, false, true, true);

        private static readonly AnnotationBehavior VerbosedStructuralVariantBehavior =
            new AnnotationBehavior(false, true, true, false, true, true, true);

        public static IVariant Create(IChromosome chromosome, int start, string refAllele, string altAllele,
            IBreakEnd[] breakEnds, IInfoData infoData, bool enableVerboseTranscript)
        {

            var svType = infoData?.SvType ?? VariantType.unknown;
            if (svType == VariantType.duplication && altAllele == TandemDuplicationAltAllele)
                svType = VariantType.tandem_duplication;

            if (svType != VariantType.translocation_breakend) start++;
            var end = infoData?.End ?? start;
            var vid = GetVid(chromosome.EnsemblName, start, end, svType, breakEnds);
            
            return new Variant(chromosome, start, end, refAllele, altAllele, svType, vid, false, false, false, null,
                breakEnds, enableVerboseTranscript ? VerbosedStructuralVariantBehavior : StructuralVariantBehavior);
        }

        
        private static string GetVid(string ensemblName, int start, int end, VariantType variantType,
            IReadOnlyList<IBreakEnd> breakEnds)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (variantType)
            {
                case VariantType.insertion:
                    return $"{ensemblName}:{start}:{end}:INS";

                case VariantType.deletion:
                    return $"{ensemblName}:{start}:{end}";

                case VariantType.duplication:
                    return $"{ensemblName}:{start}:{end}:DUP";

                case VariantType.tandem_duplication:
                    return $"{ensemblName}:{start}:{end}:TDUP";

                case VariantType.translocation_breakend:
                    return breakEnds?[0].ToString();

                case VariantType.inversion:
                    return $"{ensemblName}:{start}:{end}:Inverse";

                case VariantType.mobile_element_insertion:
                    return $"{ensemblName}:{start}:{end}:MEI";

                default:
                    return $"{ensemblName}:{start}:{end}";
            }
        }
    }
}