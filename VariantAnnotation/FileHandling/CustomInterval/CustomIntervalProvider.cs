using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling.CustomInterval
{
    public class CustomIntervalProvider : ISupplementaryAnnotationProvider
    {
        private string _currentUcscReferenceName;
        private readonly List<string> _ciDirs;
        private bool _hasIntervals;
        private readonly List<ICustomInterval> _overlappingIntervals;
        private IIntervalForest<ICustomInterval> _intervalForest;

        public GenomeAssembly GenomeAssembly => SupplementaryAnnotationCommon.GetGenomeAssembly(_ciDirs.FirstOrDefault());

        public IEnumerable<IDataSourceVersion> DataSourceVersions
            => SupplementaryAnnotationCommon.GetDataSourceVersions(_ciDirs.FirstOrDefault());

        /// <summary>
        /// constructor
        /// </summary>
        public CustomIntervalProvider(IEnumerable<string> ciDirs)
        {
            _ciDirs               = ciDirs.ToList();
            _overlappingIntervals = new List<ICustomInterval>();
            _intervalForest       = new NullIntervalSearch<ICustomInterval>();
        }

        public void AddAnnotation(IVariantFeature variant)
        {
            if (!_hasIntervals) return;

            _intervalForest.GetAllOverlappingValues(variant.ReferenceIndex, variant.OverlapReferenceBegin,
                variant.OverlapReferenceEnd, _overlappingIntervals);
            if (_overlappingIntervals.Count == 0) return;

            variant.AddCustomIntervals(_overlappingIntervals);
        }

        public void Load(string ucscReferenceName, IChromosomeRenamer renamer)
        {
            if (_ciDirs.Count == 0 || ucscReferenceName == _currentUcscReferenceName) return;

            var intervals = GetIntervals(ucscReferenceName);

            _intervalForest = IntervalArrayFactory.CreateIntervalArray(intervals, renamer);
            _hasIntervals = !(_intervalForest is NullIntervalSearch<ICustomInterval>);

            _currentUcscReferenceName = ucscReferenceName;
        }

        private IEnumerable<ICustomInterval> GetIntervals(string ucscReferenceName)
        {
            var intervals = new List<ICustomInterval>();
            foreach (var ciDir in _ciDirs) AddIntervals(ciDir, ucscReferenceName, intervals);
            return intervals;
        }

        private static void AddIntervals(string ciDir, string ucscReferenceName, List<ICustomInterval> intervals)
        {
            if (string.IsNullOrEmpty(ciDir)) return;

            var ciPath = Path.Combine(ciDir, ucscReferenceName + ".nci");
            if (!File.Exists(ciPath)) return;

            using (var reader = new CustomIntervalReader(ciPath))
            {
                while (true)
                {
                    var interval = reader.GetNextCustomInterval();
                    if (interval == null) break;
                    intervals.Add(interval);
                }
            }
        }

        public void Clear()
        {
            _hasIntervals = false;
        }
    }
}
