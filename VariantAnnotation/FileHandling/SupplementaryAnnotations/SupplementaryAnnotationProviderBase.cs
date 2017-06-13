using System.Collections.Generic;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling.SupplementaryAnnotations
{
    public class SupplementaryAnnotationProviderBase
    {
        // supplementary annotation
        protected List<ISupplementaryAnnotationReader> SaReaders;

        // supplementary intervals
        protected bool HasSmallVariantIntervals;
        protected bool HasSvIntervals;
        protected bool HasAllVariantIntervals;
        private readonly List<IInterimInterval> _overlappingIntervals;
        protected IIntervalSearch<IInterimInterval> SmallVariantIntervalArray;
        protected IIntervalSearch<IInterimInterval> SvIntervalArray;
        protected IIntervalSearch<IInterimInterval> AllVariantIntervalArray;

        /// <summary>
        /// constructor
        /// </summary>
        protected SupplementaryAnnotationProviderBase()
        {
            _overlappingIntervals     = new List<IInterimInterval>();
            SmallVariantIntervalArray = new NullIntervalSearch<IInterimInterval>();
            SvIntervalArray           = new NullIntervalSearch<IInterimInterval>();
            AllVariantIntervalArray   = new NullIntervalSearch<IInterimInterval>();
        }

        protected void BuildIntervalForests()
        {
            if (SaReaders == null || SaReaders.Count == 0) return;

            var smallVariantIntervals = new List<Interval<IInterimInterval>>();
            var svIntervals           = new List<Interval<IInterimInterval>>();
            var allVariantIntervals   = new List<Interval<IInterimInterval>>();

            foreach (var reader in SaReaders)
            {
                if (reader.SmallVariantIntervals != null) smallVariantIntervals.AddRange(reader.SmallVariantIntervals);
                if (reader.SvIntervals           != null) svIntervals.AddRange(reader.SvIntervals);
                if (reader.AllVariantIntervals   != null) allVariantIntervals.AddRange(reader.AllVariantIntervals);
            }

            SmallVariantIntervalArray = CreateIntervalArray(smallVariantIntervals);
            SvIntervalArray           = CreateIntervalArray(svIntervals);
            AllVariantIntervalArray   = CreateIntervalArray(allVariantIntervals);
        }

        private IIntervalSearch<IInterimInterval> CreateIntervalArray(List<Interval<IInterimInterval>> intervals)
        {
            if (intervals.Count == 0) return new NullIntervalSearch<IInterimInterval>();
            return new IntervalArray<IInterimInterval>(intervals.ToArray());
        }

        public void AddAnnotation(IVariantFeature variant)
        {
            if (SaReaders == null || SaReaders.Count == 0) return;

	        if (variant.IsRepeatExpansion) return;

			AddSupplementaryIntervals(variant);

			
            if (variant.IsStructuralVariant) return;

            foreach (var saReader in SaReaders)
            {
                variant.SetSupplementaryAnnotation(saReader);
            }
        }

        private void AddSupplementaryIntervals(IVariantFeature variant)
        {
            if (!HasSmallVariantIntervals && !HasSvIntervals && !HasAllVariantIntervals) return;

            var firstAltAllele = variant.FirstAlternateAllele;

            var begin = firstAltAllele.NirvanaVariantType == VariantType.insertion
                ? firstAltAllele.End
                : firstAltAllele.Start;
            var end = firstAltAllele.End;

            if (!variant.IsStructuralVariant)
            {
                if (HasSmallVariantIntervals) AddIntervals(variant, SmallVariantIntervalArray, begin, end);
            }
            else
            {
                if (HasSvIntervals) AddIntervals(variant, SvIntervalArray, begin, end);
            }

            if (HasAllVariantIntervals) AddIntervals(variant, AllVariantIntervalArray, begin, end);
        }

        private void AddIntervals(IVariantFeature variant, IIntervalSearch<IInterimInterval> intervalArray, int begin,
            int end)
        {
            intervalArray.GetAllOverlappingValues(begin, end, _overlappingIntervals);
            variant.AddSupplementaryIntervals(_overlappingIntervals);
        }
    }
}
