using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAUtils.Schema;
using VariantAnnotation.SA;

namespace SAUtils.InputFileParsers.ClinVar
{
    public static class ClinVarSchema
    {
        private static readonly SaJsonValueType PrimaryValueType = SaJsonValueType.ObjectArray;

        private static readonly string[] JsonKeys = {
            "id",
            "variationId",
            "reviewStatus",
            "alleleOrigins",
            "refAllele",
            "altAllele",
            "phenotypes",
            "medGenIds",
            "omimIds",
            "orphanetIds",
            "significance",
            "lastUpdatedDate",
            "pubMedIds",
            "isAlleleSpecific"
        };

        private static readonly List<SaJsonValueType> ValueTypes = new List<SaJsonValueType>
        {
            SaJsonValueType.String,
            SaJsonValueType.Number,
            SaJsonValueType.String,
            SaJsonValueType.StringArray,
            SaJsonValueType.String,  
            SaJsonValueType.String,
            SaJsonValueType.StringArray,
            SaJsonValueType.StringArray,
            SaJsonValueType.StringArray,
            SaJsonValueType.StringArray,
            SaJsonValueType.StringArray,
            SaJsonValueType.String,
            SaJsonValueType.StringArray,
            SaJsonValueType.Bool
        };

        public static SaJsonSchema Get()
        {
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), SaCommon.ClinvarTag, PrimaryValueType, JsonKeys);
            jsonSchema.SetNonSaKeys(new []{"isAlleleSpecific"});

            foreach((string key, var valueType) in JsonKeys.Zip(ValueTypes, (a, b) => (a, b))) 
                jsonSchema.AddAnnotation(key, SaJsonKeyAnnotation.CreateFromProperties(valueType, 0, null));

            return jsonSchema;
        }
    }
}