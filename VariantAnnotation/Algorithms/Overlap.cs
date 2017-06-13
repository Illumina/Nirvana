namespace VariantAnnotation.Algorithms
{
    public static class Overlap
    {
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
