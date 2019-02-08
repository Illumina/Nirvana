namespace VariantAnnotation.SA
{
    public enum JsonDataType : byte
    {
        String,
        Bool,
        Number
    }

    public static class JsonDataTypeExtension
    {
        public static string ToTypeString(this JsonDataType jsonDataType)
        {
            switch (jsonDataType)
            {
                case JsonDataType.String:
                    return "string";
                case JsonDataType.Bool:
                    return "boolean";
                case JsonDataType.Number:
                    return "number";
                default:
                    return "";
            }
        }
    }
}