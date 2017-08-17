using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class MappedPositions : IMappedPositions
    {
        public NullableInterval CdnaInterval { get; }
        public NullableInterval CdsInterval { get; }
        public IInterval ImpactedCdnaInterval { get; }
        public IInterval ImpactedCdsInterval { get; }
        public NullableInterval ProteinInterval { get; }
        public IInterval Exons { get; }
        public IInterval Introns { get; }


        public MappedPositions(NullableInterval cdnaInterval,
            IInterval impactedCdnaInterval, NullableInterval cdsInterval, IInterval impactedCdsInterval,
            NullableInterval proteinInterval,
            IInterval exons, IInterval introns)
        {
            CdnaInterval = cdnaInterval;
            ImpactedCdnaInterval = impactedCdnaInterval;
            CdsInterval = cdsInterval;
            ImpactedCdsInterval = impactedCdsInterval;
            ProteinInterval = proteinInterval;
            Exons = exons;
            Introns = introns;
        }


        public sealed class Coordinate
        {
            public bool IsGap;
            public int Start;
            public int End;

            /// <summary>
            /// constructor
            /// </summary>
            public Coordinate(int start, int end, bool isGap)
            {
                Start = start;
                End = end;
                IsGap = isGap;
            }
        }
    }
}