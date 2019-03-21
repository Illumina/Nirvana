using System.Linq;
using System.Text;
using SAUtils.Schema;
using VariantAnnotation.SA;

namespace SAUtils.InputFileParsers.OMIM
{
    public static class OmimSchema
    {
        private static SaJsonSchema _jsonSchema;
        private static readonly SaJsonValueType PrimaryValueType = SaJsonValueType.ObjectArray;

        private static readonly (string JsonKey, SaJsonValueType ValueType, SaJsonSchema subSchema)[] SchemaDescription = {
            ("mimNumber", SaJsonValueType.Number, null),
            ("description", SaJsonValueType.String, null),
            ("phenotypes", SaJsonValueType.ObjectArray, OmimPhenotypeSchema.Get())
        };

        public static SaJsonSchema Get()
        {
            if (_jsonSchema != null) return _jsonSchema;

            _jsonSchema = SaJsonSchema.Create(new StringBuilder(), SaCommon.OmimTag, PrimaryValueType, SchemaDescription.Select(x => x.JsonKey));
            _jsonSchema.SetNonSaKeys(new[] { "isAlleleSpecific" });

            foreach ((string key, var valueType, var subSchema) in SchemaDescription)
                _jsonSchema.AddAnnotation(key, new SaJsonKeyAnnotation { ValueType = valueType, SubSchema = subSchema});

            return _jsonSchema;
        }
    }
}