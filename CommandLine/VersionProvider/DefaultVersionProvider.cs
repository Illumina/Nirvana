using CommandLine.Utilities;

namespace CommandLine.VersionProvider
{
    public sealed class DefaultVersionProvider : IVersionProvider
    {
        public string GetProgramVersion() => $"{CommandLineUtilities.Title} {CommandLineUtilities.InformationalVersion}";

        public string GetDataVersion()    => $"{CommandLineUtilities.Version}";
    }
}
