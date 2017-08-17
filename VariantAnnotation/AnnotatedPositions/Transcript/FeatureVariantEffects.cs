using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class FeatureVariantEffects:IFeatureVariantEffects
    {
        #region members

        private readonly bool _isSv;

        private readonly bool _completelyOverlaps;
        private readonly bool _overlaps;
        private readonly bool _completelyWithin;

        private readonly bool _lossOrDeletion;
        private readonly bool _gainOrDuplication;
        private readonly bool _isInsertionDeletion;
        private readonly bool _isInsertion;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public FeatureVariantEffects(IInterval feature, VariantType vt, int refBegin, int refEnd, bool isSv)
        {
            _isSv = isSv;

            _completelyOverlaps = IntervalUtilities.Contains(refBegin, refEnd, feature.Start, feature.End);
            _overlaps           = feature.Overlaps(refBegin, refEnd);
            _completelyWithin   = refBegin >= feature.Start && refEnd <= feature.End;

            _lossOrDeletion = vt == VariantType.copy_number_loss || vt == VariantType.deletion || vt == VariantType.copy_number_loss;
            _gainOrDuplication = vt == VariantType.copy_number_gain || vt == VariantType.duplication ||
                                 vt == VariantType.tandem_duplication || vt == VariantType.copy_number_gain;
            _isInsertionDeletion = vt == VariantType.indel;
            _isInsertion = vt == VariantType.insertion;

        }

        /// <summary>
        /// returns true if the variant ablates the feature [VariationEffect.pm:262 feature_ablation]
        /// </summary>
        public bool Ablation()
        {
            return (_lossOrDeletion || _isInsertionDeletion) && _completelyOverlaps;
        }

        /// <summary>
        /// returns true if the variant amplifies the feature [VariationEffect.pm:269 feature_amplification]
        /// </summary>
        public bool Amplification()
        {
            return _gainOrDuplication && _completelyOverlaps;
        }

        /// <summary>
        /// returns true if the variant truncates the feature [VariationEffect.pm:288 feature_truncation]
        /// </summary>
        public bool Truncation()
        {
            return _isSv && _lossOrDeletion && _overlaps && !_completelyOverlaps;
        }

        /// <summary>
        /// returns true if the variant elongates the feature [VariationEffect.pm:276 feature_elongation]
        /// </summary>
        public bool Elongation()
        {
            return _isSv && _completelyWithin && (_gainOrDuplication || _isInsertion);
        }
    }
}