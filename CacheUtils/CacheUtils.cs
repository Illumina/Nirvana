using System.Collections.Generic;
using CommandLine.Builders;
using VariantAnnotation.Interface;
using CacheUtils.ExtractTranscripts;

namespace CacheUtils
{
    internal static class CacheUtilsMain
    {
        static int Main(string[] args)
        {
            var ops = new Dictionary<string, TopLevelOption>
            {
                ["exttran"] = new TopLevelOption("extracts transcripts", ExtractTranscriptMain.Run)
            };

            var exitCode = new TopLevelAppBuilder(args, ops)
                .Parse()
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Utilities focused on querying the cache directory")
                .ShowErrors()
                .Execute();

            return (int)exitCode;
        }
    }
}