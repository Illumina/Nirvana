using VariantAnnotation.DataStructures;
using VariantAnnotation.Interface;

namespace VariantAnnotation.Algorithms.Consequences
{
    public class FeatureVariantEffects
    {
        #region members

        private readonly bool _isSV;

        private readonly bool _completelyOverlaps;
        private readonly bool _overlaps;
        private readonly bool _completelyWithin;

        private readonly bool _lossOrDeletion;
        private readonly bool _gainOrDuplication;
	    private readonly bool _isInsertionDeletion;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public FeatureVariantEffects(AnnotationInterval feature, VariantType vt, int refBegin, int refEnd, bool isSV,VariantType internalCopyNumberType)
        {
            _isSV = isSV;

            _completelyOverlaps = feature.CompletelyOverlaps(refBegin, refEnd);
            _overlaps = feature.Overlaps(refBegin, refEnd);
            _completelyWithin = (refBegin >= feature.Start) && (refEnd <= feature.End);

            _lossOrDeletion = (vt == VariantType.copy_number_loss) || (vt == VariantType.deletion) ||(internalCopyNumberType==VariantType.copy_number_loss);
            _gainOrDuplication = (vt == VariantType.copy_number_gain) || (vt == VariantType.duplication) ||
                                 (vt == VariantType.tandem_duplication) ||(internalCopyNumberType == VariantType.copy_number_gain);
	        _isInsertionDeletion = vt == VariantType.indel;
        }

        /// <summary>
        /// returns true if the variant ablates the feature [VariationEffect.pm:262 feature_ablation]
        /// </summary>
        public bool Ablation() 
        {
            return (_lossOrDeletion||_isInsertionDeletion) && _completelyOverlaps;
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
            return _isSV && _lossOrDeletion && _overlaps && !_completelyOverlaps;
        }

        /// <summary>
        /// returns true if the variant elongates the feature [VariationEffect.pm:276 feature_elongation]
        /// </summary>
        public bool Elongation()
        {
            return _isSV && _completelyWithin && _gainOrDuplication;
        }
    }
}
