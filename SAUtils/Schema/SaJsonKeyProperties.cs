using VariantAnnotation.SA;

namespace SAUtils.Schema
{
    public sealed class SaJsonKeyProperties
    {
        public readonly SaJsonValueType            ValueType;
        public readonly CustomAnnotationCategories Category;
        public readonly string                     Description;

        public SaJsonKeyProperties(SaJsonValueType valueType, CustomAnnotationCategories category, string description)
        {
            ValueType   = valueType;
            Category    = category;
            Description = description;
        }
    }
}