using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.ClinGen
{
    public class GeneDiseaseValidityItem: ISuppGeneItem
    {
        public string GeneSymbol { get; }

        public readonly string DiseaseId;
        public readonly string Disease;
        public readonly string Classification;
        public readonly string ClassificationDate;


        public GeneDiseaseValidityItem(string geneSymbol, string diseaseId, string disease, string classification,
            string classificationDate)
        {
            GeneSymbol         = geneSymbol;
            DiseaseId          = diseaseId;
            Disease            = disease;
            Classification     = classification;
            ClassificationDate = classificationDate;
        }

        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("diseaseId", DiseaseId);
            jsonObject.AddStringValue("disease", Disease);
            jsonObject.AddStringValue("classification", Classification);
            jsonObject.AddStringValue("classificationDate", ClassificationDate);
            sb.Append(JsonObject.CloseBrace);

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        
    }

}