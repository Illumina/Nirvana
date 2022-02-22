using System.Collections.Generic;
using OptimizedCore;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using Variants;

namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedVariant
    {
        public IVariant                        Variant                  { get; }
        public string                          HgvsgNotation            { get; set; }
        public List<AnnotatedRegulatoryRegion> RegulatoryRegions        { get; } = new();
        public List<AnnotatedTranscript>       Transcripts              { get; } = new();
        public List<IAnnotatedSaDataSource>    SupplementaryAnnotations { get; } = new();
        public List<ISupplementaryAnnotation>  SaList                   { get; } = new();
        public double?                         PhylopScore              { get; set; }

        public AnnotatedVariant(IVariant variant) => Variant = variant;

        public string GetJsonString(string originalChromName)
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            // data section
            sb.Append(JsonObject.OpenBrace);

            jsonObject.AddStringValue("vid", Variant.VariantId);
            jsonObject.AddStringValue("chromosome", originalChromName);
            jsonObject.AddIntValue("begin", Variant.Start);
            jsonObject.AddIntValue("end", Variant.End);
            jsonObject.AddBoolValue("isReferenceMinorAllele", Variant.IsRefMinor);
            jsonObject.AddBoolValue("isStructuralVariant", Variant.Behavior.StructuralVariantConsequence);

            jsonObject.AddStringValue("refAllele",
                string.IsNullOrEmpty(Variant.RefAllele) ? "-" : Variant.RefAllele);
            jsonObject.AddStringValue("altAllele",
                string.IsNullOrEmpty(Variant.AltAllele) ? "-" : Variant.AltAllele);

            var variantType = GetVariantType(Variant.Type);
            jsonObject.AddStringValue("variantType", variantType.ToString());
            jsonObject.AddBoolValue("isDecomposedVariant", Variant.IsDecomposed);
            if (variantType.ToString() != "SNV") jsonObject.AddBoolValue("isRecomposedVariant", Variant.IsRecomposed);
            jsonObject.AddStringValue("hgvsg", HgvsgNotation);

            jsonObject.AddDoubleValue("phylopScore", PhylopScore);

            if (RegulatoryRegions?.Count > 0) jsonObject.AddObjectValues("regulatoryRegions", RegulatoryRegions);
            
            foreach (ISupplementaryAnnotation saItem in SaList)
            {
                jsonObject.AddObjectValue(saItem.JsonKey, saItem);
            }

            if (Transcripts?.Count > 0) jsonObject.AddObjectValues("transcripts", Transcripts);

            sb.Append(JsonObject.CloseBrace);
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        private static VariantType GetVariantType(VariantType variantType)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (variantType)
            {
                case VariantType.short_tandem_repeat_variation:
                case VariantType.short_tandem_repeat_contraction:
                case VariantType.short_tandem_repeat_expansion:
                    return VariantType.short_tandem_repeat_variation;
                default:
                    return variantType;
            }
        }
    }
}