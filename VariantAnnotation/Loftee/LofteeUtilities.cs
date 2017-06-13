using System;
using System.Linq;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.Interface;

namespace VariantAnnotation.Loftee
{
    public static class LofteeUtilities
    {
        public static bool IsInExon(IAnnotatedTranscript ta) => !string.IsNullOrEmpty(ta.Exons);

        public static bool IsSpliceVariant(IAnnotatedTranscript ta) => IsSpliceDonor(ta) || IsSpliceAcceptor(ta);

        private static bool IsSpliceDonor(IAnnotatedTranscript ta) => ta.Consequence.Contains("splice_donor_variant");

        private static bool IsSpliceAcceptor(IAnnotatedTranscript ta) => ta.Consequence.Contains("splice_acceptor_variant");

        public static bool IsApplicable(IAnnotatedTranscript ta)
        {
            return ta.Consequence.Contains("stop_gained") || ta.Consequence.Contains("frameshift_variant") ||
                   ta.Consequence.Contains("splice_donor_variant") || ta.Consequence.Contains("splice_acceptor_variant");
        }

        public static int GetIntronIndex(IAnnotatedTranscript ta, Transcript transcript)
        {
            if (ta.Introns == null) return -1;
            int affectedIntron = Convert.ToInt32(ta.Introns.Split('/').First().Split('-').First());
            var totalIntrons = transcript.Introns.Length;
            return transcript.Gene.OnReverseStrand ? totalIntrons - affectedIntron : affectedIntron - 1;
        }
    }
}