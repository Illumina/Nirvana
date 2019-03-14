namespace VariantAnnotation.SA
{
    public enum JsonDataType : byte
    {
        String,
        Bool,
        Number,
        Array,
        Object
    }

    public static class BacisJsonTypeExtension
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
                case JsonDataType.Array:
                    return "array";
                case JsonDataType.Object:
                    return "object";
                default:
                    return "";
            }
        }

        public static string GetSchemaKey(this JsonDataType jsonDataType)
        {
            switch (jsonDataType)
            {
                case JsonDataType.Array:
                    return "items";
                case JsonDataType.Object:
                    return "properties";
                default:
                    return "";
            }
        }

        public static bool IsComplexType(this JsonDataType jsonDataType)
        {
            switch (jsonDataType)
            {
                case JsonDataType.Array:
                case JsonDataType.Object:
                    return true;
                default:
                    return false;
            }
        }
    }
}