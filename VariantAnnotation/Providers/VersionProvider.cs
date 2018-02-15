using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.SA;
using VariantAnnotation.Sequence;

namespace VariantAnnotation.Providers
{
    public sealed class VersionProvider : IVersionProvider
    {
        public string DataVersion { get; } = $"Cache version: {CacheConstants.DataVersion}, Supplementary annotation version: {SaDataBaseCommon.DataVersion}, Reference version: {CompressedSequenceCommon.HeaderVersion}";
    }
}