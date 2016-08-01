using System.Collections.Generic;
using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.Utilities;
using SAUtils.CreateCustomIntervalDatabase;
using SAUtils.CreateSupplementaryDatabase;

namespace SAUtils
{
    public class SAUtils : TopLevelCommandLineHandler
    {
        /// <summary>
        /// constructor
        /// </summary>
        private SAUtils(string programDescription, Dictionary<string, TopLevelOption> ops, string authors,
            IVersionProvider provider = null)
            : base(programDescription, OutputHelper.GetExecutableName(), ops, authors, provider)
        { }

        public static int Main(string[] args)
        {
            var ops = new Dictionary<string, TopLevelOption>
            {
                ["createSA"] = new TopLevelOption("create supplementary annotation database", CreateSupplementaryDb.Run),
                ["createCI"] = new TopLevelOption("create custom intervals database", CreateCustomIntervalDb.Run)
            };

            var utils = new SAUtils("Utilities focused on querying the supplementary annotation directory", ops, VariantAnnotation.DataStructures.Constants.Authors,
                new SAVersionProvider());

            utils.ParseCommandLine(args);
            return utils.ExitCode;
        }
    }
}
