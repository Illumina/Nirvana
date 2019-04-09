using VariantAnnotation.SA;

namespace SAUtils.Schema
{
    public sealed class  SaJsonKeyProperties
    {
        public SaJsonValueType ValueType;
        public CustomAnnotationCategories Category;
        public string Description;

        public SaJsonKeyProperties(SaJsonValueType valueType, CustomAnnotationCategories category, string description)
        {
            ValueType = valueType;
            Category = category;
            Description = description;
        }
    }
}