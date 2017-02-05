using System.Collections.Generic;
using System.IO;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling.SupplementaryAnnotations
{
    public class SupplementaryAnnotationProvider : ISupplementaryAnnotationProvider
    {        
        private string _currentUcscReferenceName;

        // supplementary annotation
        private readonly string _saDir;
        private ISupplementaryAnnotationReader _saReader;

        // supplementary intervals
        private bool _hasIntervals;
        private readonly List<ISupplementaryInterval> _overlappingIntervals;
        private IIntervalForest<ISupplementaryInterval> _intervalForest;

        public GenomeAssembly GenomeAssembly => SupplementaryAnnotationCommon.GetGenomeAssembly(_saDir);

        public IEnumerable<IDataSourceVersion> DataSourceVersions
            => SupplementaryAnnotationCommon.GetDataSourceVersions(_saDir);

        /// <summary>
        /// constructor
        /// </summary>
        public SupplementaryAnnotationProvider(string saDir)
        {
            _saDir                = saDir;
            _overlappingIntervals = new List<ISupplementaryInterval>();
            _intervalForest       = new NullIntervalSearch<ISupplementaryInterval>();
        }

        public void Load(string ucscReferenceName, IChromosomeRenamer renamer)
        {
            if (string.IsNullOrEmpty(_saDir) || ucscReferenceName == _currentUcscReferenceName) return;

            var saPath = Path.Combine(_saDir, ucscReferenceName + ".nsa");
            _saReader = File.Exists(saPath) ? new SupplementaryAnnotationReader(saPath) : null;

            _intervalForest = _saReader?.GetIntervalForest(renamer);
            _hasIntervals = !(_intervalForest is NullIntervalSearch<ISupplementaryInterval>);

            _currentUcscReferenceName = ucscReferenceName;
        }

        public void AddAnnotation(IVariantFeature variant)
        {
            if (_saReader == null) return;
            if (variant.IsStructuralVariant) AddOverlappingIntervals(variant);
            else variant.SetSupplementaryAnnotation(_saReader);
        }

        private void AddOverlappingIntervals(IVariantFeature variant)
        {
            if (!_hasIntervals) return;

            _overlappingIntervals.Clear();
            var firstAltAllele = variant.FirstAlternateAllele;

            var variantBegin = firstAltAllele.NirvanaVariantType == VariantType.insertion
                ? firstAltAllele.End
                : firstAltAllele.Start;
            var variantEnd = firstAltAllele.End;

            _intervalForest.GetAllOverlappingValues(variant.ReferenceIndex, variantBegin, variantEnd,
                _overlappingIntervals);

            variant.AddSupplementaryIntervals(_overlappingIntervals);
        }

        public void Clear()
        {
            _hasIntervals = false;
            _saReader     = null;
        }
    }
}
