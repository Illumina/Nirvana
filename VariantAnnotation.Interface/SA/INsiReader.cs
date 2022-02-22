using System.Collections.Generic;
using Genome;
using Variants;
using Versioning;

namespace VariantAnnotation.Interface.SA
{
    public interface INsiReader
    {
        GenomeAssembly Assembly { get; }
        IDataSourceVersion Version { get; }
        string JsonKey { get; }
        ReportFor ReportFor { get; }
        IEnumerable<string> GetAnnotation(IVariant variant);
    }
}