using System;
using OptimizedCore;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    /// <summary>
    /// This class performs all of the functional consequence testing. An additional caching layer
    /// has been added to prevent unneeded calculations. The caching layer is reset when each new
    /// variant has been read.
    /// </summary>
    public sealed class VariantEffect : IVariantEffect
    {
        private readonly TranscriptPositionalEffect _preCache;

        private readonly ITranscript _transcript;
        private readonly ISimpleVariant _variant;

        private readonly VariantEffectCache _cache;

        private readonly string _referenceAminoAcids;
        private readonly string _alternateAminoAcids;

        private readonly int _referenceAminoAcidsLen;
        private readonly int _alternateAminoAcidsLen;

        private readonly string _coveredReferenceAminoAcids;
        private readonly string _coveredAlternateAminoAcids;

        private readonly string _referenceCodons;
        private readonly string _alternateCodons;

        private readonly int _referenceCodonsLen;
        private readonly int _alternateCodonsLen;

        private readonly bool _isInsertion;
        private readonly bool _isDeletion;

        private readonly int _proteinBegin;

        public VariantEffect(TranscriptPositionalEffect transcriptEffect, ISimpleVariant variant, ITranscript transcript,
            string referenAminoAcids, string alternateAminoAcids, string referenceCodons, string alternateCodons,
            int? proteinBegin, string coveredReferenceAminoAcids, string coveredAlternateAminoAcids, VariantEffectCache cache = null)
        {
            _transcript = transcript;
            _variant    = variant;

            _preCache = transcriptEffect;

            _cache = cache ?? new VariantEffectCache();

            _referenceAminoAcids    = referenAminoAcids;
            _alternateAminoAcids    = alternateAminoAcids;
            _referenceAminoAcidsLen = _referenceAminoAcids?.Length ?? 0;
            _alternateAminoAcidsLen = _alternateAminoAcids?.Length ?? 0;

            _coveredReferenceAminoAcids = coveredReferenceAminoAcids;
            _coveredAlternateAminoAcids = coveredAlternateAminoAcids;

            _referenceCodons        = referenceCodons;
            _alternateCodons        = alternateCodons;
            _referenceCodonsLen     = _referenceCodons?.Length ?? 0;
            _alternateCodonsLen     = _alternateCodons?.Length ?? 0;

            _isInsertion = variant.AltAllele.Length > variant.RefAllele.Length;
            _isDeletion  = variant.AltAllele.Length < variant.RefAllele.Length;

            _proteinBegin = proteinBegin ?? -1;
        }

        /// <summary>
        /// returns true if the variant is a splice acceptor variant [VariationEffect.pm:404 acceptor_splice_site]
        /// </summary>
        public bool IsSpliceAcceptorVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.splice_acceptor_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = _transcript.Gene.OnReverseStrand ? _preCache.IsStartSpliceSite : _preCache.IsEndSpliceSite;

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a splice donor variant [VariationEffect.pm:459 donor_splice_site]
        /// </summary>
        public bool IsSpliceDonorVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.splice_donor_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = _transcript.Gene.OnReverseStrand ? _preCache.IsEndSpliceSite : _preCache.IsStartSpliceSite;

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a 5' UTR variant (VariationEffect.pm:595 within_5_prime_utr)
        /// </summary>
        public bool IsFivePrimeUtrVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.five_prime_UTR_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            if (_transcript.Translation != null)
            {

                var isFivePrimeOfCoding = _transcript.Gene.OnReverseStrand
                    ? _preCache.AfterCoding
                    : _preCache.BeforeCoding;

                result = isFivePrimeOfCoding && _preCache.WithinCdna;
            }

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a frameshift variant [VariantEffect.pm:940 frameshift]
        /// </summary>
        public bool IsFrameshiftVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.frameshift_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            // check the predicates
            if (!_preCache.IsCoding)
            {
                _cache.Add(ct, false);
                return false;
            }

            if (IsIncompleteTerminalCodonVariant())
            {
                _cache.Add(ct, false);
                return false;
            }

            bool result = _preCache.HasFrameShift && !IsStopRetained() && !IsTruncatedByStop();

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if we have an incomplete terminal codon variant. [VariantEffect.pm:983 partial_codon]
        /// </summary>
        public bool IsIncompleteTerminalCodonVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.incomplete_terminal_codon_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            if (_transcript.Translation == null)
            {
                _cache.Add(ct, false);
                return false;
            }

            int cdsLength       = _transcript.Translation.CodingRegion.Length;
            int codonCdsStart   = _proteinBegin * 3 - 2;
            int lastCodonLength = cdsLength - (codonCdsStart - 1);

            bool result = lastCodonLength < 3 && lastCodonLength > 0;

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is an inframe deletion [VariantEffect.pm:825 inframe_deletion]
        /// </summary>
        public bool IsInframeDeletion()
        {
            const ConsequenceTag ct = ConsequenceTag.inframe_deletion;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            // check the predicates
            if (!_preCache.IsCoding || !_isDeletion)
            {
                _cache.Add(ct, false);
                return false;
            }

            if (_referenceCodonsLen == 0 //|| (PreCache.ReferenceCodonLen < PreCache.AlternateCodonLen) 
                || IsFrameshiftVariant()
                || IsIncompleteTerminalCodonVariant()
                || IsStopGained())
            {
                _cache.Add(ct, false);
                return false;
            }

            // simple string match
            var referenceCodon = _referenceCodons.ToLower();
            var alternateCodon = _alternateCodons.ToLower();

            if (referenceCodon.StartsWith(alternateCodon) || referenceCodon.EndsWith(alternateCodon))
            {
                _cache.Add(ct, true);
                return true;
            }

            // try a more complex string match
            var commonPrefixLength = _referenceCodons.CommonPrefixLength(_alternateCodons);
            var commonSuffixLength = _referenceCodons.CommonSuffixLength(_alternateCodons);

            bool result = _alternateCodonsLen - commonPrefixLength - commonSuffixLength == 0;

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is an inframe insertion [VariantEffect.pm:780 inframe_insertion]
        /// </summary>
        public bool IsInframeInsertion()
        {
            const ConsequenceTag ct = ConsequenceTag.inframe_insertion;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            // check the predicates
            if (!_preCache.IsCoding || !_isInsertion)
            {
                _cache.Add(ct, false);
                return false;
            }

            if (IsStopRetained() ||
                IsFrameshiftVariant() ||
                IsStartLost() ||
                _alternateCodonsLen <= _referenceCodonsLen ||
                IsIncompleteTerminalCodonVariant())
            {
                _cache.Add(ct, false);
                return false;
            }

            bool result = !IsTruncatedByStop();

            _cache.Add(ct, result);
            return result;
        }

        private bool IsTruncatedByStop()
        {
            if (_alternateAminoAcids != null && _alternateAminoAcids.Contains(AminoAcids.StopCodon))
            {
                var stopPos = _alternateAminoAcids.IndexOf(AminoAcids.StopCodon, StringComparison.Ordinal);
                var altAminoAcidesBeforeStop = _alternateAminoAcids.Substring(0, stopPos);
                if (_alternateAminoAcids.OptimizedStartsWith(AminoAcids.StopCodonChar) ||
                    _referenceAminoAcids.StartsWith(altAminoAcidesBeforeStop))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// returns true if at least one base of the first codon was changed in the transcript [VariantEffect.pm:722 affects_start_codon]
        /// </summary>
        public bool IsStartLost()
        {
            const ConsequenceTag ct = ConsequenceTag.start_lost;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            // check the predicates
            if (!_preCache.IsCoding)
            {
                _cache.Add(ct, false);
                return false;
            }

            if (_proteinBegin != 1 || _referenceAminoAcidsLen == 0)
            {
                _cache.Add(ct, false);
                return false;
            }

            // insertion in start codon and do not change start codon
            if (_isInsertion && _proteinBegin == 1 && _alternateAminoAcids.EndsWith(_referenceAminoAcids))
            {
                _cache.Add(ct, false);
                return false;
            }

            bool result = _alternateAminoAcidsLen == 0 || _alternateAminoAcids[0] != _referenceAminoAcids[0];

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a missense variant [VariantEffect.pm:682 missense_variant]
        /// </summary>
        public bool IsMissenseVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.missense_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            // check the predicates
            if (!_preCache.IsCoding)
            {
                _cache.Add(ct, false);
                return false;
            }

            if (IsStartLost() ||
                IsStopLost() ||
                IsStopGained() ||
                IsIncompleteTerminalCodonVariant() ||
                IsFrameshiftVariant() ||
                IsInframeDeletion() ||
                IsInframeInsertion())
            {
                _cache.Add(ct, false);
                return false;
            }

            bool result = _referenceAminoAcids != _alternateAminoAcids &&
                _referenceAminoAcidsLen == _alternateAminoAcidsLen;

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a non-coding transcript exon variant [VariationEffect.pm:405 non_coding_exon_variant]
        /// </summary>
        public bool IsNonCodingTranscriptExonVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.non_coding_transcript_exon_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = _preCache.HasExonOverlap && _transcript.Translation == null && !_preCache.OverlapWithMicroRna;

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a nonsense-mediated decay transcript variant [VariationEffect.pm:391 within_nmd_transcript]
        /// </summary>
        public bool IsNonsenseMediatedDecayTranscriptVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.NMD_transcript_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);
            var result = _transcript.BioType == BioType.nonsense_mediated_decay;
            _cache.Add(ct, result);
            return result;
        }
        

        /// <summary>
        /// returns true if the variant is a protein altering variant [VariationEffect.pm:300 protein_altering_variant]
        /// </summary>
        public bool IsProteinAlteringVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.protein_altering_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            var result = true;

            var sameLen = _referenceAminoAcidsLen == _alternateAminoAcidsLen;
            var startsWithTer = _referenceAminoAcids.OptimizedStartsWith('X') || _alternateAminoAcids.OptimizedStartsWith('X');

            var isInframeDeletion = IsInframeDeletion();
            // Note: sequence ontology says that stop retained should not be here (http://www.sequenceontology.org/browser/current_svn/term/SO:0001567)
            var isStopCodonVarinat = IsStopLost() || IsStopGained();

            if (sameLen || startsWithTer || isInframeDeletion || isStopCodonVarinat ||
                IsStartLost() || IsFrameshiftVariant() || IsInframeInsertion() || IsStopRetained() || !_preCache.IsCoding)
            {
                result = false;
            }

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a splice region variant [VariationEffect.pm:483 splice_region]
        /// </summary>
        public bool IsSpliceRegionVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.splice_region_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            if (IsSpliceDonorVariant() || IsSpliceAcceptorVariant())
            {
                // false
            }
            else
            {
                result = _preCache.IsWithinSpliceSiteRegion;
            }

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant's amino acid changes to a stop codon [VariationEffect.pm:884 stop_gained]
        /// </summary>
        public bool IsStopGained()
        {
            const ConsequenceTag ct = ConsequenceTag.stop_gained;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = !IsStopRetained() &&
                     (string.IsNullOrEmpty(_referenceAminoAcids) || !_referenceAminoAcids.Contains(AminoAcids.StopCodon)) &&
                          !string.IsNullOrEmpty(_alternateAminoAcids) && _alternateAminoAcids.Contains(AminoAcids.StopCodon);

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a stop lost variant [VariationEffect.pm:898 stop_lost]
        /// </summary>
        public bool IsStopLost()
        {
            const ConsequenceTag ct = ConsequenceTag.stop_lost;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;
            if (!string.IsNullOrEmpty(_coveredReferenceAminoAcids) && _coveredAlternateAminoAcids != null)
                result = _coveredReferenceAminoAcids.Contains(AminoAcids.StopCodon) &&
                         !_coveredAlternateAminoAcids.Contains(AminoAcids.StopCodon);

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a stop retained variant [VariationEffect.pm:701 stop_lost]
        /// </summary>
        public bool IsStopRetained()
        {
            const ConsequenceTag ct = ConsequenceTag.stop_retained_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            var alternateAminoAcids = TrimPeptides(_alternateAminoAcids);

            bool result = !string.IsNullOrEmpty(_referenceAminoAcids) && alternateAminoAcids != null &&
                     _referenceAminoAcids == alternateAminoAcids &&
                     _referenceAminoAcids.Contains(AminoAcids.StopCodon) ||
                     string.IsNullOrEmpty(_referenceAminoAcids) && alternateAminoAcids != null &&
                     _proteinBegin == _transcript.Translation?.PeptideSeq.Length + 1 &&
                     alternateAminoAcids == AminoAcids.StopCodon;

            _cache.Add(ct, result);
            return result;
        }

        public bool IsStartRetained()
        {
            const ConsequenceTag ct = ConsequenceTag.start_retained_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            if (_proteinBegin != 1 || string.IsNullOrEmpty(_referenceAminoAcids))
            {
                _cache.Add(ct, false);
                return false;
            }

            var startProtein = _referenceAminoAcids[0].ToString();
            var alternateAminoAcids = TrimPeptides(_alternateAminoAcids);

            var result = alternateAminoAcids != null
                          && alternateAminoAcids.Contains(startProtein);

            _cache.Add(ct, result);
            return result;
        }

        private static string TrimPeptides(string alternateAminoAcids)
        {
            if (string.IsNullOrEmpty(alternateAminoAcids)) return null;
            if (!alternateAminoAcids.Contains(AminoAcids.StopCodon)) return alternateAminoAcids;
            var pos = alternateAminoAcids.IndexOf(AminoAcids.StopCodon, StringComparison.Ordinal);
            return pos < 0 ? alternateAminoAcids : alternateAminoAcids.Substring(0, pos + 1);
        }

        /// <summary>
        /// returns true if the variant is a synonymous variant [VariationEffect.pm:755 synonymous_variant]
        /// </summary>
        public bool IsSynonymousVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.synonymous_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = !string.IsNullOrEmpty(_referenceAminoAcids)  &&
                     (_variant.Type == VariantType.SNV ||
                      _variant.Type == VariantType.MNV) &&
                     _referenceAminoAcids == _alternateAminoAcids && !_referenceAminoAcids.Contains("X") &&
                     !_alternateAminoAcids.Contains("X") && !IsStopRetained();

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a 3' UTR variant [VariationEffect.pm:609 within_3_prime_utr]
        /// </summary>
        public bool IsThreePrimeUtrVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.three_prime_UTR_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            if (_transcript.Translation != null)
            {
                var isThreePrimeOfCoding = _transcript.Gene.OnReverseStrand
                    ? _preCache.BeforeCoding
                    : _preCache.AfterCoding;

                result = isThreePrimeOfCoding && _preCache.WithinCdna;
            }

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is within a non-coding gene [VariationEffect.pm:398 within_non_coding_gene]
        /// </summary>
        public bool IsNonCodingTranscriptVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.non_coding_transcript_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            // NOTE: Isn't IsWithinTranscript always true? and not within mature miRNA is always true
            // For Ensembl transcript, miRNA may be a valid attribute. We have their location and we would like to check if the variant overlaps with the miRNA
            var result = !_preCache.HasExonOverlap && _transcript.Translation == null && !_preCache.OverlapWithMicroRna;

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if it's a coding sequnce variant [VariationEffect.pm:998 coding_unknown]
        /// </summary>
        public bool IsCodingSequenceVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.coding_sequence_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = _preCache.WithinCds &&
                     (string.IsNullOrEmpty(_transcript.Translation.PeptideSeq) ||
                      string.IsNullOrEmpty(_alternateAminoAcids) || _alternateAminoAcids.Contains("X"))
                     && !(IsFrameshiftVariant() || IsInframeDeletion() || IsIncompleteTerminalCodonVariant() ||
                          IsProteinAlteringVariant() || IsStopGained() || IsStopRetained() || IsStopLost());

            _cache.Add(ct, result);
            return result;
        }

        ///<summary>
        /// returns true if the variant occurs within an intron [VariationEffect.pm:494 within_intron]
        /// </summary>
        public bool IsWithinIntron() => _preCache.IsWithinIntron;

        /// <summary>
        /// returns true if the variant overlaps a mature MiRNA. [VariationEffect.pm:432 within_mature_miRNA]
        /// </summary>
        public bool IsMatureMirnaVariant()
        {
            const ConsequenceTag ct = ConsequenceTag.mature_miRNA_variant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = _preCache.OverlapWithMicroRna;

            _cache.Add(ct, result);
            return result;
        }
    }
}