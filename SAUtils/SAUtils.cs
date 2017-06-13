using System.Collections.Generic;
using CommandLine.Handlers;
using CommandLine.Utilities;
using CommandLine.VersionProvider;
using SAUtils.CreateIntermediateTsvs;
using SAUtils.CreateOmimDatabase;
using SAUtils.ExtractMiniSa;
using SAUtils.MergeInterimTsvs;
using VariantAnnotation.DataStructures;

namespace SAUtils
{
    public class SaUtils : TopLevelCommandLineHandler
    {
        private SaUtils(string programDescription, Dictionary<string, TopLevelOption> ops, string authors,
            IVersionProvider provider = null)
            : base(programDescription, OutputHelper.GetExecutableName(), ops, authors, provider)
        { }

        public static int Main(string[] args)
        {
            var ops = new Dictionary<string, TopLevelOption>
            {
                ["createOMIM"] = new TopLevelOption("create omim database", CreateOmimDatabaseMain.Run),
                ["createSA"] = new TopLevelOption("create Nirvana supplementary annotation database", MergeIntermediateTsvsMain.Run),
                ["createTSV"] = new TopLevelOption("create intermediate tsv file for supplementary annotation", CreateIntermediateTsvsMain.Run),
                ["extractMiniSA"] = new TopLevelOption("extracts mini SA", ExtractMiniSaMain.Run),

            };

            var utils = new SaUtils("Utilities focused on querying the cache directory", ops, Constants.Authors);

            utils.ParseCommandLine(args);
            return utils.ExitCode;
        }







    }
}
