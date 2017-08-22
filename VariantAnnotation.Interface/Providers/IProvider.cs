using System.Collections.Generic;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Interface.Providers
{
    public interface IProvider
    {
		string Name { get; }
        GenomeAssembly GenomeAssembly { get; }
        IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
    }
}