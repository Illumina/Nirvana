namespace VariantAnnotation.SA
{
    public enum CustomAnnotationType:byte
    {
        String,
        Bool,
        Number
    }

    public static class CustomAnnotationTypeMethods
    {
        public static string ToJsonTypeString(this CustomAnnotationType customAnnotationType)
        {
            switch (customAnnotationType)
            {
                case CustomAnnotationType.String:
                    return "string";
                case CustomAnnotationType.Bool:
                    return "boolean";
                case CustomAnnotationType.Number:
                    return "number";
                default:
                    return "";
            }
        }
    }
}