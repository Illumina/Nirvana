using System;
using System.Linq;
using VariantAnnotation.DataStructures;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.Algorithms.Consequences
{
    /// <summary>
    /// This class performs all of the functional consequence testing. An additional caching layer
    /// has been added to prevent unneeded calculations. The caching layer is reset when each new
    /// variant has been read.
    /// </summary>
    public sealed class VariantEffect
    {
        #region members

        private readonly BasicVariantEffects _preCache;
        private readonly TranscriptAnnotation _ta;
        private readonly Transcript _transcript;
        private readonly VariantAlternateAllele _altAllele;

        private readonly VariantEffectCache _cache;
        private readonly TempVariantEffectCache _tempCache;

        private readonly FeatureVariantEffects _featureVariantEffects;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public VariantEffect(TranscriptAnnotation ta, Transcript transcript, VariantType internalCopyNumberType = VariantType.unknown)
        {
            _ta = ta;
            _transcript = transcript;
            _altAllele = ta.AlternateAllele;

            _preCache = new BasicVariantEffects(ta);

            _cache = new VariantEffectCache();
            _tempCache = new TempVariantEffectCache();

            _featureVariantEffects = new FeatureVariantEffects(transcript, _altAllele.NirvanaVariantType,
                _altAllele.Start, _altAllele.End, _altAllele.IsStructuralVariant, internalCopyNumberType);
        }

        /// <summary>
        /// returns the appropriate copy number consequence
        /// </summary>
        public ConsequenceType EvaluateCopyNumberConsequence(VariantType internalCopyNumberType)
        {

            if (internalCopyNumberType == VariantType.copy_number_loss) return ConsequenceType.CopyNumberDecrease;

            if (internalCopyNumberType == VariantType.copy_number_gain) return ConsequenceType.CopyNumberIncrease;

            if (internalCopyNumberType == VariantType.copy_number_variation) return ConsequenceType.CopyNumberChange;

            return ConsequenceType.Unknown;

        }

        /// <summary>
        /// returns true if the two intervals overlap (VariationEffect.pm:57)
        /// </summary>
        private static bool HasOverlap(int start1, int end1, int start2, int end2)
        {
            return end1 >= start2 && start1 <= end2;
        }



        /// <summary>
        /// returns true if the variant is a splice acceptor variant [VariationEffect.pm:404 acceptor_splice_site]
        /// </summary>
        public bool IsSpliceAcceptorVariant()
        {
            const ConsequenceType ct = ConsequenceType.SpliceAcceptorVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            if (!_altAllele.IsStructuralVariant)
            {
                result = _transcript.Gene.OnReverseStrand ? _ta.IsStartSpliceSite : _ta.IsEndSpliceSite;
            }

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant occurs before the coding start position [VariationEffect.pm:513 _after_coding]
        /// </summary>
        private bool IsAfterCoding(int variantRefBegin, int variantRefEnd, int transcriptEnd, int codingRegionEnd, bool hasTranslation)
        {
            const TempConsequenceType ct = TempConsequenceType.AfterCoding;
            if (_tempCache.Contains(ct)) return _tempCache.Get(ct);

            if (!hasTranslation)
            {
                _tempCache.Add(ct, false);
                return false;
            }

            // special case to handle insertions after the CDS end
            if (variantRefBegin == variantRefEnd + 1 && variantRefEnd == codingRegionEnd)
            {
                _tempCache.Add(ct, true);
                return true;
            }

            bool result = HasOverlap(variantRefBegin, variantRefEnd, codingRegionEnd + 1, transcriptEnd);

            _tempCache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant occurs before the coding start position [VariationEffect.pm:561 _before_coding]
        /// </summary>
        private bool IsBeforeCoding(int variantRefBegin, int variantRefEnd, int transcriptStart, int codingRegionStart, bool hasTranslation)
        {
            const TempConsequenceType ct = TempConsequenceType.BeforeCoding;
            if (_tempCache.Contains(ct)) return _tempCache.Get(ct);

            if (!hasTranslation)
            {
                _tempCache.Add(ct, false);
                return false;
            }

            // special case to handle insertions before the CDS start
            if (variantRefBegin == variantRefEnd + 1 && variantRefBegin == codingRegionStart)
            {
                _tempCache.Add(ct, true);
                return true;
            }

            bool result = HasOverlap(variantRefBegin, variantRefEnd, transcriptStart, codingRegionStart - 1);

            _tempCache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant overlaps a mature MiRNA. [VariationEffect.pm:432 within_mature_miRNA]
        /// </summary>
        public bool IsMatureMirnaVariant()
        {
            const ConsequenceType ct = ConsequenceType.MatureMirnaVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            if (_transcript.BioType == BioType.miRNA && !_altAllele.IsStructuralVariant)
            {
                if (_transcript.MicroRnas != null)
                    if (_transcript.MicroRnas.Any(microRna => microRna.Overlaps(_ta.ComplementaryDnaBegin, _ta.ComplementaryDnaEnd)))
                    {
                        result = true;
                    }
            }

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a splice donor variant [VariationEffect.pm:459 donor_splice_site]
        /// </summary>
        public bool IsSpliceDonorVariant()
        {
            const ConsequenceType ct = ConsequenceType.SpliceDonorVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;
            if (!_altAllele.IsStructuralVariant) result = _transcript.Gene.OnReverseStrand ? _ta.IsEndSpliceSite : _ta.IsStartSpliceSite;

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is an essential splice site [VariationEffect.pm:479 essential_splice_site]
        /// </summary>
        private bool IsEssentialSpliceSite()
        {
            const TempConsequenceType ct = TempConsequenceType.EssentialSpliceSite;
            if (_tempCache.Contains(ct)) return _tempCache.Get(ct);

            bool result = false;
            if (!_altAllele.IsStructuralVariant) result = IsSpliceAcceptorVariant() || IsSpliceDonorVariant();

            _tempCache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a 5' UTR variant (VariationEffect.pm:595 within_5_prime_utr)
        /// </summary>
        public bool IsFivePrimeUtrVariant()
        {
            const ConsequenceType ct = ConsequenceType.FivePrimeUtrVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            if (!_altAllele.IsStructuralVariant && _transcript.Translation != null)
            {
                var hasTranslation = _transcript.Translation != null;

                var isFivePrimeOfCoding = _transcript.Gene.OnReverseStrand
                    ? IsAfterCoding(_altAllele.Start, _altAllele.End, _transcript.End, _transcript.Translation.CodingRegion.GenomicEnd, hasTranslation)
                    : IsBeforeCoding(_altAllele.Start, _altAllele.End, _transcript.Start, _transcript.Translation.CodingRegion.GenomicStart, hasTranslation);

                result = isFivePrimeOfCoding && IsWithinCdna();
            }

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a frameshift variant [VariantEffect.pm:940 frameshift]
        /// </summary>
        public bool IsFrameshiftVariant()
        {
            const ConsequenceType ct = ConsequenceType.FrameshiftVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            // check the predicates
            if (!_preCache.IsCoding)
            {
                _cache.Add(ct, false);
                return false;
            }

            bool result = false;

            if (!_altAllele.IsStructuralVariant)
            {
                if (IsIncompleteTerminalCodonVariant() || !_ta.HasValidCdsStart || !_ta.HasValidCdsEnd)
                {
                    _cache.Add(ct, false);
                    return false;
                }

                int varLen = _ta.CodingDnaSequenceEnd - _ta.CodingDnaSequenceBegin + 1;
                int alleleLen = _altAllele.AlternateAllele?.Length ?? 0;

                result = !Codons.IsTriplet(alleleLen - varLen) && !IsStopRetained();
            }
            result = result && !IsTruncatedByStop();

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if we have an incomplete terminal codon variant. [VariantEffect.pm:983 partial_codon]
        /// </summary>
        public bool IsIncompleteTerminalCodonVariant()
        {
            const ConsequenceType ct = ConsequenceType.IncompleteTerminalCodonVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            if (_transcript.Translation == null || _altAllele.IsStructuralVariant)
            {
                _cache.Add(ct, false);
                return false;
            }

            int cdsLength = CodingSequence.GetCodingSequenceLength(_transcript.CdnaMaps,
                _transcript.Translation.CodingRegion.GenomicStart, _transcript.Translation.CodingRegion.GenomicEnd,
                _transcript.StartExonPhase);

            int codonCdsStart   = _ta.ProteinBegin * 3 - 2;
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
            const ConsequenceType ct = ConsequenceType.InframeDeletion;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            // check the predicates
            if (!_preCache.IsCoding || !_preCache.IsDeletion)
            {
                _cache.Add(ct, false);
                return false;
            }

            if (!_altAllele.IsStructuralVariant)
            {
                if (_preCache.ReferenceCodonLen == 0 //|| (PreCache.ReferenceCodonLen < PreCache.AlternateCodonLen) 
                    || IsFrameshiftVariant()
                    || IsIncompleteTerminalCodonVariant()
                    || IsStopGained())
                {
                    _cache.Add(ct, false);
                    return false;
                }

                // simple string match
                var referenceCodon = _preCache.ReferenceCodon.ToLower();
                var alternateCodon = _preCache.AlternateCodon.ToLower();

                if (referenceCodon.StartsWith(alternateCodon) || referenceCodon.EndsWith(alternateCodon))
                {
                    _cache.Add(ct, true);
                    return true;
                }

                // try a more complex string match
                var commonPrefixLength = _preCache.ReferenceCodon.CommonPrefixLength(_preCache.AlternateCodon);
                var commonSuffixLength = _preCache.ReferenceCodon.CommonSuffixLength(_preCache.AlternateCodon);
                _preCache.AlternateCodonLen = _preCache.AlternateCodonLen - commonPrefixLength - commonSuffixLength;

                result = _preCache.AlternateCodonLen == 0;
            }

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is an inframe insertion [VariantEffect.pm:780 inframe_insertion]
        /// </summary>
        public bool IsInframeInsertion()
        {
            const ConsequenceType ct = ConsequenceType.InframeInsertion;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            // check the predicates
            if (!_preCache.IsCoding || !_preCache.IsInsertion)
            {
                _cache.Add(ct, false);
                return false;
            }

            bool result = false;

            if (!_altAllele.IsStructuralVariant)
            {
                if (IsStopRetained() ||
                    IsFrameshiftVariant() ||
                    IsStartLost() ||
                    _preCache.AlternateCodonLen <= _preCache.ReferenceCodonLen ||
                    IsIncompleteTerminalCodonVariant())
                {
                    _cache.Add(ct, false);
                    return false;
                }



                if (!IsTruncatedByStop()) result = true;


            }

            _cache.Add(ct, result);
            return result;
        }




        private bool IsTruncatedByStop()
        {
            if (_preCache.AlternateAminoAcids != null && _preCache.AlternateAminoAcids.Contains(AminoAcids.StopCodon))
            {
                var stopPos = _preCache.AlternateAminoAcids.IndexOf(AminoAcids.StopCodon, StringComparison.Ordinal);
                var altAminoAcidesBeforeStop = _preCache.AlternateAminoAcids.Substring(0, stopPos);
                if (_preCache.AlternateAminoAcids.StartsWith(AminoAcids.StopCodon) ||
                    _preCache.ReferenceAminoAcids.StartsWith(altAminoAcidesBeforeStop))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// returns true if at least one base of the first codon was changed in the transcript [VariantEffect.pm:722 affects_start_codon]
        /// </summary>
        public bool IsStartLost()
        {
            const ConsequenceType ct = ConsequenceType.StartLost;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            // check the predicates
            if (!_preCache.IsCoding)
            {
                _cache.Add(ct, false);
                return false;
            }

            bool result = false;

            if (!_altAllele.IsStructuralVariant)
            {
                if (_ta.ProteinBegin != 1 || _preCache.ReferenceAminoAcidsLen == 0)
                {
                    _cache.Add(ct, false);
                    return false;
                }

                result = _preCache.AlternateAminoAcidsLen == 0 || _preCache.AlternateAminoAcids[0] != _preCache.ReferenceAminoAcids[0];
            }

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a missense variant [VariantEffect.pm:682 missense_variant]
        /// </summary>
        public bool IsMissenseVariant()
        {
            const ConsequenceType ct = ConsequenceType.MissenseVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            // check the predicates
            if (!_preCache.IsCoding)
            {
                _cache.Add(ct, false);
                return false;
            }

            if (_altAllele.IsStructuralVariant ||
                IsStartLost() ||
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

            if (_preCache.ReferenceAminoAcids != _preCache.AlternateAminoAcids &&
                _preCache.ReferenceAminoAcidsLen == _preCache.AlternateAminoAcidsLen)
                result = true;

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a non-coding transcript exon variant [VariationEffect.pm:405 non_coding_exon_variant]
        /// </summary>
        public bool IsNonCodingTranscriptExonVariant()
        {
            const ConsequenceType ct = ConsequenceType.NonCodingTranscriptExonVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = !_altAllele.IsStructuralVariant && _ta.HasExonOverlap && IsNonCodingTranscriptVariant();

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a nonsense-mediated decay transcript variant [VariationEffect.pm:391 within_nmd_transcript]
        /// </summary>
        public bool IsNonsenseMediatedDecayTranscriptVariant()
        {
            return _transcript.BioType == BioType.NonsenseMediatedDecay && !_altAllele.IsStructuralVariant;
        }

        /// <summary>
        /// returns true if the variant is a protein altering variant [VariationEffect.pm:300 protein_altering_variant]
        /// </summary>
        public bool IsProteinAlteringVariant()
        {
            const ConsequenceType ct = ConsequenceType.ProteinAlteringVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            if (_altAllele.IsStructuralVariant)
            {
                _cache.Add(ct, false);
                return false;
            }

            var result = true;

            var sameLen = _preCache.ReferenceAminoAcidsLen == _preCache.AlternateAminoAcidsLen;
            var startsWithTer = _preCache.ReferenceAminoAcids.StartsWith("X") || _preCache.AlternateAminoAcids.StartsWith("X");

            var isInframeDeletion = IsInframeDeletion();
            // Note: sequence ontology says that stop retained should not be here (http://www.sequenceontology.org/browser/current_svn/term/SO:0001567)
            var isStopCodonVarinat = IsStopLost() || IsStopGained();//|| IsStopRetained() 

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
            const ConsequenceType ct = ConsequenceType.SpliceRegionVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            if (!_altAllele.IsStructuralVariant)
            {
                if (IsSpliceDonorVariant())
                {
                    // false
                }
                else if (IsSpliceAcceptorVariant())
                {
                    // false
                }
                else if (IsEssentialSpliceSite())
                {
                    // false
                }
                else
                {
                    result = _ta.IsWithinSpliceSiteRegion;
                }
            }

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant's amino acid changes to a stop codon [VariationEffect.pm:884 stop_gained]
        /// </summary>
        public bool IsStopGained()
        {
            const ConsequenceType ct = ConsequenceType.StopGained;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            if (!_altAllele.IsStructuralVariant)
                result = !IsStopRetained() && (_ta.ReferenceAminoAcids == null || !_ta.ReferenceAminoAcids.Contains(AminoAcids.StopCodon)) && _ta.AlternateAminoAcids != null && _ta.AlternateAminoAcids.Contains(AminoAcids.StopCodon);

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a stop lost variant [VariationEffect.pm:898 stop_lost]
        /// </summary>
        public bool IsStopLost()
        {
            const ConsequenceType ct = ConsequenceType.StopLost;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;
            if (!_altAllele.IsStructuralVariant && !string.IsNullOrEmpty(_ta.ReferenceAminoAcids) && _ta.AlternateAminoAcids != null)
                result = _ta.ReferenceAminoAcids.Contains(AminoAcids.StopCodon) &&
                          !_ta.AlternateAminoAcids.Contains(AminoAcids.StopCodon);

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a stop retained variant [VariationEffect.pm:701 stop_lost]
        /// </summary>
        public bool IsStopRetained()
        {
            const ConsequenceType ct = ConsequenceType.StopRetainedVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;


            var alternateAminoAcids = TrimPeptides(_ta.AlternateAminoAcids);

            if (!_altAllele.IsStructuralVariant)
                result = _ta.ReferenceAminoAcids != null && alternateAminoAcids != null &&
                         _ta.ReferenceAminoAcids == alternateAminoAcids &&
                         _ta.ReferenceAminoAcids.Contains(AminoAcids.StopCodon) ||
                         string.IsNullOrEmpty(_ta.ReferenceAminoAcids) && alternateAminoAcids != null &&
                         _ta.ProteinBegin == _transcript.Translation.PeptideSeq.Length + 1 &&
                         alternateAminoAcids == AminoAcids.StopCodon;


            _cache.Add(ct, result);
            return result;
        }

        private static string TrimPeptides(string alternateAminoAcids)
        {
            if (alternateAminoAcids == null) return null;
            if (!alternateAminoAcids.Contains(AminoAcids.StopCodon)) return alternateAminoAcids;
            var pos = alternateAminoAcids.IndexOf(AminoAcids.StopCodon, StringComparison.Ordinal);
            return pos < 0 ? alternateAminoAcids : alternateAminoAcids.Substring(0, pos + 1);
        }

        /// <summary>
        /// returns true if the variant is a synonymous variant [VariationEffect.pm:755 synonymous_variant]
        /// </summary>
        public bool IsSynonymousVariant()
        {
            const ConsequenceType ct = ConsequenceType.SynonymousVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            if (!_altAllele.IsStructuralVariant)
                result = _ta.ReferenceAminoAcids != null &&
                         (_altAllele.VepVariantType == VariantType.SNV || _altAllele.VepVariantType == VariantType.MNV) &&
                         _ta.ReferenceAminoAcids == _ta.AlternateAminoAcids && !_ta.ReferenceAminoAcids.Contains("X") && !_ta.AlternateAminoAcids.Contains("X") && !IsStopRetained();

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant is a 3' UTR variant [VariationEffect.pm:609 within_3_prime_utr]
        /// </summary>
        public bool IsThreePrimeUtrVariant()
        {
            const ConsequenceType ct = ConsequenceType.ThreePrimeUtrVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            if (!_altAllele.IsStructuralVariant && _transcript.Translation != null)
            {
                var hasTranslation = _transcript.Translation != null;

                var isThreePrimeOfCoding = _transcript.Gene.OnReverseStrand
                    ? IsBeforeCoding(_altAllele.Start, _altAllele.End, _transcript.Start, _transcript.Translation.CodingRegion.GenomicStart, hasTranslation)
                    : IsAfterCoding(_altAllele.Start, _altAllele.End, _transcript.End, _transcript.Translation.CodingRegion.GenomicEnd, hasTranslation);

                result = isThreePrimeOfCoding && IsWithinCdna();
            }

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if the variant occurs within the cDNA [VariationEffect.pm:469 within_cdna]
        /// </summary>
        private bool IsWithinCdna()
        {
            const TempConsequenceType ct = TempConsequenceType.WithinCdna;
            if (_tempCache.Contains(ct)) return _tempCache.Get(ct);

            bool result = _ta.BackupCdnaEnd > 0 && _ta.BackupCdnaEnd <= _transcript.TotalExonLength;

            _tempCache.Add(ct, result);
            return result;
        }

        ///<summary>
        /// returns true if the variant occurs within the CDS [VariationEffect.pm:501 within_cds]
        /// </summary>
        private bool IsWithinCds()
        {
            const TempConsequenceType ct = TempConsequenceType.WithinCds;
            if (_tempCache.Contains(ct)) return _tempCache.Get(ct);

            if (_transcript.Translation == null) return false;

            // Here I have a complete refactoring of this code.
            // the name seems to suggests that any overlap between the variant and transcript coding region should set it to true
            bool result = false;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var cdnaCoordinateMap in _transcript.CdnaMaps)
            {
                var cdsStartGenomic = cdnaCoordinateMap.GenomicStart > _transcript.Translation.CodingRegion.GenomicStart ?
                    cdnaCoordinateMap.GenomicStart : _transcript.Translation.CodingRegion.GenomicStart;

                // this is the genomic start position of the cds of this cdnaCoordinate if it exists
                // it does not exist if this exon is completely a UTR. In that case, it is set to the transcript
                // coding region start
                var cdsEndGenomic = cdnaCoordinateMap.GenomicEnd < _transcript.Translation.CodingRegion.GenomicEnd ?
                    cdnaCoordinateMap.GenomicEnd : _transcript.Translation.CodingRegion.GenomicEnd;

                if (_altAllele.Start <= cdsEndGenomic &&
                    _altAllele.End >= cdsStartGenomic)
                {
                    _tempCache.Add(ct, true);
                    return true;
                }
            }

            // we also need to check if the vf is in a frameshift intron within the CDS
            if (_transcript.Translation != null && _ta.IsWithinFrameshiftIntron)
            {
                // if the variant and transcript coding region overlaps
                result = _altAllele.Start <= _transcript.Translation.CodingRegion.GenomicEnd
                         && _transcript.Translation.CodingRegion.GenomicStart <= _altAllele.End;
            }

            _tempCache.Add(ct, result);
            return result;
        }

        ///<summary>
        /// returns true if the variant occurs within an intron [VariationEffect.pm:494 within_intron]
        /// </summary>
        public bool IsWithinIntron()
        {
            return !_altAllele.IsStructuralVariant && _ta.IsWithinIntron;
        }

        /// <summary>
        /// returns true if the variant is within a non-coding gene [VariationEffect.pm:398 within_non_coding_gene]
        /// </summary>
        public bool IsNonCodingTranscriptVariant()
        {
            const ConsequenceType ct = ConsequenceType.NonCodingTranscriptVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            // TODO: Isn't IsWithinTranscript always true? and not within mature miRNA is always true
            // For Ensembl transcript, miRNA may be a valid attribute. We have their location and we would like to check if the variant overlaps with the miRNA
            var result = !_altAllele.IsStructuralVariant && _transcript.Translation == null && !IsMatureMirnaVariant();

            _cache.Add(ct, result);
            return result;
        }

        /// <summary>
        /// returns true if it's a coding sequnce variant [VariationEffect.pm:998 coding_unknown]
        /// </summary>
        public bool IsCodingSequenceVariant()
        {
            const ConsequenceType ct = ConsequenceType.CodingSequenceVariant;
            if (_cache.Contains(ct)) return _cache.Get(ct);

            bool result = false;

            if (!_altAllele.IsStructuralVariant)
            {
                result = IsWithinCds() &&
                         (string.IsNullOrEmpty(_transcript.Translation.PeptideSeq) ||
                          string.IsNullOrEmpty(_ta.AlternateAminoAcids) || _ta.AlternateAminoAcids.Contains("X"))
                         && !(IsFrameshiftVariant() || IsInframeDeletion() || IsIncompleteTerminalCodonVariant() || IsProteinAlteringVariant() || IsStopGained() || IsStopRetained());
            }

            _cache.Add(ct, result);
            return result;
        }

        // ==============================================================
        // These calls are already mostly cached by FeatureVariantEffects
        // ==============================================================

        /// <summary>
        /// returns true if the variant ablates the transcript [VariationEffect.pm:262 feature_ablation]
        /// </summary>
        public bool HasTranscriptAblation()
        {
            return _featureVariantEffects.Ablation();
        }

        /// <summary>
        /// returns true if the variant amplifies the transcript [VariationEffect.pm:269 feature_amplification]
        /// </summary>
        public bool HasTranscriptAmplification()
        {
            return _featureVariantEffects.Amplification();
        }

        /// <summary>
        /// returns true if the variant truncates the transcript [VariationEffect.pm:288 feature_truncation]
        /// </summary>
        public bool HasTranscriptTruncation()
        {
            return _featureVariantEffects.Truncation();
        }

        /// <summary>
        /// returns true if the variant elongates the transcript [VariationEffect.pm:276 feature_elongation]
        /// </summary>
        public bool HasTranscriptElongation()
        {
            return _featureVariantEffects.Elongation();
        }

	    public bool IsGeneFusion()
	    {
			const ConsequenceType ct = ConsequenceType.GeneFusion;
			if (_cache.Contains(ct)) return _cache.Get(ct);

		    if (_ta.BreakendTranscriptAnnotation == null || _ta.BreakendPos2Annotations == null ||
		        _ta.BreakendPos2Annotations.Count <= 0 || !_ta.BreakendTranscriptAnnotation.InCodingRegion)
		    {
				_cache.Add(ConsequenceType.GeneFusion,false);
			    return false;
		    }
		    var result = _ta.BreakendPos2Annotations.Any(
			    x =>
				    x.GeneName != _ta.BreakendTranscriptAnnotation.GeneName && x.InCodingRegion &&
				    !x.IsTranscriptCodingRegionOverlapped(_ta.BreakendTranscriptAnnotation) &&
				    x.TranscriptDataSource == _ta.BreakendTranscriptAnnotation.TranscriptDataSource);


			_cache.Add(ConsequenceType.GeneFusion, result);
			return result;

		}




    }
}
