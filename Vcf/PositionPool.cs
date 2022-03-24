using Genome;
using Microsoft.Extensions.ObjectPool;
using VariantAnnotation.Interface.Positions;
using Variants;

namespace Vcf
{
    public static class PositionPool
    {
        private static readonly ObjectPool<Position> Pool = new DefaultObjectPool<Position>(new DefaultPooledObjectPolicy<Position>(), 4);
                
        public static Position Get(Chromosome chromosome, int start, int end, string refAllele, string[] altAlleles,
            double? quality, string[] filters, IVariant[] variants, ISample[] samples, IInfoData infoData,
            string[] vcfFields, bool[] isDecomposed, bool isRecomposed)
        {
            var position =  Pool.Get();
            position.Initialize( chromosome,  start, end, refAllele, altAlleles,
                quality, filters, variants, samples,infoData,
                vcfFields, isDecomposed, isRecomposed);
            return position;
        }
        
        public static void Return(Position position) => Pool.Return(position);
    }
}