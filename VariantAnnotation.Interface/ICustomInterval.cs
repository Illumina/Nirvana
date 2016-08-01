using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface ICustomInterval
    {
        int Start { get; }
        int End { get; }
        string ReferenceName { get; }
        string Type { get;  }
        IDictionary<string, string> StringValues { get; }
        IDictionary<string, string> NonStringValues { get; }
    }
}