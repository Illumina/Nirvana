using System.Collections.Generic;
using System.IO;

namespace Cache.Utilities;

public static class OutputUtilities
{
    public static int GetIndex<T>(T? item, Dictionary<T, int> indices) where T : notnull
    {
        if (item == null) throw new InvalidDataException("Tried to write the index of a null item");
        if (!indices.TryGetValue(item, out int index))
            throw new InvalidDataException($"Unable to locate the {typeof(T)} in the indices: {item}");
        return index;
    }
}