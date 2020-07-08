using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class FeatureVariantEffects : IFeatureVariantEffects
    {
        private readonly bool _isSv;

        private readonly bool _completelyOverlaps;
        private readonly bool _partialOverlap;
        private readonly bool _fivePrimeOverlap;
        private readonly bool _threePrimeOverlap;
        private readonly bool _completelyWithin;

        private readonly bool _lossOrDeletion;
        private readonly bool _gainOrDuplication;
        private readonly bool _isInsertionDeletion;
        private readonly bool _isInsertion;

        public FeatureVariantEffects(OverlapType overlapType, EndpointOverlapType endpointOverlapType, bool onReverseStrand, VariantType vt,
                                     bool        isSv)
        {
            _isSv = isSv;

            _partialOverlap     = overlapType != OverlapType.CompletelyOverlaps && overlapType != OverlapType.None;
            _completelyOverlaps = overlapType == OverlapType.CompletelyOverlaps;
            _completelyWithin   = overlapType == OverlapType.CompletelyWithin;

            _fivePrimeOverlap = !onReverseStrand && endpointOverlapType == EndpointOverlapType.Start ||
                                onReverseStrand  && endpointOverlapType == EndpointOverlapType.End;

            _threePrimeOverlap = !onReverseStrand && endpointOverlapType == EndpointOverlapType.End ||
                                 onReverseStrand  && endpointOverlapType == EndpointOverlapType.Start;

            _lossOrDeletion = vt == VariantType.copy_number_loss || vt == VariantType.deletion;
            _gainOrDuplication = vt == VariantType.copy_number_gain || vt == VariantType.duplication ||
                                 vt == VariantType.tandem_duplication;

            _isInsertionDeletion = vt == VariantType.indel;
            _isInsertion         = vt == VariantType.insertion;
        }

        public bool Ablation()      => (_lossOrDeletion || _isInsertionDeletion) && _completelyOverlaps;
        public bool Amplification() => _gainOrDuplication && _completelyOverlaps;
        public bool Truncation()    => _isSv && _lossOrDeletion && _partialOverlap;
        public bool Elongation()    => _isSv && _completelyWithin && (_gainOrDuplication || _isInsertion);

        public bool FivePrimeDuplicatedTranscript()  => _gainOrDuplication && _fivePrimeOverlap;
        public bool ThreePrimeDuplicatedTranscript() => _gainOrDuplication && _threePrimeOverlap;
    }
}