using System;
using System.Globalization;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.ClinGen
{
    public sealed class GeneDiseaseValidityItem: ISuppGeneItem
    {
        public string GeneSymbol { get; }

        public readonly string DiseaseId;
        private readonly string _disease;
        private readonly string _classification;
        private readonly string _classificationDate;


        public GeneDiseaseValidityItem(string geneSymbol, string diseaseId, string disease, string classification,
            string classificationDate)
        {
            GeneSymbol          = geneSymbol;
            DiseaseId           = diseaseId;
            _disease            = disease;
            _classification     = classification;
            _classificationDate = classificationDate;
        }

        public string GetJsonString()
        {
            var sb = StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("diseaseId", DiseaseId);
            jsonObject.AddStringValue("disease", _disease);
            jsonObject.AddStringValue("classification", _classification);
            jsonObject.AddStringValue("classificationDate", _classificationDate);
            sb.Append(JsonObject.CloseBrace);

            return StringBuilderPool.GetStringAndReturn(sb);
        }

        public int CompareDate(GeneDiseaseValidityItem other)
        {
            var date = DateTime.ParseExact(_classificationDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var otherDate = DateTime.ParseExact(other._classificationDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            return date.CompareTo(otherDate);
        }
    }

}