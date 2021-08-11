using Microsoft.Extensions.ObjectPool;
using VariantAnnotation.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.Pools
{
    public static class AnnotatedVariantPool
    {
        private static readonly ObjectPool<AnnotatedVariant> Pool 
            = new DefaultObjectPool<AnnotatedVariant>(new DefaultPooledObjectPolicy<AnnotatedVariant>(), 8);
        
        public static AnnotatedVariant Get(IVariant variant)
        {
            var annotatedVariant =  Pool.Get();
            annotatedVariant.Initialize(variant);
            return annotatedVariant;
        }
        
        public static void Return(AnnotatedVariant av) => Pool.Return(av);
    }
}