using System.Collections.Generic;
using Variants;

namespace VariantAnnotation.Interface.SA
{
    public interface INsiReader : ISaMetadata
    {
        ReportFor           ReportFor { get; }
        IEnumerable<string> GetAnnotation(IVariant variant);
    }
}