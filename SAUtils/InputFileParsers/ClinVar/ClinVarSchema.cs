using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAUtils.Schema;
using VariantAnnotation.SA;

namespace SAUtils.InputFileParsers.ClinVar
{
    public static class ClinVarSchema
    {
        private static SaJsonSchema _jsonSchema;
        private static readonly JsonDataType[] RootTypes = { JsonDataType.Array, JsonDataType.Object };
        private static readonly List<string> JsonKeys = new List<string>
        {
            "id",
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
            SaJsonValueType.String,
            SaJsonValueType.StringArray,
            SaJsonValueType.String,  
            SaJsonValueType.String,
            SaJsonValueType.StringArray,
            SaJsonValueType.StringArray,
            SaJsonValueType.StringArray,
            SaJsonValueType.StringArray,
            SaJsonValueType.String,
            SaJsonValueType.String,
            SaJsonValueType.StringArray,
            SaJsonValueType.Bool
        };

        public static SaJsonSchema Get()
        {
            if (_jsonSchema != null) return _jsonSchema;

            _jsonSchema = SaJsonSchema.Create(new StringBuilder(), SaCommon.ClinvarTag, RootTypes, JsonKeys);
            _jsonSchema.SetNonSaKeys(new []{"isAlleleSpecific"});

            foreach((string key, var valueType) in JsonKeys.Zip(ValueTypes, (a, b) => (a, b))) 
                _jsonSchema.AddAnnotation(key, new SaJsonKeyAnnotation{ValueType = valueType });

            return _jsonSchema;
        }
    }
}