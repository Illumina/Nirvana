using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;

namespace GffComparison
{
    sealed class GffComparisonMain : AbstractCommandLineHandler
    {
        public static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|1=",
                    "input genes {path}",
                    v => ConfigurationSettings.InputPath = v
                },
                {
                    "in2|2=",
                    "input genes 2 {path}",
                    v => ConfigurationSettings.InputPath2 = v
                },
                //{
                //    "out|o=",
                //    "output tsv {path}",
                //    v => ConfigurationSettings.OutputTsvPath = v
                //}
            };

            var commandLineExample = "-1 <GFF path> -2 <GFF path> -o <tsv from GFF 1>";

            var gff = new GffComparisonMain("Compares two GFF files", ops, commandLineExample, Constants.Authors);
            gff.Execute(args);
            return gff.ExitCode;
        }

        /// <summary>
        /// constructor
        /// </summary>
        private GffComparisonMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.InputPath, "GFF #1", "--in");
            CheckInputFilenameExists(ConfigurationSettings.InputPath2, "GFF #2", "--in2");
            //HasRequiredParameter(ConfigurationSettings.OutputTsvPath, "output tsv path", "--out");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var comparer = new GffComparer(ConfigurationSettings.InputPath, ConfigurationSettings.InputPath2);
            comparer.Compare(ConfigurationSettings.OutputTsvPath);
        }
    }
}
