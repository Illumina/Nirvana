using System.Collections.Generic;

namespace CommonUtilities
{
    public static class StringExtensions
    {
        public static List<int> AllIndicesOf(this string str, char separator)
        {
            var positions = new List<int>();

            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] == separator) positions.Add(i);
            }

            return positions;
        }

        public static string GetSlice(this string str, int index, List<int> sliceIndices)
        {
            if (sliceIndices == null || sliceIndices.Count==0) return str;
            if (index > sliceIndices.Count) return null;
            return index == sliceIndices.Count - 1
                ? str.Substring(sliceIndices[index])
                : str.Substring(sliceIndices[index], sliceIndices[index + 1] - sliceIndices[index]);

        }
    }
}