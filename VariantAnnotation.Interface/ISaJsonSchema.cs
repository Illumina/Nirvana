using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface ISaJsonSchema
    {
        List<string> Keys { get; }
        int TotalItems { get; set; }
        void Count(string key);
        string GetJsonType(string key);
        string ToString();
        string GetJsonString(List<string> values);
    }
}