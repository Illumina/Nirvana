using CommandLine.Handlers;
using CommandLine.NDesk.Options;

namespace CreateGeneSymbolsAndSynonyms
{
    class ParseGeneSymbolsMain : AbstractCommandLineHandler
    {
        // constructor
        public ParseGeneSymbolsMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            HasRequiredParameter(ConfigurationSettings.OutputGeneSymbolsPath, "output gene symbols", "--out");
            CheckInputFilenameExists(ConfigurationSettings.InputGeneInfoPath, "gene_info", "--geneinfo");
            CheckInputFilenameExists(ConfigurationSettings.InputGene2RefSeqPath, "gene2refseq", "--gene2refseq");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var parser = new GeneSymbolParser();
            parser.LoadGeneInfo(ConfigurationSettings.InputGeneInfoPath);
            parser.LoadGene2RefSeq(ConfigurationSettings.InputGene2RefSeqPath);
            parser.WriteGeneSymbols(ConfigurationSettings.OutputGeneSymbolsPath);
        }

        static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "geneinfo=",
                    "input gene_info {filename} (RefSeq)",
                    v => ConfigurationSettings.InputGeneInfoPath = v
                },
                {
                    "gene2refseq=",
                    "input gene2refseq {filename} (RefSeq)",
                    v => ConfigurationSettings.InputGene2RefSeqPath = v
                },
                {
                    "out|o=",
                    "output gene symbols {filename}",
                    v => ConfigurationSettings.OutputGeneSymbolsPath = v
                }
            };

            var commandLineExample = "--geneinfo <filename> --gene2refseq <filename> --out <filename>";

            var parser = new ParseGeneSymbolsMain("Creates a flat gene symbols file", ops, commandLineExample, "Stromberg, Roy, and Lajugie");
            parser.Execute(args);
            return parser.ExitCode;
        }
    }
}
