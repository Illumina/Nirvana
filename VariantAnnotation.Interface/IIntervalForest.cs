using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface IIntervalForest<T>
    {
        bool OverlapsAny(ushort refIndex, int begin, int end);
        void GetAllOverlappingValues(ushort refIndex, int begin, int end, List<T> values);
    }
}
