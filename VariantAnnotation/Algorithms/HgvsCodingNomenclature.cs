using System.Linq;
using System.Text;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.Annotation;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.DataStructures.Variants;

namespace VariantAnnotation.Algorithms
{
    public sealed class HgvsCodingNomenclature
    {
        #region members

        private readonly TranscriptAnnotation _ta;
        private readonly Transcript _transcript;
        private readonly VariantFeature _variant;
        private readonly ICompressedSequence _compressedSequence;
        private readonly bool _isGenomicDuplicate;

        private readonly HgvsNotation _hgvsNotation;
        private readonly StringBuilder _sb;
        private readonly int _hgvsStart;
        private readonly int _hgvsEnd;

        #endregion

        private sealed class PositionOffset
        {
            #region members

            public int? Position;
            public int? Offset;
            public string Value;
            public bool HasStopCodonNotation;

            #endregion

            /// <summary>
            /// constructor
            /// </summary>
            public PositionOffset(int position)
            {
                Position = position;
            }
        }

        private sealed class HgvsNotation
        {
            #region members

            public string ReferenceBases;
            public readonly string AlternateBases;

            public PositionOffset Start;
            public PositionOffset End;

            public readonly string TranscriptId;

            public readonly char TranscriptType;

            public GenomicChange Type;
            public int AlleleMultiple;

            private const char CodingType    = 'c';
            private const char NonCodingType = 'n';

            #endregion

