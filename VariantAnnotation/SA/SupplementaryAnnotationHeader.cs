using System.Collections.Generic;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.SA
{
    public sealed class SupplementaryAnnotationHeader : ISupplementaryAnnotationHeader
    {
        public string ReferenceSequenceName { get; }
        private long CreationTimeTicks { get; }
        private ushort DataVersion { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
	    public string Name { get; }
	    public GenomeAssembly GenomeAssembly { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public SupplementaryAnnotationHeader(string referenceSequenceName, long creationTimeTicks, ushort dataVersion,
            IEnumerable<IDataSourceVersion> dataSourceVersions, GenomeAssembly genomeAssembly)
        {
            ReferenceSequenceName = referenceSequenceName;
            CreationTimeTicks = creationTimeTicks;
            DataVersion = dataVersion;
            DataSourceVersions = dataSourceVersions;
            GenomeAssembly = genomeAssembly;
        }
    }
}