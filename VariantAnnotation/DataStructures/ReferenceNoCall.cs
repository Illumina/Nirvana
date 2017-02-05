using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures
{
    public static class ReferenceNoCall
    {
        /// <summary>
        /// checks if the variant is a reference no-call and sets the flag accordingly
        /// </summary>
        public static void Check(VariantFeature variant, bool limitToTranscript,
            IIntervalForest<Transcript> transcriptIntervals)
        {
            // make sure we enabled reference no-call checking and that this is a reference site
            if (!variant.IsReference) return;

            // make sure the filters failed
            if (variant.PassFilter()) return;

            if (!limitToTranscript)
            {
                variant.IsRefNoCall = true;
                return;
            }

            // check if the variant overlaps any transcripts
            variant.IsRefNoCall = transcriptIntervals.OverlapsAny(variant.ReferenceIndex, variant.OverlapReferenceBegin,
                variant.OverlapReferenceEnd);
        }
    }
}
