using System.Collections.Generic;
using Genome;

namespace VariantAnnotation.Interface.Providers
{
    public interface IProvider
    {
		string Name { get; }
        GenomeAssembly Assembly { get; }
        IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
    }
}