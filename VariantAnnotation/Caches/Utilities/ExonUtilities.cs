using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Caches.Utilities
{
    public static class ExonUtilities
    {
        /// <summary>
        /// returns the total exon length
        /// </summary>
        public static int GetTotalExonLength(ICdnaCoordinateMap[] maps)
        {
            int totalExonLength = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int mapIndex = 0; mapIndex < maps.Length; mapIndex++)
            {
                var cdnaMap = maps[mapIndex];
                totalExonLength += cdnaMap.End - cdnaMap.Start + 1;
            }

            return totalExonLength;
        }
    }
}