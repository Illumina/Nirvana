using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Caches.Utilities
{
    public static class ExonUtilities
    {
        public static int GetTotalExonLength(ITranscriptRegion[] regions)
        {
            int totalExonLength = 0;

            foreach (var region in regions)
            {
                if (region.Type != TranscriptRegionType.Exon) continue;
                totalExonLength += region.End - region.Start + 1;
            }

            return totalExonLength;
        }
    }
}