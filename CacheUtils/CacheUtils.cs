using System.Collections.Generic;
using CacheUtils.Commands.CombineCacheDirectories;
using CacheUtils.Commands.CreateCache;
using CacheUtils.Commands.Download;
using CacheUtils.Commands.ExtractTranscripts;
using CacheUtils.Commands.GFF;
using CacheUtils.Commands.Header;
using CacheUtils.Commands.ParseVepCacheDirectory;
using CacheUtils.Commands.RegulatoryGFF;
using CacheUtils.Commands.UniversalGeneArchive;
using CommandLine.Builders;
using VariantAnnotation.Interface;

namespace CacheUtils
{
    internal static class CacheUtilsMain
    {
        private static int Main(string[] args)
        {
            var ops = new Dictionary<string, TopLevelOption>
            {
                ["combine"]  = new TopLevelOption("combine cache directories", CombineCacheDirectoriesMain.Run),
                ["create"]   = new TopLevelOption("create Nirvana cache files", CreateNirvanaDatabaseMain.Run),
                ["download"] = new TopLevelOption("downloads required files", DownloadMain.Run),
                ["extract"]  = new TopLevelOption("extracts transcripts", ExtractTranscriptsMain.Run),
                ["gene"]     = new TopLevelOption("updates the universal gene archive", UniversalGeneArchiveMain.Run),
                ["gff"]      = new TopLevelOption("export transcripts to GFF", CreateGffMain.Run),
                ["header"]   = new TopLevelOption("displays the header information", HeaderMain.Run),
                ["parse"]    = new TopLevelOption("parses the VEP cache files", ParseVepCacheDirectoryMain.Run),
                ["rgff"]     = new TopLevelOption("export regulatory regions to GFF", CreateRegulatoryGffMain.Run)
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