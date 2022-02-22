using System.Linq;

namespace Cache.Utilities;

public static class EqualityUtilities
{
    public static bool ArrayEquals<T>(T[]? a, T[]? b) =>
        a == null && b == null || a != null && b != null && a.SequenceEqual(b);
}