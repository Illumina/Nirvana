using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using VariantAnnotation.DataStructures;

namespace CreateCompressedReference
{
    class CreateCompressedReferenceMain : AbstractCommandLineHandler
    {
        // constructor
        public CreateCompressedReferenceMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.InputFastaPath, "FASTA", "--in");
            CheckInputFilenameExists(ConfigurationSettings.InputChromosomeNamesPath, "chromosome names", "--cn");
            CheckInputFilenameExists(ConfigurationSettings.InputCytobandPath, "cytoband", "--cytoband");
            HasRequiredParameter(ConfigurationSettings.GenomeAssembly, "genome assembly", "--ga");
            HasRequiredParameter(ConfigurationSettings.OutputCompressedPath, "output compressed path", "--out");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var converter = new FastaToCompressedConverter();

            var genomeAssembly = GenomeAssemblyUtilities.Convert(ConfigurationSettings.GenomeAssembly);

            converter.Convert(ConfigurationSettings.InputFastaPath, ConfigurationSettings.InputCytobandPath,
                ConfigurationSettings.InputChromosomeNamesPath, ConfigurationSettings.OutputCompressedPath,
                genomeAssembly);
        }

        static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input FASTA {filename}",
                    v => ConfigurationSettings.InputFastaPath = v
                },
                {
                    "cn=",
                    "input chromosome names {filename}",
                    v => ConfigurationSettings.InputChromosomeNamesPath = v
                },
                {
                    "cytoband=",
                    "input cytoband {filename}",
                    v => ConfigurationSettings.InputCytobandPath = v
                },
                {
                    "ga=",
                    "genome assembly {version}",
                    v => ConfigurationSettings.GenomeAssembly = v
                },
                {
                    "out|o=",
                    "output compressed reference {filename}",
                    v => ConfigurationSettings.OutputCompressedPath = v
                }
            };

            var commandLineExample = "--in <FASTA filename> --cn <chromosome names filename> --out <compressed filename>";

            var creator = new CreateCompressedReferenceMain("Converts a FASTA file to a compressed reference file.", ops, commandLineExample, Constants.Authors);
            creator.Execute(args);
            return creator.ExitCode;
        }
    }
}
