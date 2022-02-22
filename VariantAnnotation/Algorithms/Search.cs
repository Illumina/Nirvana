using System;
using System.Collections.Generic;

namespace VariantAnnotation.Algorithms
{
    public static class Search
    {
        public static int BinarySearch<T, K>(List<T> items, K value) where T:IComparable<K>
        {
            var begin = 0;
            int end   = items.Count - 1;

            while (begin <= end)
            {
                int index = begin + (end - begin >> 1);

                int ret = items[index].CompareTo(value);
                if (ret == 0) return index;
                if (ret < 0) begin = index + 1;
                else end           = index - 1;
            }

            return ~begin;
        }
    }
}