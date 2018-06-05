using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.SA
{
    public sealed class SupplementaryAnnotationHeader : ISupplementaryAnnotationHeader
    {
        public string ReferenceSequenceName { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
	    public string Name { get; }
	    public GenomeAssembly Assembly { get; }

        public SupplementaryAnnotationHeader(string referenceSequenceName,
            IEnumerable<IDataSourceVersion> dataSourceVersions, GenomeAssembly genomeAssembly)
        {
            ReferenceSequenceName = referenceSequenceName;
            DataSourceVersions    = dataSourceVersions;
            Assembly              = genomeAssembly;
        }
    }
}