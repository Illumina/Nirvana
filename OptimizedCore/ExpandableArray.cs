using System.Buffers;

namespace OptimizedCore
{
    public static class ExpandableArray<T>
    {
        public static T[] Get(int size)
        {
            var pool = ArrayPool<T>.Shared;
            return pool.Rent(size);
        }

        public static T[] Resize(T[] array, int newSize)
        {
            var pool = ArrayPool<T>.Shared;
            pool.Return(array);

            return pool.Rent(newSize);
        }

        public static void Return(T[] array)
        {
            var pool = ArrayPool<T>.Shared;
            pool.Return(array);
        }
    }
}