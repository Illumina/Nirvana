namespace VariantAnnotation.Utilities
{
    public static class BaseFormatting
    {
        public static string EmptyToDash(string bases) => bases == "" ? "-" : bases;
    }
}