namespace OptimizedCore
{
    public static class NullSequenceEqual
    {
        public static bool ArrayEqual<T>(this T[] first, T[] second)
        {
            if (ReferenceEquals(first, second)) return true;
            if (first == null || second == null) return false;

            if (first.Length != second.Length) return false;

            for (var i = 0; i < first.Length; i++)
                if (!first[i].Equals(second[i]))
                    return false;

            return true;
        }
    }
}