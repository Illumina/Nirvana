using VariantAnnotation.Interface.Providers;

namespace CommandLine.VersionProviders
{
    public sealed class DefaultVersionProvider : IVersionProvider
    {
        public string DataVersion { get; } = string.Empty;
    }
}
