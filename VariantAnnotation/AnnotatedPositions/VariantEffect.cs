using System;
using Cache.Data;
using OptimizedCore;
using VariantAnnotation.AnnotatedPositions.AminoAcids;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.AnnotatedPositions;

/// <summary>
/// This class performs all of the functional consequence testing. An additional caching layer
/// has been added to prevent unneeded calculations. The caching layer is reset when each new
/// variant has been read.
/// </summary>
public sealed class VariantEffect : IVariantEffect
{
    private readonly TranscriptPositionalEffect _preCache;

    private readonly Transcript     _transcript;
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

    public VariantEffect(TranscriptPositionalEffect transcriptEffect, ISimpleVariant variant, Transcript transcript,
        string referenceAminoAcids, string alternateAminoAcids, string referenceCodons, string alternateCodons,
        int? proteinBegin, string coveredReferenceAminoAcids, string coveredAlternateAminoAcids,
        VariantEffectCache cache = null)
    {
        _transcript = transcript;
        _variant    = variant;

        _preCache = transcriptEffect;

        _cache = cache ?? new VariantEffectCache();

        _referenceAminoAcids    = referenceAminoAcids;
        _alternateAminoAcids    = alternateAminoAcids;
        _referenceAminoAcidsLen = _referenceAminoAcids?.Length ?? 0;
        _alternateAminoAcidsLen = _alternateAminoAcids?.Length ?? 0;

        _coveredReferenceAminoAcids = coveredReferenceAminoAcids;
        _coveredAlternateAminoAcids = coveredAlternateAminoAcids;

        _referenceCodons    = referenceCodons;
        _alternateCodons    = alternateCodons;
        _referenceCodonsLen = _referenceCodons?.Length ?? 0;
        _alternateCodonsLen = _alternateCodons?.Length ?? 0;

        _isInsertion = variant.AltAllele.Length > variant.RefAllele.Length;
        _isDeletion  = variant.AltAllele.Length < variant.RefAllele.Length;

        _proteinBegin = proteinBegin ?? -1;
    }

    public bool IsSpliceAcceptorVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.splice_acceptor_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        bool result = _transcript.Gene.OnReverseStrand ? _preCache.IsStartSpliceSite : _preCache.IsEndSpliceSite;

