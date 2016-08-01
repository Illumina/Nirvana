using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface ICustomAnnotation
    {
        string Id { get; }
        string AnnotationType { get; }
        string AltAllele { get; }
        bool IsPositional { get; }
        string IsAlleleSpecific { get; }
        IDictionary<string, string> StringFields { get; }
        IEnumerable<string> BooleanFields { get; }
    }
}