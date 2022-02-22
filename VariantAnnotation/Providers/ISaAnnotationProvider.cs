using System.Collections.Generic;
using Genome;

namespace VariantAnnotation.Providers;

public interface ISaAnnotationProvider : IAnnotationProvider
{
    void PreLoad(Chromosome chromosome, List<int> positions);
}