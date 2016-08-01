using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface ISupplementaryInterval
    {
        int Start { get; }
        int End { get; }
        string ReferenceName { get; }
        string AlternateAllele { get; }
        VariantType VariantType { get; }
        string Source { get; }
        IReadOnlyDictionary<string, string> StringValues { get; }
        IReadOnlyDictionary<string, int> IntValues { get; }
        IEnumerable<string> BoolValues { get; }
        IReadOnlyDictionary<string, double> DoubleValues { get; }
        IReadOnlyDictionary<string, double> PopulationFrequencies { get; }
        IReadOnlyDictionary<string,IEnumerable<string>> StringLists { get; } 
    }
}