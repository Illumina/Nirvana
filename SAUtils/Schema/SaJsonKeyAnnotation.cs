using VariantAnnotation.SA;

namespace SAUtils.Schema
{
    public sealed class SaJsonKeyAnnotation
    {
        public SaJsonKeyProperties Properties;
        public SaJsonSchema Schema;

        private SaJsonKeyAnnotation() { }

        public static SaJsonKeyAnnotation CreateFromProperties(SaJsonValueType valueType, CustomAnnotationCategories category, string description)
        {
            return new SaJsonKeyAnnotation {Properties = new SaJsonKeyProperties(valueType, category, description)};
        }

        public static SaJsonKeyAnnotation CreateFromSubSchema(SaJsonSchema schema)
        {
            return new SaJsonKeyAnnotation { Schema = schema};
        }
    }
}