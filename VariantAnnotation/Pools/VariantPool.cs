using Genome;
using Microsoft.Extensions.ObjectPool;
using Variants;

namespace VariantAnnotation.Pools
{
    public static class VariantPool
    {
        private static readonly ObjectPool<Variant> Pool = 
            new DefaultObjectPool<Variant>(new DefaultPooledObjectPolicy<Variant>(), 8);
        
        public static Variant Get(Chromosome chromosome, int start, int end, string refAllele, string altAllele,
            VariantType variantType, string variantId, bool isRefMinor, bool isDecomposed, bool isRecomposed,
            string[] linkedVids, AnnotationBehavior behavior, bool isStructuralVariant)
        {
            var variant =  Pool.Get();
            variant.Initialize( chromosome,  start,  end,  refAllele,  altAllele,
                 variantType,  variantId,  isRefMinor,  isDecomposed,  isRecomposed,
                 linkedVids,  behavior, isStructuralVariant);
            return variant;
        }
        
        public static void Return(Variant variant) => Pool.Return(variant);
    }
}