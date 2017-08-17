using System.Collections.Generic;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IAnnotatedRegulatoryRegion:IJsonSerializer
    {
        IRegulatoryRegion RegulatoryRegion { get; }
        IEnumerable<ConsequenceTag> Consequences { get; }
    }
}