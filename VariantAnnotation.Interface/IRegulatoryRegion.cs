using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface IRegulatoryRegion
    {
        string ID { get; } 
        IEnumerable<string> Consequence { get; } 
    }
}