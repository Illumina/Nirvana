using System;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO.Caches;

namespace CacheUtils.Utilities
{
    public static class HeaderUtilities
    {
        public static Header GetHeader(Source source, GenomeAssembly genomeAssembly) => new Header(
            CacheConstants.Identifier, CacheConstants.SchemaVersion, CacheConstants.DataVersion, source,
            DateTime.Now.Ticks, genomeAssembly);
    }
}
