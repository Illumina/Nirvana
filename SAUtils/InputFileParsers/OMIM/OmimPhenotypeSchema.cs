using System.Linq;
using System.Text;
using SAUtils.Schema;
using VariantAnnotation.SA;

namespace SAUtils.InputFileParsers.OMIM
{
    public static class OmimPhenotypeSchema
    {
        private static SaJsonSchema _jsonSchema;
        private static readonly SaJsonValueType PrimaryValueType = SaJsonValueType.ObjectArray;

        private static readonly (string JsonKey, SaJsonValueType ValueType)[] SchemaDescription = {
            ("mimNumber", SaJsonValueType.Number),
            ("phenotype", SaJsonValueType.String),
            ("mapping", SaJsonValueType.String),
            ("inheritances", SaJsonValueType.StringArray),
            ("comments", SaJsonValueType.String)
        };

        public static SaJsonSchema Get()
        {
            if (_jsonSchema != null) return _jsonSchema;

            _jsonSchema = SaJsonSchema.Create(new StringBuilder(), null, PrimaryValueType, SchemaDescription.Select(x => x.JsonKey));
            _jsonSchema.SetNonSaKeys(new[] { "isAlleleSpecific" });

            foreach ((string key, var valueType) in SchemaDescription)
                _jsonSchema.AddAnnotation(key, new SaJsonKeyAnnotation { ValueType = valueType});

            return _jsonSchema;
        }
    }
}