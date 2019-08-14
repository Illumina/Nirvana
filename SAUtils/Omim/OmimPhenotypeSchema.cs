using System.Linq;
using System.Text;
using SAUtils.Schema;

namespace SAUtils.Omim
{
    public static class OmimPhenotypeSchema
    {
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
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), null, PrimaryValueType, SchemaDescription.Select(x => x.JsonKey));
            jsonSchema.SetNonSaKeys(new[] { "isAlleleSpecific" });

            foreach ((string key, var valueType) in SchemaDescription)
                jsonSchema.AddAnnotation(key, SaJsonKeyAnnotation.CreateFromProperties(valueType, 0, null));

            return jsonSchema;
        }
    }
}