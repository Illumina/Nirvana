using System.Collections.Generic;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace UnitTests.Mocks
{
    public class MockSupplementaryAnnotationProvider : ISupplementaryAnnotationProvider
    {
        private ISupplementaryAnnotationReader _saReader;

        // supplementary intervals
        private readonly List<ISupplementaryInterval> _overlappingSupplementaryIntervals;
        private readonly IIntervalForest<ISupplementaryInterval> _suppIntervalForest;
        public GenomeAssembly GenomeAssembly => GenomeAssembly.Unknown;
        public IEnumerable<IDataSourceVersion> DataSourceVersions => new List<IDataSourceVersion>();
        public void Clear() => _saReader = null;

        /// <summary>
        /// constructor
        /// </summary>
        public MockSupplementaryAnnotationProvider(ISupplementaryAnnotationReader saReader, ChromosomeRenamer renamer)
        {
            if (saReader == null) return;
            _saReader = saReader;

            _overlappingSupplementaryIntervals = new List<ISupplementaryInterval>();
            _suppIntervalForest = _saReader.GetIntervalForest(renamer);
        }

        public void AddAnnotation(IVariantFeature variant)
        {
            if (_saReader == null) return;
            if (variant.IsStructuralVariant) AddOverlappingIntervals(variant);
            else variant.SetSupplementaryAnnotation(_saReader);
        }

        private void AddOverlappingIntervals(IVariantFeature variant)
        {
            // get overlapping supplementary intervals.
            _overlappingSupplementaryIntervals.Clear();

            var firstAltAllele = variant.FirstAlternateAllele;

            var variantBegin = firstAltAllele.NirvanaVariantType == VariantType.insertion
                ? firstAltAllele.End
                : firstAltAllele.Start;
            var variantEnd = firstAltAllele.End;

            _suppIntervalForest.GetAllOverlappingValues(variant.ReferenceIndex, variantBegin, variantEnd,
                _overlappingSupplementaryIntervals);

            variant.AddSupplementaryIntervals(_overlappingSupplementaryIntervals);
        }

        public void Load(string ucscReferenceName, IChromosomeRenamer renamer) {}
    }
}
