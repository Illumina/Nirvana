using System.Collections.Generic;
using VariantAnnotation.SA;

namespace SAUtils
{
    public sealed class SaJsonKeyAnnotation
    {
        public JsonDataType Type;
        public CustomAnnotationCategories Category;
        public string Description;

        public IEnumerable<(string, string)> GetDefinedAnnotations()
        {
            yield return ("type", Type.ToTypeString());
            if (Category != CustomAnnotationCategories.Unknown) yield return ("category", Category.ToString());
            if (Description != null) yield return ("description", Description);
        }
    }
}