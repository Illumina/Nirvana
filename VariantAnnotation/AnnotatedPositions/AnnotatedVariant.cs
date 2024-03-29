﻿using System.Collections.Generic;
using System.Text;
using OptimizedCore;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using Variants;

namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedVariant : IAnnotatedVariant
    {
        public IVariant                          Variant                   { get; private set; }
        public string                            HgvsgNotation             { get; set; }
        public IList<IAnnotatedRegulatoryRegion> RegulatoryRegions         { get; } = new List<IAnnotatedRegulatoryRegion>();
        public IList<IAnnotatedTranscript>       Transcripts               { get; } = new List<IAnnotatedTranscript>();
        public IList<ISupplementaryAnnotation>   SaList                    { get; } = new List<ISupplementaryAnnotation>();
        public ISupplementaryAnnotation          RepeatExpansionPhenotypes { get; set; }
        public double?                           PhylopScore               { get; set; }
        public double?                           GerpScore               { get; set; }
        
        public bool InLowComplexityRegion { get; set; }
        
        public void Initialize(IVariant variant)
        {
            Variant       = variant;
            HgvsgNotation = null;
            RegulatoryRegions.Clear();
            Transcripts.Clear();
            SaList.Clear();
            RepeatExpansionPhenotypes = null;
            PhylopScore               = null;
            GerpScore                 = null;
        }

        
        public StringBuilder GetJsonStringBuilder(string originalChromName)
        {
            var sb         = StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            // data section
            sb.Append(JsonObject.OpenBrace);

            jsonObject.AddStringValue("vid", Variant.VariantId);
            jsonObject.AddStringValue("chromosome", originalChromName);
            jsonObject.AddIntValue("begin", Variant.Start);
            jsonObject.AddIntValue("end", Variant.End);
            jsonObject.AddBoolValue("isReferenceMinorAllele", Variant.IsRefMinor);
            jsonObject.AddBoolValue("isStructuralVariant", Variant.IsStructuralVariant);

            jsonObject.AddStringValue("refAllele",
                string.IsNullOrEmpty(Variant.RefAllele) ? "-" : Variant.RefAllele);
            jsonObject.AddStringValue("altAllele",
                string.IsNullOrEmpty(Variant.AltAllele) ? "-" : Variant.AltAllele);

            jsonObject.AddStringValue("variantType", Variant.Type.ToString());
            jsonObject.AddBoolValue("isDecomposedVariant", Variant.IsDecomposed);
            jsonObject.AddBoolValue("isRecomposedVariant", Variant.IsRecomposed);
            jsonObject.AddStringValues("linkedVids", Variant.LinkedVids);
            jsonObject.AddStringValue("hgvsg", HgvsgNotation);

            jsonObject.AddDoubleValue("phylopScore", PhylopScore);
            jsonObject.AddDoubleValue("gerpScore", GerpScore);
            jsonObject.AddBoolValue("inLowComplexityRegion", InLowComplexityRegion);

            if (RegulatoryRegions?.Count > 0) jsonObject.AddObjectValues("regulatoryRegions", RegulatoryRegions);

            foreach (ISupplementaryAnnotation saItem in SaList)
            {
                jsonObject.AddObjectValue(saItem.JsonKey, saItem);
            }

            jsonObject.AddObjectValue(RepeatExpansionPhenotypes?.JsonKey, RepeatExpansionPhenotypes);

            if (Transcripts?.Count > 0) jsonObject.AddObjectValues("transcripts", Transcripts);

            sb.Append(JsonObject.CloseBrace);
            return sb;
        }
    }
}