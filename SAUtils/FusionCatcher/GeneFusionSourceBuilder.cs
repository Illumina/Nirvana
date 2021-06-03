using System.Collections.Generic;
using VariantAnnotation.GeneFusions.SA;

namespace SAUtils.FusionCatcher
{
    public sealed class GeneFusionSourceBuilder
    {
        public          bool                   IsPseudogenePair;
        public          bool                   IsParalogPair;
        public          bool                   IsReadthrough;
        public readonly List<GeneFusionSource> GermlineSources = new();
        public readonly List<GeneFusionSource> SomaticSources  = new();

        public GeneFusionSourceCollection Create()
        {
            GeneFusionSource[] germlineSources = GermlineSources.Count > 0 ? GermlineSources.ToArray() : null;
            GeneFusionSource[] somaticSources  = SomaticSources.Count  > 0 ? SomaticSources.ToArray() : null;
            return new GeneFusionSourceCollection(IsPseudogenePair, IsParalogPair, IsReadthrough, germlineSources, somaticSources);
        }
    }
}