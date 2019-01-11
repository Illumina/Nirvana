using System.Collections.Generic;

namespace SAUtils
{
    public sealed class SaJsonKeyAnnotation
    {
        public string Type;
        public string Category;
        public string Description;

        public IEnumerable<(string, string)> GetDefinedAnnotations()
        {
            if (Type != null) yield return ("type", Type);
            if (Category != null) yield return ("category", Category);
            if (Description != null) yield return ("description", Description);
        }
    }
}