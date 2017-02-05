using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.Loftee
{
    public class Loftee : IPlugin
    {
        private readonly Dictionary<string, Transcript> _transcriptsById;

        /// <summary>
        /// constructor
        /// </summary>
        public Loftee()
        {
            _transcriptsById = new Dictionary<string, Transcript>();
        }

        public void AnnotateVariant(IVariantFeature variant, List<Transcript> transcripts,
            IAnnotatedVariant annotatedVariant, ICompressedSequence sequence)
        {
            if (variant.IsStructuralVariant) return;

            CreateTranscriptDictionary(transcripts);

            foreach (var altAllele in annotatedVariant.AnnotatedAlternateAlleles)
            {
                var ensemblTranscripts = new List<IAnnotatedTranscript>();

                foreach (var transcript in altAllele.EnsemblTranscripts)
                {
                    var ta = LofteeAnalysis(transcript, altAllele, sequence);
                    ensemblTranscripts.Add(ta);
                }

                altAllele.EnsemblTranscripts = ensemblTranscripts;

                var refSeqTranscripts = new List<IAnnotatedTranscript>();

                foreach (var transcript in altAllele.RefSeqTranscripts)
                {
                    var ta = LofteeAnalysis(transcript, altAllele, sequence);
                    refSeqTranscripts.Add(ta);
                }

                altAllele.RefSeqTranscripts = refSeqTranscripts;
            }
        }

        private void CreateTranscriptDictionary(List<Transcript> transcripts)
        {
            _transcriptsById.Clear();

            foreach (var transcript in transcripts)
            {
                _transcriptsById[TranscriptUtilities.GetTranscriptId(transcript)] = transcript;
            }
        }

        private IAnnotatedTranscript LofteeAnalysis(IAnnotatedTranscript ta, IAnnotatedAlternateAllele allele,
            ICompressedSequence sequence)
        {
            if (!LofteeUtilities.IsApplicable(ta)) return ta;

            var filters    = new HashSet<LofteeFilter.Filter>();
            var flags      = new HashSet<LofteeFilter.Flag>();
            var transcript = _transcriptsById[ta.TranscriptID];

            CheckSingleExon(transcript, flags);

            if (LofteeUtilities.IsInExon(ta))
            {
                CheckEndTruncation(ta, transcript, filters);
                CheckIncompleteCds();
                CheckNonCanonicalSpliceSurr(ta, transcript, filters, sequence);
            }

            var intronIdx = LofteeUtilities.GetIntronIndex(ta, transcript);

            if (LofteeUtilities.IsSpliceVariant(ta) && intronIdx != -1)
            {
                CheckSmallIntron(intronIdx, transcript, filters);
                CheckNonCanonicalSplice(intronIdx, transcript, filters, sequence);
                CheckNagnagSite(transcript, allele, flags, sequence);
            }

            return new LofteeTranscript(ta, filters, flags);
        }

        private void CheckNagnagSite(Transcript transcript, IAnnotatedAlternateAllele allele,
            HashSet<LofteeFilter.Flag> flags, ICompressedSequence sequence)
        {
            if (allele.ReferenceBegin == null || allele.ReferenceEnd == null ||
                allele.ReferenceBegin.Value != allele.ReferenceEnd.Value) return;

            int pos = allele.ReferenceBegin.Value;

            string upStreamSeq   = sequence.Substring(pos - 6, 6);
            string downStreamSeq = sequence.Substring(pos, 5);

            var combineSeq = transcript.Gene.OnReverseStrand
                ? SequenceUtilities.GetReverseComplement(upStreamSeq + downStreamSeq)
                : upStreamSeq + downStreamSeq;

            if (Regex.Match(combineSeq, "[A|T|C|G]AG[A|T|C|G]AG").Success)
                flags.Add(LofteeFilter.Flag.nagnag_site);
        }

        private void CheckNonCanonicalSplice(int intronIdx, Transcript transcript,
            HashSet<LofteeFilter.Filter> filters, ICompressedSequence sequence)
        {
            var intron          = transcript.Introns[intronIdx];
            var startNucleotide = sequence.Substring(intron.Start - 1, 2);
            var endNucleotide   = sequence.Substring(intron.End - 2, 2);
            var onReverseStrand = transcript.Gene.OnReverseStrand;

            if (!onReverseStrand && (startNucleotide != "GT" || endNucleotide != "AG"))
                filters.Add(LofteeFilter.Filter.non_can_splice);

            if (onReverseStrand && (startNucleotide != "CT" || endNucleotide != "AC"))
                filters.Add(LofteeFilter.Filter.non_can_splice);
        }

        private void CheckSmallIntron(int intronIdx, Transcript transcript, HashSet<LofteeFilter.Filter> filters)
        {
            if (transcript.Introns[intronIdx].End - transcript.Introns[intronIdx].Start + 1 < 15)
                filters.Add(LofteeFilter.Filter.small_intron);
        }

        private void CheckNonCanonicalSpliceSurr(IAnnotatedTranscript ta, Transcript transcript,
            HashSet<LofteeFilter.Filter> filters, ICompressedSequence sequence)
        {
            if (ta.Exons == null) return;
            int affectedExonIndex = Convert.ToInt32(ta.Exons.Split('/').First().Split('-').First());
            var totalExons = transcript.CdnaMaps.Length;

            string surrDonor = null;
            string surrAcceptor = null;

            if (totalExons <= 1) return;

            var onReverseStrand = transcript.Gene.OnReverseStrand;

            if (affectedExonIndex > 1)
            {
                var intron        = onReverseStrand ? transcript.Introns[totalExons - affectedExonIndex] : transcript.Introns[affectedExonIndex - 2];
                int acceptorStart = onReverseStrand ? intron.Start : intron.End - 1;
                var acceptorSeq   = sequence.Substring(acceptorStart - 1, 2);
                surrAcceptor      = onReverseStrand ? SequenceUtilities.GetReverseComplement(acceptorSeq) : acceptorSeq;
            }

            if (affectedExonIndex < totalExons)
            {
                var intron     = onReverseStrand ? transcript.Introns[totalExons - affectedExonIndex - 1] : transcript.Introns[affectedExonIndex - 1];
                int donorStart = onReverseStrand ? intron.End - 1 : intron.Start;
                var donorSeq   = sequence.Substring(donorStart - 1, 2);
                surrDonor      = onReverseStrand ? SequenceUtilities.GetReverseComplement(donorSeq) : donorSeq;
            }

            if (surrAcceptor != null && surrAcceptor != "AG" || surrDonor != null && surrDonor != "GT")
            {
                filters.Add(LofteeFilter.Filter.non_can_splice_surr);
            }                
        }

        private void CheckIncompleteCds() {}

        private void CheckSingleExon(Transcript transcript, HashSet<LofteeFilter.Flag> flags)
        {
            if (transcript.CdnaMaps.Length == 1) flags.Add(LofteeFilter.Flag.single_exon);
        }

        private void CheckEndTruncation(IAnnotatedTranscript ta, Transcript transcript, HashSet<LofteeFilter.Filter> filters)
        {
            if (!ta.Consequence.Contains("stop_gained") && !ta.Consequence.Contains("frameshift_variant")) return;

            var cdsPositions = ta.CdsPosition.Split('-');
            var startCdPos   = Convert.ToInt32(cdsPositions[0]);

            var cdsLen = CodingSequence.GetCodingSequenceLength(transcript.CdnaMaps,
                transcript.Translation.CodingRegion.GenomicStart, transcript.Translation.CodingRegion.GenomicEnd,
                transcript.StartExonPhase);

            if ((double)startCdPos / cdsLen > 0.95) filters.Add(LofteeFilter.Filter.end_trunc);
        }
    }
}