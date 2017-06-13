using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface ISupplementaryAnnotationHeader
    {
        string ReferenceSequenceName { get; }
        long CreationTimeTicks { get; }
        ushort DataVersion { get; }
        IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
        GenomeAssembly GenomeAssembly { get; }
    }
}