using System.Collections.Generic;
using CacheUtils.GFF;
using CacheUtils.RegulatoryGFF;
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
                ["gff"]  = new TopLevelOption("export transcripts to GFF", CreateGff.Run),
                ["rgff"] = new TopLevelOption("export regulatory regions to GFF", CreateRegulatoryGff.Run)
            };

            var utils = new CacheUtils("Utilities focused on querying the cache directory", ops, VariantAnnotation.DataStructures.Constants.Authors,
                new CacheVersionProvider());

            utils.ParseCommandLine(args);
            return utils.ExitCode;
        }
    }
}
