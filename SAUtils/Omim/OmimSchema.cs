using System.Linq;
using System.Text;
using SAUtils.Schema;
using VariantAnnotation.SA;

namespace SAUtils.Omim
{
    public static class OmimSchema
    {
        private static readonly SaJsonValueType PrimaryValueType = SaJsonValueType.ObjectArray;

        private static readonly (string JsonKey, SaJsonValueType ValueType, SaJsonSchema subSchema)[] SchemaDescription = {
            ("mimNumber", SaJsonValueType.Number, null),
            ("geneName", SaJsonValueType.String, null),
            ("description", SaJsonValueType.String, null),
            ("phenotypes", null, OmimPhenotypeSchema.Get())
        };

        public static SaJsonSchema Get()
        {
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), SaCommon.OmimTag, PrimaryValueType, SchemaDescription.Select(x => x.JsonKey));
            jsonSchema.SetNonSaKeys(new[] { "isAlleleSpecific" });

            foreach ((string key, var valueType, var subSchema) in SchemaDescription)
            {
                var keyAnnotation = valueType == null
                    ? SaJsonKeyAnnotation.CreateFromSubSchema(subSchema)
                    : SaJsonKeyAnnotation.CreateFromProperties(valueType, 0, null);

                jsonSchema.AddAnnotation(key, keyAnnotation);
            }

            return jsonSchema;
        }
    }
}