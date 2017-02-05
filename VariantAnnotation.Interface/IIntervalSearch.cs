using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface IIntervalSearch<T>
    {
        bool OverlapsAny(int begin, int end);
        void GetAllOverlappingValues(int begin, int end, List<T> values);
    }
}
