﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VariantAnnotation.Algorithms;
using VariantAnnotation.GeneFusions.SA;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class AnnotatedTranscript : IAnnotatedTranscript
    {
        public ITranscript          Transcript          { get; private set; }
        public string               ReferenceAminoAcids { get; private set;}
        public string               AlternateAminoAcids { get; private set;}
        public string               ReferenceCodons     { get; private set;}
        public string               AlternateCodons     { get; private set;}
        public IMappedPosition      MappedPosition      { get; private set;}
        public string               HgvsCoding          { get; private set;}
        public string               HgvsProtein         { get; private set;}
        public PredictionScore      Sift                { get; private set;}
        public PredictionScore      PolyPhen            { get; private set;}
        public List<ConsequenceTag> Consequences        { get; private set;}
        public bool?                CompleteOverlap     { get; private set;}
        public List<double>         ConservationScores  { get; set; }

        private List<IAnnotatedGeneFusion> _geneFusions;
        
        public void Initialize(ITranscript transcript, string referenceAminoAcids, string alternateAminoAcids,
            string referenceCodons, string alternateCodons, IMappedPosition mappedPosition, string hgvsCoding,
            string hgvsProtein, PredictionScore sift, PredictionScore polyphen,
            List<ConsequenceTag> consequences, bool? completeOverlap)
        {
            Transcript          = transcript;
            ReferenceAminoAcids = referenceAminoAcids;
            AlternateAminoAcids = alternateAminoAcids;
            ReferenceCodons     = referenceCodons;
            AlternateCodons     = alternateCodons;
            MappedPosition      = mappedPosition;
            HgvsCoding          = hgvsCoding;
            HgvsProtein         = hgvsProtein;
            Sift                = sift;
            PolyPhen            = polyphen;
            Consequences        = consequences;
            CompleteOverlap     = completeOverlap;
            _geneFusions        = null;
        }
        
        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("transcript", Transcript.Id.WithVersion);
            jsonObject.AddStringValue("source", Transcript.Source.ToString());
            if (CompleteOverlap.HasValue && !CompleteOverlap.Value) jsonObject.AddStringValue("bioType", GetBioType(Transcript.BioType));
            jsonObject.AddStringValue("codons", GetCodonString(ReferenceCodons, AlternateCodons));
            jsonObject.AddStringValue("aminoAcids", GetAminoAcidString(ReferenceAminoAcids, AlternateAminoAcids));

            if (MappedPosition != null)
            {
                jsonObject.AddStringValue("cdnaPos",    GetRangeString(MappedPosition.CoveredCdnaStart, MappedPosition.CoveredCdnaEnd));
                jsonObject.AddStringValue("cdsPos",     GetRangeString(MappedPosition.CoveredCdsStart, MappedPosition.CoveredCdsEnd));
                jsonObject.AddStringValue("exons",      GetFractionString(MappedPosition.ExonStart,   MappedPosition.ExonEnd, Transcript.NumExons));
                jsonObject.AddStringValue("introns",    GetFractionString(MappedPosition.IntronStart, MappedPosition.IntronEnd, Transcript.NumExons - 1));
                jsonObject.AddStringValue("proteinPos", GetRangeString(MappedPosition.CoveredProteinStart, MappedPosition.CoveredProteinEnd));
            }

            string geneId = Transcript.Source == Source.Ensembl
                ? Transcript.Gene.EnsemblId.ToString()
                : Transcript.Gene.EntrezGeneId.ToString();

            if (CompleteOverlap.HasValue &&!CompleteOverlap.Value) jsonObject.AddStringValue("geneId", geneId);
            jsonObject.AddStringValue("hgnc", Transcript.Gene.Symbol);

            if (Consequences != null) AddConsequences(jsonObject);
            jsonObject.AddStringValue("hgvsc", HgvsCoding);
            jsonObject.AddStringValue("hgvsp", HgvsProtein);

            if (_geneFusions != null) jsonObject.AddObjectValues("geneFusions", _geneFusions);

            jsonObject.AddBoolValue("isCanonical", Transcript.IsCanonical);

            jsonObject.AddDoubleValue("polyPhenScore", PolyPhen?.Score);

            jsonObject.AddStringValue("polyPhenPrediction", PolyPhen?.Prediction);
            if (CompleteOverlap.HasValue && !CompleteOverlap.Value && Transcript.Translation != null) jsonObject.AddStringValue("proteinId", Transcript.Translation.ProteinId.WithVersion);

            jsonObject.AddDoubleValue("siftScore", Sift?.Score);

            jsonObject.AddStringValue("siftPrediction", Sift?.Prediction);

            if (ConservationScores != null && ConservationScores.Count > 0)
            {
                jsonObject.AddObjectValue("aminoAcidConservation", new AnnotatedConservationScore(ConservationScores));
            }

            if (CompleteOverlap.HasValue) jsonObject.AddBoolValue("completeOverlap", CompleteOverlap.Value);

            sb.Append(JsonObject.CloseBrace);
        }

        private void AddConsequences(JsonObject jsonObject)
        {
            jsonObject.AddStringValues("consequence", Consequences?.Select(ConsequenceUtil.GetConsequence));
        }

        public static string GetBioType(BioType bioType) => bioType == BioType.three_prime_overlapping_ncRNA
            ? "3prime_overlapping_ncRNA"
            : bioType.ToString();

        private static string GetAminoAcidString(string a, string b)
        {
            if (a == b) return a;
            a = string.IsNullOrEmpty(a) ? "-" : a;
            b = string.IsNullOrEmpty(b) ? "-" : b;
            return $"{a}/{b}";
        }

        private static string GetCodonString(string a, string b)
        {
            if (a == b && string.IsNullOrEmpty(a)) return a;
            a = string.IsNullOrEmpty(a) ? "-" : a;
            b = string.IsNullOrEmpty(b) ? "-" : b;
            return $"{a}/{b}";
        }

        private static string GetRangeString(int start, int end)
        {
            if (start == -1 && end == -1) return null;
            if (start == -1) return "?-" + end;
            if (end == -1) return start + "-?";
            if (start > end) Swap.Int(ref start, ref end);
            return start == end ? start.ToString(CultureInfo.InvariantCulture) : start + "-" + end;
        }

        private static string GetFractionString(int start, int end, int total)
        {
            if (start == -1 && end == -1) return null;
            return GetRangeString(start, end) + "/" + total;
        }

        public void AddGeneFusions(IAnnotatedGeneFusion[] geneFusions)
        {
            _geneFusions ??= new List<IAnnotatedGeneFusion>();
            _geneFusions.AddRange(geneFusions);
            Consequences.Add(ConsequenceTag.unidirectional_gene_fusion);
        }

        public void AddGeneFusionPairs(HashSet<IGeneFusionPair> fusionPairs)
        {
            if (_geneFusions == null) return;
            foreach (IAnnotatedGeneFusion gf in _geneFusions)
                fusionPairs.Add(new GeneFusionPair(gf.FusionKey, gf.FirstGeneSymbol, gf.FirstGeneKey, gf.SecondGeneSymbol, gf.SecondGeneKey));
        }
    }
}