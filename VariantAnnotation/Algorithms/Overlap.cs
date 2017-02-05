using VariantAnnotation.Interface;

namespace VariantAnnotation.Algorithms
{
    public static class Overlap
    {
        /// <summary>
        /// returns true if this object overlaps with the interval defined by the
        /// endpoints.
        /// </summary>
        public static bool Partial(IInterval first, IInterval second)
        {
            return Partial(first.Start, first.End, second.Start, second.End);
        }

        public static bool Partial(int firstStart, int firstEnd, int secondStart, int secondEnd)
        {
            return secondEnd >= firstStart && secondStart <= firstEnd;
        }

        public static bool Complete(int firstStart, int firstEnd, int secondStart, int secondEnd)
        {
            return secondStart <= firstStart && secondEnd >= firstEnd;
        }
    }
}
