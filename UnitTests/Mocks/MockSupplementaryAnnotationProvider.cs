using System.Collections.Generic;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;

namespace UnitTests.Mocks
{
    public class MockSupplementaryAnnotationProvider : SupplementaryAnnotationProviderBase, ISupplementaryAnnotationProvider
    {
        /// <summary>
        /// constructor
        /// </summary>
        public MockSupplementaryAnnotationProvider(List<ISupplementaryAnnotationReader> saReaders)
        {
            SaReaders = saReaders;

            BuildIntervalForests();

            HasSmallVariantIntervals = !(SmallVariantIntervalArray is NullIntervalSearch<IInterimInterval>);
            HasSvIntervals           = !(SvIntervalArray           is NullIntervalSearch<IInterimInterval>);
            HasAllVariantIntervals   = !(AllVariantIntervalArray   is NullIntervalSearch<IInterimInterval>);
        }

        public GenomeAssembly GenomeAssembly => GenomeAssembly.Unknown;
        public IEnumerable<IDataSourceVersion> DataSourceVersions => new List<IDataSourceVersion>();
        public void Clear() => SaReaders.Clear();
        public void Load(string ucscReferenceName) { }
    }
}
