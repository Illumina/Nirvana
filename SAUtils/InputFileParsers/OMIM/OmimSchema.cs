using System.Linq;
using System.Text;
using SAUtils.Schema;
using VariantAnnotation.SA;

namespace SAUtils.InputFileParsers.OMIM
{
    public static class OmimSchema
    {
        private static readonly SaJsonValueType PrimaryValueType = SaJsonValueType.ObjectArray;

        private static readonly (string JsonKey, SaJsonValueType ValueType, SaJsonSchema subSchema)[] SchemaDescription = {
            ("mimNumber", SaJsonValueType.Number, null),
            ("description", SaJsonValueType.String, null),
            ("phenotypes", SaJsonValueType.ObjectArray, OmimPhenotypeSchema.Get())
        };

        public static SaJsonSchema Get()
        {
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), SaCommon.OmimTag, PrimaryValueType, SchemaDescription.Select(x => x.JsonKey));
            jsonSchema.SetNonSaKeys(new[] { "isAlleleSpecific" });

            foreach ((string key, var valueType, var subSchema) in SchemaDescription)
                jsonSchema.AddAnnotation(key, new SaJsonKeyAnnotation { ValueType = valueType, SubSchema = subSchema});

            return jsonSchema;
        }
    }
}