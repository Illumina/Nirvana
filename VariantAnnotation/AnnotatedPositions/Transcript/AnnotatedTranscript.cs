using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class AnnotatedTranscript : IAnnotatedTranscript
    {
        public ITranscript Transcript { get; }
        public string ReferenceAminoAcids { get; }
        public string AlternateAminoAcids { get; }
        public string ReferenceCodons { get; }
        public string AlternateCodons { get; }
        public IMappedPosition MappedPosition { get; }
        public string HgvsCoding { get; }
        public string HgvsProtein { get; }
        public PredictionScore Sift { get; }
        public PredictionScore PolyPhen { get; }
        public IEnumerable<ConsequenceTag> Consequences { get; }
        public IGeneFusionAnnotation GeneFusionAnnotation { get; }
        public IList<IPluginData> PluginData { get; }
        public bool? CompleteOverlap { get; }

        public AnnotatedTranscript(ITranscript transcript, string referenceAminoAcids, string alternateAminoAcids,
            string referenceCodons, string alternateCodons, IMappedPosition mappedPosition, string hgvsCoding,
            string hgvsProtein, PredictionScore sift, PredictionScore polyphen,
            IEnumerable<ConsequenceTag> consequences, IGeneFusionAnnotation geneFusionAnnotation, bool? completeOverlap)
        {
            Transcript           = transcript;
            ReferenceAminoAcids  = referenceAminoAcids;
            AlternateAminoAcids  = alternateAminoAcids;
            ReferenceCodons      = referenceCodons;
            AlternateCodons      = alternateCodons;
            MappedPosition       = mappedPosition;
            HgvsCoding           = hgvsCoding;
            HgvsProtein          = hgvsProtein;
            Sift                 = sift;
            PolyPhen             = polyphen;
            Consequences         = consequences;
            GeneFusionAnnotation = geneFusionAnnotation;
            PluginData           = new List<IPluginData>();
            CompleteOverlap      = completeOverlap;
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
                jsonObject.AddStringValue("cdnaPos",    GetRangeString(MappedPosition.CdnaStart,      MappedPosition.CdnaEnd));
                jsonObject.AddStringValue("cdsPos",     GetRangeString(MappedPosition.CdsStart,       MappedPosition.CdsEnd));
                jsonObject.AddStringValue("exons",      GetFractionString(MappedPosition.ExonStart,   MappedPosition.ExonEnd, Transcript.NumExons));
                jsonObject.AddStringValue("introns",    GetFractionString(MappedPosition.IntronStart, MappedPosition.IntronEnd, Transcript.NumExons - 1));
                jsonObject.AddStringValue("proteinPos", GetRangeString(MappedPosition.ProteinStart,   MappedPosition.ProteinEnd));
            }

            var geneId = Transcript.Source == Source.Ensembl
                ? Transcript.Gene.EnsemblId.ToString()
                : Transcript.Gene.EntrezGeneId.ToString();

            if (CompleteOverlap.HasValue &&!CompleteOverlap.Value) jsonObject.AddStringValue("geneId", geneId);
            jsonObject.AddStringValue("hgnc", Transcript.Gene.Symbol);
            jsonObject.AddStringValues("consequence", Consequences?.Select(ConsequenceUtil.GetConsequence));
            jsonObject.AddStringValue("hgvsc", HgvsCoding);
            jsonObject.AddStringValue("hgvsp", HgvsProtein);
            jsonObject.AddStringValue("geneFusion", GeneFusionAnnotation?.ToString(), false);

            jsonObject.AddBoolValue("isCanonical", Transcript.IsCanonical);

            jsonObject.AddDoubleValue("polyPhenScore", PolyPhen?.Score);

            jsonObject.AddStringValue("polyPhenPrediction", PolyPhen?.Prediction);
            if (CompleteOverlap.HasValue && !CompleteOverlap.Value && Transcript.Translation != null) jsonObject.AddStringValue("proteinId", Transcript.Translation.ProteinId.WithVersion);

            jsonObject.AddDoubleValue("siftScore", Sift?.Score);

            jsonObject.AddStringValue("siftPrediction", Sift?.Prediction);

            if (PluginData != null)
                foreach (var pluginData in PluginData)
                {
                    jsonObject.AddStringValue(pluginData.Name, pluginData.GetJsonString(), false);
                }

            if (CompleteOverlap.HasValue) jsonObject.AddBoolValue("completeOverlap", CompleteOverlap.Value);

            sb.Append(JsonObject.CloseBrace);
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
    }
}