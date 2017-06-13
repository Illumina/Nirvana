using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling.SupplementaryAnnotations
{
    public class SupplementaryAnnotationProvider : SupplementaryAnnotationProviderBase, ISupplementaryAnnotationProvider
    {
        private string _currentUcscReferenceName;
        private readonly List<string> _saDirs;

        public GenomeAssembly GenomeAssembly => SupplementaryAnnotationCommon.GetGenomeAssembly(_saDirs);

        public IEnumerable<IDataSourceVersion> DataSourceVersions => SupplementaryAnnotationCommon.GetDataSourceVersions(_saDirs);

        /// <summary>
        /// constructor
        /// </summary>
        public SupplementaryAnnotationProvider(IEnumerable<string> saDirs)
        {
            _saDirs = saDirs.ToList();
        }

        public void Load(string ucscReferenceName)
        {
            if (_saDirs == null || _saDirs.Count == 0 || ucscReferenceName == _currentUcscReferenceName) return;

            SaReaders = SupplementaryAnnotationCommon.GetReaders(_saDirs, ucscReferenceName);

            BuildIntervalForests();

            HasSmallVariantIntervals = !(SmallVariantIntervalArray is NullIntervalSearch<IInterimInterval>);
            HasSvIntervals           = !(SvIntervalArray is NullIntervalSearch<IInterimInterval>);
            HasAllVariantIntervals   = !(AllVariantIntervalArray is NullIntervalSearch<IInterimInterval>);

            _currentUcscReferenceName = ucscReferenceName;
        }

        public void Clear()
        {
            HasSmallVariantIntervals = false;
            HasSvIntervals           = false;
            HasAllVariantIntervals   = false;
            SaReaders.Clear();
        }
    }
}
