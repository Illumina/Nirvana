using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;

namespace Piano
{
    public class PianoAnnotatedTranscript:IAnnotatedTranscript
    {
        public void SerializeJson(StringBuilder sb)
        {
            throw new System.NotImplementedException();
        }

        public ITranscript Transcript { get; }
        public string ReferenceAminoAcids { get; }
        public string AlternateAminoAcids { get; }
        public string ReferenceCodons { get; }
        public string AlternateCodons { get; }
        public IMappedPositions MappedPositions { get; }
        public string HgvsCoding { get; }
        public string HgvsProtein { get; }
        public PredictionScore Sift { get; }
        public PredictionScore PolyPhen { get; }
        public IEnumerable<ConsequenceTag> Consequences { get; }
        public IGeneFusionAnnotation GeneFusionAnnotation { get; }

        public string UpstreamAminoAcids { get; }
        public string DownStreamAminoAcids { get; }

        public PianoAnnotatedTranscript(ITranscript transcript, string referenceAminoAcids, string alternateAminoAcids, IMappedPositions mappedPositions, string upstreamAminoAcids,
            string downStreamAminoAcids,IEnumerable<ConsequenceTag> consequences)
        {
            Transcript           = transcript;
            ReferenceAminoAcids  = referenceAminoAcids;
            AlternateAminoAcids  = alternateAminoAcids;
            MappedPositions      = mappedPositions;
            UpstreamAminoAcids   = upstreamAminoAcids;
            DownStreamAminoAcids = downStreamAminoAcids;
            Consequences = consequences;
        }

        public override string ToString()
        {
            if (MappedPositions.ProteinInterval.Start == null || MappedPositions.ProteinInterval.End == null)
                return null;
            var geneId = Transcript.Source == Source.Ensembl
                ? Transcript.Gene.EnsemblId.ToString()
                : Transcript.Gene.EntrezGeneId.ToString();
            var downStreamAminoAcids = string.IsNullOrEmpty(DownStreamAminoAcids) ? "." : DownStreamAminoAcids;
            var upstreamAminoAcids = string.IsNullOrEmpty(UpstreamAminoAcids) ? "." : UpstreamAminoAcids;

            var line = Transcript.Gene.Symbol + "\t" + geneId + "\t" + CombineIdAndVersion(Transcript.Id,Transcript.Version) + "\t" +
                       CombineIdAndVersion(Transcript.Translation.ProteinId,Transcript.Translation.ProteinVersion) + "\t" +
                       GetNullablePositionRange(MappedPositions.ProteinInterval) + "\t" + upstreamAminoAcids + "\t" +
                       GetAlleleString(ReferenceAminoAcids, AlternateAminoAcids) + "\t" + downStreamAminoAcids+"\t"+string.Join(',', Consequences?.Select(ConsequenceUtil.GetConsequence));
            return line;
        }

        private static string GetAlleleString(string a, string b)
        {
            return a == b ? a : $"{(string.IsNullOrEmpty(a) ? "-" : a)}/{(string.IsNullOrEmpty(b) ? "-" : b)}";
        }

        private static string CombineIdAndVersion(ICompactId id, byte version) => id + "." + version;

        private static string GetNullablePositionRange(NullableInterval interval)
        {
            if (interval.Start == null && interval.End == null) return null;
            if (interval.Start == null) return "?-" + interval.End.Value;
            if (interval.End == null) return interval.Start.Value + "-?";
            var start = interval.Start.Value;
            var end = interval.End.Value;
            if (start > end) Swap.Int(ref start, ref end);
            return start == end ? start.ToString(CultureInfo.InvariantCulture) : start + "-" + end;
        }
    }
}