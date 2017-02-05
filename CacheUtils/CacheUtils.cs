using System.Collections.Generic;
using CacheUtils.CombineAndUpdateGenes;
using CacheUtils.CombineCacheDirectories;
using CacheUtils.CreateCache;
using CacheUtils.ExtractRegulatoryElements;
using CacheUtils.ExtractTranscripts;
using CacheUtils.GFF;
using CacheUtils.ParseVepCacheDirectory;
using CacheUtils.RegulatoryGFF;
using CacheUtils.UpdateMiniCacheFiles;
using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.Utilities;

namespace CacheUtils
{
    public class CacheUtils : TopLevelCommandLineHandler
    {
        /// <summary>
        /// constructor
        /// </summary>
        private CacheUtils(string programDescription, Dictionary<string, TopLevelOption> ops, string authors,
            IVersionProvider provider = null)
            : base(programDescription, OutputHelper.GetExecutableName(), ops, authors, provider)
        { }

        public static int Main(string[] args)
        {
            var ops = new Dictionary<string, TopLevelOption>
            {
                ["combine"] = new TopLevelOption("combine cache directories", CombineCacheDirectoriesMain.Run),
                ["create"]  = new TopLevelOption("create Nirvana cache files", CreateNirvanaDatabaseMain.Run),
                ["extreg"]  = new TopLevelOption("extracts regulatory regions", ExtractRegulatoryElementsMain.Run),
                ["exttran"] = new TopLevelOption("extracts transcripts", ExtractTranscriptsMain.Run),
                ["gff"]     = new TopLevelOption("export transcripts to GFF", CreateGff.Run),
                ["parse"]   = new TopLevelOption("parses the VEP cache files", ParseVepCacheDirectoryMain.Run),
                ["rgff"]    = new TopLevelOption("export regulatory regions to GFF", CreateRegulatoryGff.Run),
                ["gene"]    = new TopLevelOption("updates genes in intermediate files", CombineAndUpdateGenesMain.Run),
                ["update"]  = new TopLevelOption("updates the mini-cache files", UpdateMiniCacheFilesMain.Run)
            };

            var utils = new CacheUtils("Utilities focused on querying the cache directory", ops, VariantAnnotation.DataStructures.Constants.Authors,
                new CacheVersionProvider());

            utils.ParseCommandLine(args);
            return utils.ExitCode;
        }
    }
}
