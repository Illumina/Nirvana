using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Cache.Data;
using JSON;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedTranscript : IJsonSerializer
    {
        public Transcript     Transcript          { get; }
        public string          ReferenceAminoAcids { get; }
        public string          AlternateAminoAcids { get; }
        public string          ReferenceCodons     { get; }
        public string          AlternateCodons     { get; }
        public IMappedPosition MappedPosition      { get; }
        public string          HgvsCoding          { get; }
        public string          HgvsProtein         { get; }
        public PredictionScore Sift                { get; private set;}
        public PredictionScore PolyPhen            { get; private set;}

        public IEnumerable<ConsequenceTag> Consequences { get; }
        public IGeneFusionAnnotation GeneFusionAnnotation { get; }
        public bool CompleteOverlap { get; }

        public AnnotatedTranscript(Transcript transcript, string referenceAminoAcids, string alternateAminoAcids,
            string referenceCodons, string alternateCodons, IMappedPosition mappedPosition, string hgvsCoding,
            string hgvsProtein, IEnumerable<ConsequenceTag> consequences, IGeneFusionAnnotation geneFusionAnnotation,
            bool completeOverlap)
        {
            Transcript           = transcript;
            ReferenceAminoAcids  = referenceAminoAcids;
            AlternateAminoAcids  = alternateAminoAcids;
            ReferenceCodons      = referenceCodons;
            AlternateCodons      = alternateCodons;
            MappedPosition       = mappedPosition;
            HgvsCoding           = hgvsCoding;
            HgvsProtein          = hgvsProtein;
            Consequences         = consequences;
            GeneFusionAnnotation = geneFusionAnnotation;
            CompleteOverlap      = completeOverlap;
        }

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("transcript", Transcript.Id);
            jsonObject.AddStringValue("source", Transcript.Source.ToString());
            if (!CompleteOverlap) jsonObject.AddStringValue("bioType", Transcript.BioType.ToString());
            jsonObject.AddStringValue("codons", GetCodonString(ReferenceCodons, AlternateCodons));
            jsonObject.AddStringValue("aminoAcids", GetAminoAcidString(ReferenceAminoAcids, AlternateAminoAcids));

            if (MappedPosition != null)
            {
                var numExons   = (int) ((Transcript.TranscriptRegions.Length + 1) / 2.0);
                int numIntrons = numExons - 1;
                
                jsonObject.AddStringValue("cdnaPos",    GetRangeString(MappedPosition.CoveredCdnaStart, MappedPosition.CoveredCdnaEnd));
                jsonObject.AddStringValue("cdsPos",     GetRangeString(MappedPosition.CoveredCdsStart, MappedPosition.CoveredCdsEnd));
                jsonObject.AddStringValue("exons",      GetFractionString(MappedPosition.ExonStart,   MappedPosition.ExonEnd, numExons));
                jsonObject.AddStringValue("introns",    GetFractionString(MappedPosition.IntronStart, MappedPosition.IntronEnd, numIntrons));
                jsonObject.AddStringValue("proteinPos", GetRangeString(MappedPosition.CoveredProteinStart, MappedPosition.CoveredProteinEnd));
            }
            
            string geneId = Transcript.Source == Source.Ensembl
                ? Transcript.Gene.EnsemblId
                : Transcript.Gene.NcbiGeneId;

            if (!CompleteOverlap) jsonObject.AddStringValue("geneId", geneId);
            jsonObject.AddStringValue("hgnc", Transcript.Gene.Symbol);
            jsonObject.AddStringValues("consequence", Consequences?.Select(ConsequenceUtil.GetConsequence));
            jsonObject.AddStringValue("hgvsc", HgvsCoding);
            jsonObject.AddStringValue("hgvsp", HgvsProtein);
            jsonObject.AddStringValue("geneFusion", GeneFusionAnnotation?.ToString(), false);

            jsonObject.AddBoolValue("isCanonical", Transcript.IsCanonical);

            jsonObject.AddDoubleValue("polyPhenScore", PolyPhen?.Score);

            jsonObject.AddStringValue("polyPhenPrediction", PolyPhen?.Prediction);
            if (!CompleteOverlap && Transcript.CodingRegion != null) jsonObject.AddStringValue("proteinId", Transcript.CodingRegion.ProteinId);

            jsonObject.AddDoubleValue("siftScore", Sift?.Score);

            jsonObject.AddStringValue("siftPrediction", Sift?.Prediction);

            jsonObject.AddBoolValue("completeOverlap", CompleteOverlap);

            sb.Append(JsonObject.CloseBrace);
        }

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
            if (start == -1) return "?-"  + end;
            if (end   == -1) return start + "-?";
            if (start > end) (start, end) = (end, start);
            return start == end ? start.ToString(CultureInfo.InvariantCulture) : start + "-" + end;
        }

        private static string GetFractionString(int start, int end, int total)
        {
            if (start == -1 && end == -1) return null;
            return GetRangeString(start, end) + "/" + total;
        }

        public void AddSift(PredictionScore score) => Sift = score;
        public void AddPolyPhen(PredictionScore score) => PolyPhen = score;
    }
}