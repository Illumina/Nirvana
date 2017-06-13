using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.Annotation;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.DataStructures.Variants;
using VariantAnnotation.FileHandling.VCF;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace Piano
{
    public sealed class PianoVariant
    {
        #region members
        // Piano variant is associated with a variant object from the vcf file
        public string ReferenceName { get; }
        public int? ReferenceBegin { get; }
        public string ReferenceAllele { get; }
        public IEnumerable<string> AlternateAlleles { get; }
        public readonly List<PianoAllele> PianoAlleles;
        private PianoAllele _currPianoAllele;
        private PianoAllele.Transcript _currTranscript;

        #endregion


        public PianoVariant(VariantFeature variant)
        {
            ReferenceName = variant.ReferenceName;
            ReferenceBegin = variant.VcfReferenceBegin;
            ReferenceAllele = variant.VcfColumns[VcfCommon.RefIndex].ToUpperInvariant();
            AlternateAlleles = variant.AlternateAlleles[0].NirvanaVariantType == VariantType.translocation_breakend
                                        ? variant.VcfColumns[VcfCommon.AltIndex].Split(',')
                                        : variant.VcfColumns[VcfCommon.AltIndex].ToUpperInvariant().Split(',');

            PianoAlleles = new List<PianoAllele>();

        }

        public void AddVariantData(VariantFeature variant)
        {
            foreach (var altAllele in variant.AlternateAlleles)
            {
                var pianoAllele = new PianoAllele(altAllele);
                PianoAlleles.Add(pianoAllele);
            }
        }

        public void CreateAnnotationObject(Transcript transcript, VariantAlternateAllele altAllele)
        {
            FindCorrespondingJsonVariant(altAllele);

            if (_currPianoAllele == null)
            {
                throw new GeneralException("Cannot find jsonVariant corresponding to alternate allele");
            }

            _currTranscript = new PianoAllele.Transcript
            {
                IsCanonical = transcript.IsCanonical ? "true" : null,
                TranscriptID = transcript.Id.ToString(),
                BioType = BioTypeUtilities.GetBiotypeDescription(transcript.BioType),
                Gene = transcript.TranscriptSource == TranscriptDataSource.Ensembl ? transcript.Gene.EnsemblId.ToString() : transcript.Gene.EntrezGeneId.ToString(),
                Hgnc = transcript.Gene.Symbol
            };
        }

        private void FindCorrespondingJsonVariant(VariantAlternateAllele altAllele)
        {
            _currPianoAllele = null;
            foreach (var pianoAllele in PianoAlleles)
            {
                if (pianoAllele.ReferenceBegin != altAllele.Start) continue;
                if (pianoAllele.SaAltAllele != altAllele.SuppAltAllele) continue;

                _currPianoAllele = pianoAllele;
            }
        }

        public void FinalizeAndAddAnnotationObject(Transcript transcript, TranscriptAnnotation ta, string[] consequences)
        {
            if (ta.AlternateAllele.IsStructuralVariant) return;

            if (ta.HasValidCdnaCodingStart) _currTranscript.ProteinID = transcript.Translation.ProteinId.ToString();
            _currTranscript.Consequence = consequences;

            if (ta.HasValidCdsStart || ta.HasValidCdsEnd)
            {
                _currTranscript.ProteinPosition = GetProtRangeString(ta);
                _currTranscript.UpStreamPeptides = GetFlankingPeptides(ta, transcript, 15, true);


                _currTranscript.AminoAcids = GetAlleleString(ta.ReferenceAminoAcids, ta.AlternateAminoAcids);
                if (!ta.HasFrameShift)
                    _currTranscript.DownStreamPeptides = GetFlankingPeptides(ta, transcript, 15, false);
            }

            _currPianoAllele.Transcripts.Add(_currTranscript);
        }

        /// <summary>
        /// returns an allele string representation of two alleles
        /// </summary>
        private static string GetAlleleString(string a, string b)
        {
            //trim alternateAminoAcides
            var altAa = b;
            b = altAa == null || !altAa.Contains("*") ? altAa : altAa.Split('*')[0] + "*";

            return a == b ? a : $"{(string.IsNullOrEmpty(a) ? "-" : a)}/{(string.IsNullOrEmpty(b) ? "-" : b)}";
        }

        private string GetFlankingPeptides(TranscriptAnnotation ta, Transcript transcript, int nBase, bool upStrem)
        {

            var begin = ta.ProteinBegin;
            var end = ta.ProteinEnd;

            if (!ta.HasValidCdsStart && !ta.HasValidCdsEnd) return null;
            if (!ta.HasValidCdsStart && ta.HasValidCdsEnd) begin = end;
            if (!ta.HasValidCdsEnd && ta.HasValidCdsStart) end = begin;

            if (upStrem)
            {
                var peptideStart = Math.Max(1, begin - nBase);
                return transcript.Translation.PeptideSeq.Substring(peptideStart - 1, (begin - peptideStart));
            }

            var peptideEnd = Math.Min(transcript.Translation.PeptideSeq.Length, end + nBase);
            return peptideEnd > end + 1 ? transcript.Translation.PeptideSeq.Substring(end, (peptideEnd - end)) : "";
        }

        /// <summary>
        /// returns a range string representation of two integers
        /// </summary>
        private static string GetProtRangeString(TranscriptAnnotation ta)
        {
            if (!ta.HasValidCdsStart && !ta.HasValidCdsEnd) return "";
            if (!ta.HasValidCdsStart && ta.HasValidCdsEnd) return "?-" + ta.ProteinEnd;
            if (!ta.HasValidCdsEnd && ta.HasValidCdsStart) return ta.ProteinBegin + "-?";

            var begin = ta.ProteinBegin;
            var end = ta.ProteinEnd;

            if (end < begin) Swap.Int(ref begin, ref end);

            return begin == end ? begin.ToString(CultureInfo.InvariantCulture) : $"{begin}-{end}";
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var altAllele in PianoAlleles)
            {
                var refAllele = string.IsNullOrEmpty(altAllele.RefAllele) ? "-" : altAllele.RefAllele;
                var alternateAllele = string.IsNullOrEmpty(altAllele.AltAllele) ? "-" : altAllele.AltAllele;
                foreach (var transcript in altAllele.Transcripts)
                {
                    if (transcript.ProteinPosition == null) continue;

                    sb.Append(ReferenceName + "\t" + altAllele.ReferenceBegin + "\t" + refAllele + "\t" + alternateAllele +
                              "\t" + transcript.Hgnc + "\t" + transcript.Gene + "\t" + transcript.TranscriptID + "\t" +
                              transcript.ProteinID + "\t" + transcript.ProteinPosition);
                    //+"\t"+transcript.UpStreamPeptides + "\t" + transcript.AminoAcids + "\t"+transcript.DownStreamPeptides +"\t"+string.Join(",",transcript.Consequence) +"\n");
                    AddField(transcript.UpStreamPeptides, sb);
                    AddField(transcript.AminoAcids, sb);
                    AddField(transcript.DownStreamPeptides, sb);
                    AddField(string.Join(",", transcript.Consequence), sb);
                    sb.Append("\n");

                }
            }

            return sb.ToString();
        }

        private void AddField(string info, StringBuilder sb)
        {
            var addedString = string.IsNullOrEmpty(info) ? "." : info;
            sb.Append("\t" + addedString);
        }
    }
}