using System.Collections.Generic;
using CommonUtilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedVariant : IAnnotatedVariant
    {
        public IVariant Variant { get; }
        public string HgvsgNotation { get; set; }
        public IList<IAnnotatedRegulatoryRegion> RegulatoryRegions { get; }    = new List<IAnnotatedRegulatoryRegion>();
        public IList<IAnnotatedTranscript> EnsemblTranscripts { get; }         = new List<IAnnotatedTranscript>();
        public IList<IAnnotatedTranscript> RefSeqTranscripts { get; }          = new List<IAnnotatedTranscript>();
        public IList<IAnnotatedSaDataSource> SupplementaryAnnotations { get; } = new List<IAnnotatedSaDataSource>();
        public ISet<string> OverlappingGenes { get; }                          = new HashSet<string>(); 
        public IList<IOverlappingTranscript> OverlappingTranscripts { get; }   = new List<IOverlappingTranscript>();
        public double? PhylopScore { get; set; }

        private static readonly string[] TranscriptLabels = { "refSeq", "ensembl" };

        public IList<IPluginData> PluginDataSet { get; } = new List<IPluginData>();

        public AnnotatedVariant(IVariant variant)
        {
            Variant = variant;
        }

        
        public string GetJsonString(string originalChromName)
        {
            var sb         = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            // data section
            sb.Append(JsonObject.OpenBrace);

            jsonObject.AddStringValue("vid", Variant.VariantId);
            jsonObject.AddStringValue("chromosome", originalChromName);
            jsonObject.AddIntValue("begin", Variant.Start);
            jsonObject.AddIntValue("end", Variant.End);
            jsonObject.AddBoolValue("isReferenceMinorAllele", Variant.IsRefMinor);

            if (!Variant.IsRefMinor)
            {
                jsonObject.AddStringValue("refAllele",
                    string.IsNullOrEmpty(Variant.RefAllele) ? "-" : Variant.RefAllele);
                jsonObject.AddStringValue("altAllele",
                    string.IsNullOrEmpty(Variant.AltAllele) ? "-" : Variant.AltAllele);
            }
            else
            {
                jsonObject.AddStringValue("refAllele",
                    string.IsNullOrEmpty(Variant.AltAllele) ? "-" : Variant.AltAllele);
            }

	        var variantType = GetVariantType(Variant.Type);
			jsonObject.AddStringValue("variantType", variantType.ToString());
            jsonObject.AddBoolValue("isDecomposedVariant", Variant.IsDecomposed);
            if (variantType.ToString() != "SNV") jsonObject.AddBoolValue("isRecomposedVariant", Variant.IsRecomposed);
            jsonObject.AddStringValue("hgvsg",HgvsgNotation);

            jsonObject.AddDoubleValue("phylopScore", PhylopScore);

            if (RegulatoryRegions?.Count > 0) jsonObject.AddObjectValues("regulatoryRegions", RegulatoryRegions);
            if (SupplementaryAnnotations.Count > 0) AddSAstoJsonObject(jsonObject);
            foreach (var pluginData in PluginDataSet)
            {
                jsonObject.AddStringValue(pluginData.Name, pluginData.GetJsonString(), false);
            }

            if(OverlappingGenes.Count>0) jsonObject.AddStringValues("overlappingGenes", OverlappingGenes);
            if(OverlappingTranscripts.Count>0) jsonObject.AddObjectValues("overlappingTranscripts",OverlappingTranscripts);

            if (EnsemblTranscripts?.Count > 0 || RefSeqTranscripts?.Count > 0)
            {
                jsonObject.AddGroupedObjectValues("transcripts", TranscriptLabels, RefSeqTranscripts, EnsemblTranscripts);
            }

            sb.Append(JsonObject.CloseBrace);
            return StringBuilderCache.GetStringAndRelease(sb);
        }

	    private static VariantType GetVariantType(VariantType variantType)
	    {
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

	    private void AddSAstoJsonObject(JsonObject jsonObject)
        {
            var saDict = new Dictionary<string, (bool, List<string>)>();
            foreach (var annotatedSa in SupplementaryAnnotations)
            {
                var sa = annotatedSa.SaDataSource;

                if (!saDict.ContainsKey(sa.KeyName))
                {
                    saDict[sa.KeyName] = (sa.IsArray, new List<string>());
                }

                var jsonStrings = annotatedSa.GetJsonStrings();
                if (jsonStrings != null) saDict[sa.KeyName].Item2.AddRange(jsonStrings);
            }

            foreach (var kvp in saDict)
            {
                if (kvp.Value.Item1)
                {
                    jsonObject.AddStringValues(kvp.Key, kvp.Value.Item2.ToArray(), false);
                }
                else
                {
                    jsonObject.AddStringValue(kvp.Key, kvp.Value.Item2[0], false);
                }
            }
        }
    }
}