using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class FeatureVariantEffects : IFeatureVariantEffects
    {
        private readonly bool _isSv;

        private readonly bool _completelyOverlaps;
        private readonly bool _overlaps;
        private readonly bool _completelyWithin;

        private readonly bool _lossOrDeletion;
        private readonly bool _gainOrDuplication;
        private readonly bool _isInsertionDeletion;
        private readonly bool _isInsertion;

        public FeatureVariantEffects(IInterval feature, VariantType vt, IInterval variant, bool isSv)
        {
            _isSv = isSv;

            _completelyOverlaps = IntervalUtilities.Contains(variant.Start, variant.End, feature.Start, feature.End);
            _overlaps           = feature.Overlaps(variant);
            _completelyWithin   = variant.Start >= feature.Start && variant.End <= feature.End;

            _lossOrDeletion      = vt == VariantType.copy_number_loss || vt == VariantType.deletion || vt == VariantType.copy_number_loss;
            _gainOrDuplication   = vt == VariantType.copy_number_gain || vt == VariantType.duplication || vt == VariantType.tandem_duplication || vt == VariantType.copy_number_gain;
            _isInsertionDeletion = vt == VariantType.indel;
            _isInsertion         = vt == VariantType.insertion;
        }

        public bool Ablation()      => (_lossOrDeletion || _isInsertionDeletion) && _completelyOverlaps;
        public bool Amplification() => _gainOrDuplication && _completelyOverlaps;
        public bool Truncation()    => _isSv && _lossOrDeletion && _overlaps && !_completelyOverlaps;
        public bool Elongation()    => _isSv && _completelyWithin && (_gainOrDuplication || _isInsertion);
    }
}