using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Providers
{
	public sealed class SupplementaryAnnotationProvider : IAnnotationProvider
	{
		public string Name { get; }
		public GenomeAssembly GenomeAssembly => SaReaderUtils.GetGenomeAssembly(_saDirs);

		public IEnumerable<IDataSourceVersion> DataSourceVersions =>
			SaReaderUtils.GetDataSourceVersions(_saDirs);


	    private List<ISupplementaryAnnotationReader> _saReaders;
	    private string _currentUcscReferenceName;
		private bool _hasSmallVariantIntervals;
		private bool _hasSvIntervals;
		private bool _hasAllVariantIntervals;

		private IIntervalSearch<ISupplementaryInterval> _smallVariantIntervalArray;
		private IIntervalSearch<ISupplementaryInterval> _svIntervalArray;
		private IIntervalSearch<ISupplementaryInterval> _allVariantIntervalArray;

		private readonly List<string> _saDirs;

		public SupplementaryAnnotationProvider(List<string> saDirs)
		{
			Name = "Supplementary annotation provider";
			_currentUcscReferenceName = "";

			_smallVariantIntervalArray = new NullIntervalSearch<ISupplementaryInterval>();
			_svIntervalArray = new NullIntervalSearch<ISupplementaryInterval>();
			_allVariantIntervalArray = new NullIntervalSearch<ISupplementaryInterval>();
			_saDirs = saDirs.ToList();

		}



		public void Annotate(IAnnotatedPosition annotatedPosition)
		{
			LoadChromosome(annotatedPosition.Position.Chromosome);
			if (_saReaders == null || _saReaders.Count == 0) return;

			AddSupplementaryIntervals(annotatedPosition);


			foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
			{
				if (!annotatedVariant.Variant.Behavior.NeedSaPosition) continue;
				foreach (var saReader in _saReaders)
				{
					var saPosition = saReader.GetAnnotation(annotatedVariant.Variant.Start);
					if (saPosition != null) AddSaPositon(saPosition, annotatedVariant);
				}

			}
		}

		private static void AddSaPositon(ISaPosition saPosition, IAnnotatedVariant annotatedVariant)
		{
			foreach (var dataSource in saPosition.DataSources)
			{
				var saAltAllele = SaReaderUtils.GetReducedAllele(annotatedVariant.Variant.RefAllele,
					annotatedVariant.Variant.AltAllele);
				if (dataSource.MatchByAllele && dataSource.AltAllele != saAltAllele) continue;
				annotatedVariant.SupplementaryAnnotations.Add(new AnnotatedSaDataSource(dataSource, saAltAllele));
			}
		}

		private void AddSupplementaryIntervals(IAnnotatedPosition annotatedPosition)
		{
			if (!_hasSmallVariantIntervals && !_hasSvIntervals && !_hasAllVariantIntervals) return;

			var firstAltAllele = annotatedPosition.AnnotatedVariants[0].Variant;

			var begin = firstAltAllele.Type == VariantType.insertion
				? firstAltAllele.End
				: firstAltAllele.Start;
			var end = firstAltAllele.End;

			if (firstAltAllele.Behavior.NeedSaInterval)
			{
				if (_hasSmallVariantIntervals) AddIntervals(annotatedPosition, _smallVariantIntervalArray, begin, end);
			}

			if (firstAltAllele.Behavior.NeedSaInterval)
			{
				if (_hasSvIntervals) AddIntervals(annotatedPosition, _svIntervalArray, begin, end);
			}

			if (_hasAllVariantIntervals) AddIntervals(annotatedPosition, _allVariantIntervalArray, begin, end);
		}

	    private static void AddIntervals(IAnnotatedPosition annotatedPosition,
	        IIntervalSearch<ISupplementaryInterval> intervalArray, int begin, int end)
	    {
			var intervals = intervalArray.GetAllOverlappingValues(begin, end);
		    if (intervals == null) return;

			foreach (var overlappingInterval in intervals)
			{
			    var reciprocalOverlap = annotatedPosition.Position.Start >= annotatedPosition.Position.End
			        ? null
			        : overlappingInterval.GetReciprocalOverlap(annotatedPosition.AnnotatedVariants[0].Variant);

                annotatedPosition.SupplementaryIntervals.Add(
					new AnnotatedSupplementaryInterval(overlappingInterval, reciprocalOverlap));
			}
		}

		private void LoadChromosome(IChromosome chromosome)
		{
			var ucscReferenceName = chromosome.UcscName;

			if (_saDirs == null || _saDirs.Count == 0 || ucscReferenceName == _currentUcscReferenceName) return;

			_saReaders = SaReaderUtils.GetReaders(_saDirs, ucscReferenceName);

			BuildIntervalForests();

			_hasSmallVariantIntervals = !(_smallVariantIntervalArray is NullIntervalSearch<ISupplementaryInterval>);
			_hasSvIntervals = !(_svIntervalArray is NullIntervalSearch<ISupplementaryInterval>);
			_hasAllVariantIntervals = !(_allVariantIntervalArray is NullIntervalSearch<ISupplementaryInterval>);

			_currentUcscReferenceName = ucscReferenceName;
		}

		private void BuildIntervalForests()
		{
			if (_saReaders == null || _saReaders.Count == 0) return;

			var smallVariantIntervals = new List<Interval<ISupplementaryInterval>>();
			var svIntervals = new List<Interval<ISupplementaryInterval>>();
			var allVariantIntervals = new List<Interval<ISupplementaryInterval>>();

			foreach (var reader in _saReaders)
			{
				if (reader.SmallVariantIntervals != null) smallVariantIntervals.AddRange(reader.SmallVariantIntervals);
				if (reader.SvIntervals != null) svIntervals.AddRange(reader.SvIntervals);
				if (reader.AllVariantIntervals != null) allVariantIntervals.AddRange(reader.AllVariantIntervals);
			}

			_smallVariantIntervalArray = CreateIntervalArray(smallVariantIntervals);
			_svIntervalArray = CreateIntervalArray(svIntervals);
			_allVariantIntervalArray = CreateIntervalArray(allVariantIntervals);
		}

		private static IIntervalSearch<ISupplementaryInterval> CreateIntervalArray(List<Interval<ISupplementaryInterval>> intervals)
		{
			if (intervals.Count == 0) return new NullIntervalSearch<ISupplementaryInterval>();
			return new IntervalArray<ISupplementaryInterval>(intervals.ToArray());
		}
	}
}