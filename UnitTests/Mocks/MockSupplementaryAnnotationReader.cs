using System.Collections.Generic;
using VariantAnnotation.Interface;

namespace UnitTests.Mocks
{
    public class MockSupplementaryAnnotationReader : ISupplementaryAnnotationReader
    {
        private readonly ISaPosition _saPosition;
        private readonly int _position;
        private readonly bool _isRefMinorAllele;

        // ReSharper disable UnassignedGetOnlyAutoProperty
        public ISupplementaryAnnotationHeader Header { get; }        
        public IEnumerable<Interval<IInterimInterval>> SmallVariantIntervals { get; }
        public IEnumerable<Interval<IInterimInterval>> SvIntervals { get; }
        public IEnumerable<Interval<IInterimInterval>> AllVariantIntervals { get; }
        // ReSharper restore UnassignedGetOnlyAutoProperty

        /// <summary>
        /// constructor
        /// </summary>
        public MockSupplementaryAnnotationReader(ISaPosition saPosition, int position, bool isRefMinor)
        {
            _saPosition       = saPosition;
            _position         = position;
            _isRefMinorAllele = isRefMinor;
        }

        public bool IsRefMinor(int position)
        {
            return position == _position && _isRefMinorAllele;
        }

        public ISaPosition GetAnnotation(int referencePos)
        {
            return referencePos == _position ? _saPosition : null;
        }
    }
}
