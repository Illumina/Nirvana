using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using VariantAnnotation.DataStructures;

namespace UpdateOmimGeneSymbols
{
    sealed class UpdateOmimGeneSymbolsMain : AbstractCommandLineHandler
    {
        public static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "gi=",
                    "gene_info {path} (newest first)",
                    v => ConfigurationSettings.GeneInfoPaths.Add(v)
                },
                {
                    "hgnc=",
                    "HGNC {path}",
                    v => ConfigurationSettings.HgncPath = v
                },
                {
                    "in|i=",
                    "input genemap2 {path}",
                    v => ConfigurationSettings.InputGeneMap2Path = v
                },
                {
                    "out|o=",
                    "output genemap2 {path}",
                    v => ConfigurationSettings.OutputGeneMap2Path = v
                }
            };

            var commandLineExample = "-i <input genemap2 path> -o <output genemap2 path>";

            var update = new UpdateOmimGeneSymbolsMain("Updates the gene symbols in an OMIM genemap2 file", ops, commandLineExample, Constants.Authors);
            update.Execute(args);
            return update.ExitCode;
        }

        /// <summary>
        /// constructor
        /// </summary>
        private UpdateOmimGeneSymbolsMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.InputGeneMap2Path, "genemap2", "--in");
            HasRequiredParameter(ConfigurationSettings.OutputGeneMap2Path, "genemap2", "--out");
            CheckInputFilenameExists(ConfigurationSettings.HgncPath, "HGNC", "--hgnc");

            foreach (var geneInfoPath in ConfigurationSettings.GeneInfoPaths)
            {
                CheckInputFilenameExists(geneInfoPath, "gene info", "--gi");
            }
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var updater = new OmimGeneSymbolUpdater(ConfigurationSettings.GeneInfoPaths, ConfigurationSettings.HgncPath);
            updater.Update(ConfigurationSettings.InputGeneMap2Path, ConfigurationSettings.OutputGeneMap2Path);
        }
    }
}
