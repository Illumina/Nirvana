using Microsoft.Extensions.ObjectPool;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.Pools
{
    public static class AnnotatedPositionPool
    {
        private static readonly ObjectPool<AnnotatedPosition> Pool 
            = new DefaultObjectPool<AnnotatedPosition>(new DefaultPooledObjectPolicy<AnnotatedPosition>(), 4);
        
        public static AnnotatedPosition Get(IPosition position, IAnnotatedVariant[] annotatedVariants)
        {
            var annotatedPosition =  Pool.Get();
            annotatedPosition.Initialize(position, annotatedVariants);
            return annotatedPosition;
        }
        
        public static void Return(AnnotatedPosition ap) => Pool.Return(ap);
    }
}