            /// <summary>
            /// constructor
            /// </summary>
            public HgvsNotation(string referenceBases, string alternateBases, string transcriptId, int start, int end,
                bool isCoding)
            {
                TranscriptId = transcriptId;

                Start = new PositionOffset(start);
                End   = new PositionOffset(end);

                ReferenceBases = referenceBases ?? "";
                AlternateBases = alternateBases ?? "";

                TranscriptType = isCoding ? CodingType : NonCodingType;
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public HgvsCodingNomenclature(TranscriptAnnotation ta, Transcript transcript, VariantFeature variant,
            ICompressedSequence compressedSequence, bool isGenomicDuplicate)
        {
            _ta                 = ta;
            _transcript         = transcript;
            _variant            = variant;
            _compressedSequence = compressedSequence;
            _isGenomicDuplicate = isGenomicDuplicate;

            _sb = new StringBuilder();

            // get reference sequence strand
            var transcriptOnReverseStrand = transcript.Gene.OnReverseStrand;

            // this may be different to the input one for insertions/deletions
            var altAllele = ta.AlternateAllele;
            string variationFeatureSequence = altAllele.AlternateAllele;

            // get the reverse complement of the vfs if needed
            if (transcriptOnReverseStrand)
            {
                variationFeatureSequence = SequenceUtilities.GetReverseComplement(variationFeatureSequence);
            }

            // calculate the reference start and end
            GetReferenceCoordinates(transcript, altAllele, out _hgvsStart, out _hgvsEnd);

            // decide event type from HGVS nomenclature
            _hgvsNotation = new HgvsNotation(ta.TranscriptReferenceAllele, variationFeatureSequence,
                FormatUtilities.CombineIdAndVersion(transcript.Id, transcript.Version), _hgvsStart, _hgvsEnd,
                _transcript.Translation != null);
        }

        /// <summary>
        /// return a string representing the cDNA-level effect of this allele in HGVS format [TranscriptVariationAllele.pm:568 hgvs_transcript]
        /// </summary>
        public void SetAnnotation()
        {
            // sanity check: don't try to handle odd characters, make sure this is not a reference allele, 
            //               and make sure that we have protein coordinates
            if (_variant.IsReference || SequenceUtilities.HasNonCanonicalBase(_ta.TranscriptAlternateAllele)) return;

            GetGenomicChange(_transcript, _hgvsNotation, _isGenomicDuplicate);

            GetCdnaPosition(_hgvsNotation.Start);
            if (_hgvsStart == _hgvsEnd) _hgvsNotation.End = _hgvsNotation.Start;
            else GetCdnaPosition(_hgvsNotation.End);

            // sanity check: make sure we have coordinates
            if (_hgvsNotation.Start.Position == null || _hgvsNotation.End.Position == null) return;

            var transcriptLen = _transcript.End - _transcript.Start + 1;

            //_hgvs notation past the transcript
            if (_hgvsNotation.Start.Position > transcriptLen || _hgvsNotation.End.Position > transcriptLen) return;

            // make sure that start is always less than end
            SwapEndpoints(_hgvsNotation);

            // generic formatting
            _ta.HgvsCodingSequenceName = FormatHgvsString();
        }

        /// <summary>
        /// HGVS aligns changes 3' 
        /// e.g. given a ATG/- deletion in C[ATG]ATGT, we want to move to: CATG[ATG]T
        ///      given a   A/- deletion in  TA[A]AAAA, we want to move to:  TAAAAA[A]
        ///      given a  AA/- deletion in  TA[AA]AAA, we want to move to:  TAAAA[AA]
        /// </summary>
        private static void SwapEndpoints(HgvsNotation hn)
        {
            if (hn.Start.Offset == null) hn.Start.Offset = 0;
            if (hn.End.Offset   == null) hn.End.Offset   = 0;

            if (!hn.End.HasStopCodonNotation && hn.Start.Position + hn.Start.Offset > hn.End.Position + hn.End.Offset)
            {
                var temp = hn.Start;
                hn.Start = hn.End;
                hn.End   = temp;
            }
        }

        /// <summary>
        /// return a string representing the coding-level effect of this allele in HGVS format [Sequence.pm:615 format_hgvs_string]
        /// </summary>
        private string FormatHgvsString()
        {
            // all start with transcript name & numbering type
            _sb.Append(_hgvsNotation.TranscriptId + ':' + _hgvsNotation.TranscriptType + '.');

            // handle single and multiple positions
            string coordinates = _hgvsNotation.Start.Value == _hgvsNotation.End.Value
                ? _hgvsNotation.Start.Value
                : _hgvsNotation.Start.Value + '_' + _hgvsNotation.End.Value;

            // format rest of string according to type
            switch (_hgvsNotation.Type)
            {
                case GenomicChange.Deletion:
                    _sb.Append(coordinates + "del" + _hgvsNotation.ReferenceBases);
                    break;
                case GenomicChange.Inversion:
                    _sb.Append(coordinates + "inv" + _hgvsNotation.ReferenceBases);
                    break;
                case GenomicChange.Duplication:
                    _sb.Append(coordinates + "dup" + _hgvsNotation.ReferenceBases);
                    break;
                case GenomicChange.Substitution:
                    _sb.Append(_hgvsNotation.Start.Value + _hgvsNotation.ReferenceBases + '>' + _hgvsNotation.AlternateBases);
                    break;
                case GenomicChange.InDel:
                    _sb.Append(coordinates + "del" + _hgvsNotation.ReferenceBases + "ins" + _hgvsNotation.AlternateBases);
                    break;
                case GenomicChange.Insertion:
                    _sb.Append(coordinates + "ins" + _hgvsNotation.AlternateBases);
                    break;
                case GenomicChange.Multiple:
                    _sb.Append(coordinates + '[' + _hgvsNotation.AlleleMultiple + ']' + _hgvsNotation.ReferenceBases);
                    break;
                default:
                    throw new GeneralException("Unhandled genomic change found: " + _hgvsNotation.Type);
            }

            return _sb.ToString();
        }

        private static void GetReferenceCoordinates(Transcript transcript, VariantAlternateAllele altAllele,
            out int hgvsStart, out int hgvsEnd)
        {
            // calculate the HGVS position: use HGVS coordinates not variation feature coordinates due to duplications
            if (transcript.Gene.OnReverseStrand)
            {
                hgvsStart = transcript.End - altAllele.End + 1;
                hgvsEnd   = transcript.End - altAllele.Start + 1;
            }
            else
            {
                hgvsStart = altAllele.Start - transcript.Start + 1;
                hgvsEnd   = altAllele.End - transcript.Start + 1;
            }
        }

        /// <summary>
        /// gets the variant position (with intron offset) in the transcript [TranscriptVariationAllele.pm:1805 _get_cDNA_position]
        /// </summary>
        private void GetCdnaPosition(PositionOffset po)
        {
            int? position = po.Position;

            // start and stop coordinate relative to transcript. Take into account which
            // strand we're working on
            position = _transcript.Gene.OnReverseStrand
                ? _transcript.End - position + 1
                : _transcript.Start + position - 1;

            if (position > _transcript.End || position < _transcript.Start)
            {
                po.Position = null;
                return;
            }


            var exons = _transcript.CdnaMaps;

            // loop over the exons and get the coordinates of the variation in exon+intron notation

            for (int exonIndex = 0; exonIndex < exons.Length; exonIndex++)
            {
                var exon = exons[exonIndex];

                // skip if the start point is beyond this exon
                if (position > exon.GenomicEnd) continue;

                // EXONIC: if the start coordinate is within this exon
                if (position >= exon.GenomicStart)
                {
                    // get the cDNA start coordinate of the exon and add the number of nucleotides
                    // from the exon boundary to the variation. If the transcript is in the opposite
                    // direction, count from the end instead
                    int tempCdnaEnd, tempCdnaBegin;

                    TranscriptUtilities.GetCodingDnaEndpoints(_transcript.CdnaMaps, exon.GenomicStart, exon.GenomicEnd,
                        out tempCdnaBegin, out tempCdnaEnd);

                    po.Position = tempCdnaBegin + (_transcript.Gene.OnReverseStrand
                        ? exon.GenomicEnd - position
                        : position - exon.GenomicStart);

                    break;
                }

                // INTRONIC: the start coordinate is between this exon and the previous one, determine which one is closest and get coordinates relative to that one

                // sanity check: make sure we have at least passed one exon
                if (exonIndex < 1)
                {
                    po.Position = null;
                    return;
                }

                var prevExon = exons[exonIndex - 1];
                GetIntronOffset(prevExon, exon, position, po);
                break;
            }

            // start by correcting for the stop codon
            int startCodon = _transcript.Translation == null ? -1 : _transcript.Translation.CodingRegion.CdnaStart;
            int stopCodon  = _transcript.Translation == null ? -1 : _transcript.Translation.CodingRegion.CdnaEnd;

            string cdnaCoord = po.Position.ToString();
            po.HasStopCodonNotation = false;
            bool hasNoPosition = false;

            if (stopCodon != -1)
            {
                if (po.Position > stopCodon)
                {
                    cdnaCoord = '*' + (po.Position - stopCodon).ToString();
                    po.HasStopCodonNotation = true;
                }
                else if (po.Offset != null && po.Position == stopCodon)
                {
                    cdnaCoord = "*";
                    po.HasStopCodonNotation = true;
                    hasNoPosition = true;
                }
            }

            if (!po.HasStopCodonNotation && startCodon != -1)
            {
                cdnaCoord = (po.Position + (po.Position >= startCodon ? 1 : 0) - startCodon).ToString();
            }

            // re-assemble the cDNA position  [ return exon num & offset & direction for intron eg. 142+363]
            if (hasNoPosition) po.Value = "*" + po.Offset;
            else po.Value = cdnaCoord + (po.Offset == null ? "" : ((int)po.Offset).ToString("+0;-0;+0"));
        }

        /// <summary>
        /// get the genomic change that resulted from this variation [Sequence.pm:482 hgvs_variant_notation]
        /// </summary>
        private void GetGenomicChange(Transcript transcript, HgvsNotation hn, bool isGenomicDuplicate)
        {
            hn.Type = GenomicChange.Unknown;

            // make sure our positions are defined
            if (hn.Start.Position == null || hn.End.Position == null) return;

            int displayStart = (int)hn.Start.Position;
            int displayEnd   = (int)hn.End.Position;

            // length of the reference allele. Negative lengths make no sense
            int refLength = displayEnd - displayStart + 1;
            if (refLength < 0) refLength = 0;

            // length of alternative allele
            var altLength = hn.AlternateBases.Length;

            // sanity check: make sure that the alleles are different
            if (hn.ReferenceBases == hn.AlternateBases) return;

            // deletion
            if (altLength == 0)
            {
                hn.Type = GenomicChange.Deletion;
                return;
            }

            if (refLength == altLength)
            {
                // substitution
                if (refLength == 1)
                {
                    hn.Type = GenomicChange.Substitution;
                    return;
                }

                // inversion
                var rcRefAllele = SequenceUtilities.GetReverseComplement(hn.ReferenceBases);
                hn.Type = hn.AlternateBases == rcRefAllele ? GenomicChange.Inversion : GenomicChange.InDel;
                return;
            }

            // If this is an insertion, we should check if the preceeding reference nucleotides
            // match the insertion. In that case it should be annotated as a multiplication.
            if (refLength == 0)
            {
                int prevPosition = displayEnd - altLength;

                if (!isGenomicDuplicate && _compressedSequence != null && prevPosition >= 0)
                {
                    // Get the same number of nucleotides preceding the insertion as the length of
                    // the insertion
                    var precedingBases = SequenceUtilities.GetSubSubstring(transcript.Start, transcript.End,
                        transcript.Gene.OnReverseStrand, prevPosition, prevPosition + altLength - 1, _compressedSequence);
                    if (precedingBases == hn.AlternateBases) isGenomicDuplicate = true;
                }

                if (isGenomicDuplicate)
                {
                    hn.Type = GenomicChange.Duplication;

                    // for duplication, the hgvs positions are deceremented by alt allele length
                    var incrementLength = altLength;
                    hn.Start.Position   = displayStart - incrementLength;
                    hn.End.Position     = hn.Start.Position + incrementLength - 1;

                    hn.AlleleMultiple = 2;
                    hn.ReferenceBases = hn.AlternateBases;
                    return;
                }

                // otherwise just an insertion
                hn.Type           = GenomicChange.Insertion;
                hn.Start.Position = displayEnd;
                hn.End.Position   = displayStart;
                return;
            }

            // Otherwise, the reference and allele are of different lengths. By default, this is
            // a delins but we need to check if the alt allele is a multiplication of the reference.
            // Check if the length of the alt allele is a multiple of the reference allele
            if (altLength % refLength == 0)
            {
                hn.AlleleMultiple = altLength / refLength;
                string multRefAllele = string.Concat(Enumerable.Repeat(hn.ReferenceBases, hn.AlleleMultiple));

                if (hn.AlternateBases == multRefAllele)
                {
                    hn.Type = hn.AlleleMultiple == 2 ? GenomicChange.Duplication : GenomicChange.Multiple;
                    return;
                }
            }

            // deletion/insertion
            hn.Type = GenomicChange.InDel;
        }

        /// <summary>
        /// get the shorted intron offset from the nearest exon
        /// </summary>
        private void GetIntronOffset(CdnaCoordinateMap prevExon, CdnaCoordinateMap exon, int? position, PositionOffset po)
        {
            int? upDist   = position - prevExon.GenomicEnd;
            int? downDist = exon.GenomicStart - position;

            int tempCdnaBegin, tempCdnaEnd;

            if (upDist < downDist || upDist == downDist && !_transcript.Gene.OnReverseStrand)
            {
                // distance to upstream exon is the shortest (or equal and in the positive orientation)
                TranscriptUtilities.GetCodingDnaEndpoints(_transcript.CdnaMaps, prevExon.GenomicStart,
                    prevExon.GenomicEnd, out tempCdnaBegin, out tempCdnaEnd);

                if (_transcript.Gene.OnReverseStrand)
                {
                    po.Position = tempCdnaBegin;
                    po.Offset   = -upDist;
                }
                else
                {
                    po.Position = tempCdnaEnd;
                    po.Offset   = upDist;
                }
            }
            else
            {
                // distance to downstream exon is the shortest
                TranscriptUtilities.GetCodingDnaEndpoints(_transcript.CdnaMaps, exon.GenomicStart, exon.GenomicEnd,
                    out tempCdnaBegin, out tempCdnaEnd);

                if (_transcript.Gene.OnReverseStrand)
                {
                    po.Position = tempCdnaEnd;
                    po.Offset   = downDist;
                }
                else
                {
                    po.Position = tempCdnaBegin;
                    po.Offset   = -downDist;
                }
            }
        }
    }

    public enum GenomicChange
    {
        Unknown,
        Deletion,
        Duplication,
        InDel,
        Insertion,
        Inversion,
        Multiple,
        Substitution
    }
}
