using System;
using System.IO.Compression;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Algorithms;
using Compression.FileHandling;
using ErrorHandling;
using ErrorHandling.Exceptions;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;

namespace CacheUtils.Commands.Header
{
    public static class HeaderMain
    {
        private static string _inputPrefix;

        private static ExitCodes ProgramExecution()
        {
            var cachePath = CacheConstants.TranscriptPath(_inputPrefix);
            var header    = GetHeaderInformation(cachePath);

            Console.WriteLine($"Versions: Schema: {header.Schema}, Data: {header.Data}, VEP: {header.Vep}");
            return ExitCodes.Success;
        }

        private static (ushort Schema, ushort Data, ushort Vep) GetHeaderInformation(string cachePath)
        {
            CacheHeader header;
            TranscriptCacheCustomHeader customHeader = null;

            using (var stream      = FileUtilities.GetReadStream(cachePath))
            using (var blockStream = new BlockStream(new Zstandard(), stream, CompressionMode.Decompress))
            {
                header = blockStream.ReadHeader(CacheHeader.Read, TranscriptCacheCustomHeader.Read) as CacheHeader;
                if (header != null) customHeader = header.CustomHeader as TranscriptCacheCustomHeader;
            }

            if (header == null || customHeader == null) throw new InvalidFileFormatException($"Could not parse the header information correctly for {cachePath}");

            return (header.SchemaVersion, header.DataVersion, customHeader.VepVersion);
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
