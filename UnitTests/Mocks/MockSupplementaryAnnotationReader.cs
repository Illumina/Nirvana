using System;
using System.Collections.Generic;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.Interface;

namespace UnitTests.Mocks
{
    public class MockSupplementaryAnnotationReader : ISupplementaryAnnotationReader
    {
        private readonly ISupplementaryAnnotationPosition _saPosition;
        private readonly int _referencePosition;
        private readonly bool _isRefMinorAllele;

        /// <summary>
        /// constructor
        /// </summary>
        public MockSupplementaryAnnotationReader(SupplementaryAnnotationPosition saPosition)
        {
            _saPosition        = saPosition;
            _referencePosition = saPosition.ReferencePosition;
            _isRefMinorAllele  = saPosition.IsRefMinorAllele;
        }
        
        public bool IsRefMinor(int position)
        {
            return position == _referencePosition && _isRefMinorAllele;
        }

        public ISupplementaryAnnotationPosition GetAnnotation(int referencePos)
        {
            return referencePos == _referencePosition ? _saPosition : null;
        }

        public IIntervalForest<ISupplementaryInterval> GetIntervalForest(IChromosomeRenamer renamer)
        {
            return new NullIntervalSearch<ISupplementaryInterval>();
        }

        public IEnumerable<ISupplementaryInterval> GetSupplementaryIntervals(IChromosomeRenamer renamer)
        {
            throw new NotImplementedException();
        }
    }
}