        _cache.Add(ct, result);
        return result;
    }

    public bool IsSpliceDonorVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.splice_donor_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        bool result = _transcript.Gene.OnReverseStrand ? _preCache.IsEndSpliceSite : _preCache.IsStartSpliceSite;

        _cache.Add(ct, result);
        return result;
    }

    public bool IsFivePrimeUtrVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.five_prime_UTR_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        bool result = false;

        if (_transcript.CodingRegion != null)
        {
            var isFivePrimeOfCoding = _transcript.Gene.OnReverseStrand
                ? _preCache.AfterCoding
                : _preCache.BeforeCoding;

            result = isFivePrimeOfCoding && _preCache.WithinCdna;
        }

        _cache.Add(ct, result);
        return result;
    }

    public bool IsFrameshiftVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.frameshift_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        if (!_preCache.IsCoding)
        {
            _cache.Add(ct, false);
            return false;
        }

        bool result = _preCache.HasFrameShift;

        _cache.Add(ct, result);
        return result;
    }

    public bool IsIncompleteTerminalCodonVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.incomplete_terminal_codon_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        if (_transcript.CodingRegion == null)
        {
            _cache.Add(ct, false);
            return false;
        }

        int cdsLength       = _transcript.CodingRegion.CdnaEnd - _transcript.CodingRegion.CdnaStart + 1;
        int codonCdsStart   = _proteinBegin * 3 - 2;
        int lastCodonLength = cdsLength - (codonCdsStart - 1);

        bool result = lastCodonLength < 3 && lastCodonLength > 0;

        _cache.Add(ct, result);
        return result;
    }

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

        if (IsStopRetained()                           ||
            IsFrameshiftVariant()                      ||
            IsStartLost()                              ||
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
        if (_alternateAminoAcids != null && _alternateAminoAcids.Contains(AminoAcidCommon.StopCodon))
        {
            int    stopPos = _alternateAminoAcids.IndexOf(AminoAcidCommon.StopCodon, StringComparison.Ordinal);
            string altAminoAcidsBeforeStop = _alternateAminoAcids.Substring(0, stopPos);
            if (_alternateAminoAcids.OptimizedStartsWith(AminoAcidCommon.StopCodon) ||
                _referenceAminoAcids.StartsWith(altAminoAcidsBeforeStop))
                return true;
        }

        return false;
    }

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

        if (IsStartLost()                      ||
            IsStopLost()                       ||
            IsStopGained()                     ||
            IsIncompleteTerminalCodonVariant() ||
            IsFrameshiftVariant()              ||
            IsInframeDeletion()                ||
            IsInframeInsertion())
        {
            _cache.Add(ct, false);
            return false;
        }

        bool result = _referenceAminoAcids != _alternateAminoAcids &&
            _referenceAminoAcidsLen        == _alternateAminoAcidsLen;

        _cache.Add(ct, result);
        return result;
    }

    public bool IsNonCodingTranscriptExonVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.non_coding_transcript_exon_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        bool result = _preCache.HasExonOverlap && _transcript.CodingRegion == null && !_preCache.OverlapWithMicroRna;

        _cache.Add(ct, result);
        return result;
    }

    public bool IsNonsenseMediatedDecayTranscriptVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.NMD_transcript_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);
        const bool result = false;
        // var result = _transcript.BioType == BioType.nonsense_mediated_decay;
        _cache.Add(ct, result);
        return result;
    }

    public bool IsProteinAlteringVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.protein_altering_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        var result = true;

        var sameLen = _referenceAminoAcidsLen == _alternateAminoAcidsLen;
        var startsWithTer = _referenceAminoAcids.OptimizedStartsWith('X') ||
            _alternateAminoAcids.OptimizedStartsWith('X');

        var isInframeDeletion = IsInframeDeletion();
        // Note: sequence ontology says that stop retained should not be here (http://www.sequenceontology.org/browser/current_svn/term/SO:0001567)
        var isStopCodonVarinat = IsStopLost() || IsStopGained();

        if (sameLen       || startsWithTer         || isInframeDeletion    || isStopCodonVarinat ||
            IsStartLost() || IsFrameshiftVariant() || IsInframeInsertion() || IsStopRetained()   || !_preCache.IsCoding)
        {
            result = false;
        }

        _cache.Add(ct, result);
        return result;
    }

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

    public bool IsStopGained()
    {
        const ConsequenceTag ct = ConsequenceTag.stop_gained;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        bool result = !IsStopRetained() &&
            (string.IsNullOrEmpty(_referenceAminoAcids) || !_referenceAminoAcids.Contains(AminoAcidCommon.StopCodon)) &&
            !string.IsNullOrEmpty(_alternateAminoAcids) && _alternateAminoAcids.Contains(AminoAcidCommon.StopCodon);

        _cache.Add(ct, result);
        return result;
    }

    public bool IsStopLost()
    {
        const ConsequenceTag ct = ConsequenceTag.stop_lost;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        bool result = false;
        if (!string.IsNullOrEmpty(_coveredReferenceAminoAcids) && _coveredAlternateAminoAcids != null)
            result = _coveredReferenceAminoAcids.Contains(AminoAcidCommon.StopCodon) &&
                !_coveredAlternateAminoAcids.Contains(AminoAcidCommon.StopCodon);

        _cache.Add(ct, result);
        return result;
    }

    public bool IsStopRetained()
    {
        const ConsequenceTag ct = ConsequenceTag.stop_retained_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);
        
        if (!_preCache.IsCoding)
        {
            _cache.Add(ct, false);
            return false;
        }

        string alternateAminoAcids = _alternateAminoAcids;

        bool result = !string.IsNullOrEmpty(_referenceAminoAcids) && alternateAminoAcids != null &&
            _referenceAminoAcids == alternateAminoAcids &&
            _referenceAminoAcids.Contains(AminoAcidCommon.StopCodon) ||
            string.IsNullOrEmpty(_referenceAminoAcids) && alternateAminoAcids != null &&
            _proteinBegin == _transcript.CodingRegion!.ProteinSeq.Length + 1 &&
            alternateAminoAcids.StartsWith(AminoAcidCommon.StopCodon);

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

        var    startProtein        = _referenceAminoAcids[0].ToString();
        string alternateAminoAcids = _alternateAminoAcids;

        var result = alternateAminoAcids != null
            && alternateAminoAcids.Contains(startProtein);

        _cache.Add(ct, result);
        return result;
    }

    // private static string TrimPeptides(string alternateAminoAcids)
    // {
    //     if (string.IsNullOrEmpty(alternateAminoAcids)) return null;
    //     if (!alternateAminoAcids.Contains(AminoAcidCommon.StopCodon)) return alternateAminoAcids;
    //     var pos = alternateAminoAcids.IndexOf(AminoAcidCommon.StopCodon, StringComparison.Ordinal);
    //     return pos < 0 ? alternateAminoAcids : alternateAminoAcids.Substring(0, pos + 1);
    // }

    public bool IsSynonymousVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.synonymous_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        bool result = !string.IsNullOrEmpty(_referenceAminoAcids) &&
            (_variant.Type    == VariantType.SNV ||
                _variant.Type == VariantType.MNV)        &&
            _referenceAminoAcids == _alternateAminoAcids && !_referenceAminoAcids.Contains("X") &&
            !_alternateAminoAcids.Contains("X")          && !IsStopRetained();

        _cache.Add(ct, result);
        return result;
    }

    public bool IsThreePrimeUtrVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.three_prime_UTR_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        bool result = false;

        if (_transcript.CodingRegion != null)
        {
            var isThreePrimeOfCoding = _transcript.Gene.OnReverseStrand
                ? _preCache.BeforeCoding
                : _preCache.AfterCoding;

            result = isThreePrimeOfCoding && _preCache.WithinCdna;
        }

        _cache.Add(ct, result);
        return result;
    }

    public bool IsNonCodingTranscriptVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.non_coding_transcript_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        // NOTE: Isn't IsWithinTranscript always true? and not within mature miRNA is always true
        // For Ensembl transcript, miRNA may be a valid attribute. We have their location and we would like to check if the variant overlaps with the miRNA
        var result = !_preCache.HasExonOverlap && _transcript.CodingRegion == null && !_preCache.OverlapWithMicroRna;

        _cache.Add(ct, result);
        return result;
    }

    public bool IsCodingSequenceVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.coding_sequence_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        bool result = _preCache.WithinCds &&
            (_transcript.CodingRegion == null ||
                string.IsNullOrEmpty(_alternateAminoAcids) || _alternateAminoAcids.Contains("X"))
            && !(IsFrameshiftVariant()     || IsInframeDeletion() || IsIncompleteTerminalCodonVariant() ||
                IsProteinAlteringVariant() || IsStopGained()      || IsStopRetained() || IsStopLost());

        _cache.Add(ct, result);
        return result;
    }

    public bool IsWithinIntron() => _preCache.IsWithinIntron;

    public bool IsMatureMirnaVariant()
    {
        const ConsequenceTag ct = ConsequenceTag.mature_miRNA_variant;
        if (_cache.Contains(ct)) return _cache.Get(ct);

        bool result = _preCache.OverlapWithMicroRna;

        _cache.Add(ct, result);
        return result;
    }
}