using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface ICustomItem
    {
        string AnnotationType { get; }
        List<string> BooleanFields { get; }
        string Id { get; }
        string IsAlleleSpecific { get; set; }
        bool IsPositional { get; }
        string SaAltAllele { get; }
        Dictionary<string, string> StringFields { get; }
        void Write(IExtendedBinaryWriter writer);
    }
}
