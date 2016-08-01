using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface IVariant
    {
        // required
        string ReferenceName { get; }
        int ReferencePosition { get; }
        string ReferenceAllele { get; }
        IEnumerable<string> AlternateAlleles { get; }
        string[] Fields { get; }

        // optional
        // additional information (filter, samples, vcf info, etc.) about the variant may be presented in a dictionary
        IReadOnlyDictionary<string, string> AdditionalInfo { get; }
    }
}
