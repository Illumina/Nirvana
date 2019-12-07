using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.SA;
using VariantAnnotation.Sequence;

namespace VariantAnnotation.Providers
{
    public sealed class VersionProvider : IVersionProvider
    {
        public string DataVersion { get; } = $"Cache version: {CacheConstants.DataVersion}, Supplementary annotation version: {SaCommon.DataVersion}, Reference version: {CompressedSequenceCommon.HeaderVersion}";
    }
}