using CommandLine.Utilities;
using VariantAnnotation.Interface.Providers;

namespace CommandLine.VersionProviders
{
    public sealed class DefaultVersionProvider : IVersionProvider
    {
        public string GetProgramVersion() => $"{CommandLineUtilities.Title} {CommandLineUtilities.InformationalVersion}";

        public string GetDataVersion()    => $"{CommandLineUtilities.Version}";
    }
}
