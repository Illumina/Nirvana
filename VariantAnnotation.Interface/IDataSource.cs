using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface IDataSource
    {
        void Clear();
        GenomeAssembly GenomeAssembly { get; }
        IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
    }
}
