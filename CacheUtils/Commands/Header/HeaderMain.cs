using System;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using ErrorHandling.Exceptions;
using IO;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Providers;

namespace CacheUtils.Commands.Header
{
    public static class HeaderMain
    {
        private static string _inputPrefix;

        private static ExitCodes ProgramExecution()
        {
            string cachePath = CacheConstants.TranscriptPath(_inputPrefix);
            var header       = GetHeaderInformation(cachePath);

            Console.WriteLine($"Versions: Schema: {header.Schema}, Data: {header.Data}, VEP: {header.Vep}");
            return ExitCodes.Success;
        }

        private static (ushort Schema, ushort Data, ushort Vep) GetHeaderInformation(string cachePath)
        {
            CacheHeader header;
            using (var stream = FileUtilities.GetReadStream(cachePath))
            {
                header = CacheHeader.Read(stream);
            }

            if (header == null) throw new InvalidFileFormatException($"Could not parse the header information correctly for {cachePath}");

            return (header.SchemaVersion, header.DataVersion, header.Custom.VepVersion);
        }

        public static ExitCodes Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input cache {prefix}",
                    v => _inputPrefix = v
                }
            };

            return new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .HasRequiredParameter(_inputPrefix, "input cache prefix", "--in")
                .SkipBanner()
                .ShowHelpMenu("Displays the cache header information.", $"{command} --in <cache prefix>")
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
