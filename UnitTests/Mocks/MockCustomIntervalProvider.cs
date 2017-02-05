using System.Collections.Generic;
using System.IO;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.FileHandling.CustomInterval;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace UnitTests.Mocks
{
    public class MockCustomIntervalProvider : ISupplementaryAnnotationProvider
    {
        private bool _hasIntervals;
        private readonly List<ICustomInterval> _overlappingIntervals = new List<ICustomInterval>();
        private readonly IIntervalForest<ICustomInterval> _intervalForest;
        public GenomeAssembly GenomeAssembly => GenomeAssembly.Unknown;
        public IEnumerable<IDataSourceVersion> DataSourceVersions => new List<IDataSourceVersion>();

        public void Clear() => _hasIntervals = false;

        /// <summary>
        /// constructor
        /// </summary>
        public MockCustomIntervalProvider(List<ICustomInterval> intervals, ChromosomeRenamer renamer)
        {
            _hasIntervals   = intervals.Count > 0;
            _intervalForest = IntervalArrayFactory.CreateIntervalArray(intervals, renamer);
        }

        public MockCustomIntervalProvider(Stream stream, ChromosomeRenamer renamer)
        {
            var intervals = new List<ICustomInterval>();

            using (var reader = new CustomIntervalReader(stream))
            {
                while (true)
                {
                    var interval = reader.GetNextCustomInterval();
                    if (interval == null) break;
                    intervals.Add(interval);
                }
            }

            _hasIntervals   = intervals.Count > 0;
            _intervalForest = IntervalArrayFactory.CreateIntervalArray(intervals, renamer);
        }

        public void AddAnnotation(IVariantFeature variant)
        {
            if (!_hasIntervals) return;

            _intervalForest.GetAllOverlappingValues(variant.ReferenceIndex, variant.OverlapReferenceBegin,
                variant.OverlapReferenceEnd, _overlappingIntervals);
            if (_overlappingIntervals.Count == 0) return;

            variant.AddCustomIntervals(_overlappingIntervals);
        }

        public void Load(string ucscReferenceName, IChromosomeRenamer renamer) { }
    }
}
