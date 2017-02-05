using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;

namespace CacheUtils.CombineAndUpdateGenes
{
    sealed class CombineAndUpdateGenesMain : AbstractCommandLineHandler
    {
        public static int Run(string command, string[] args)
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
                    "in|1=",
                    "input genes {path}",
                    v => ConfigurationSettings.InputPath = v
                },
                {
                    "in2|2=",
                    "input genes 2 {path}",
                    v => ConfigurationSettings.InputPath2 = v
                },
                {
                    "out|o=",
                    "output genes {path}",
                    v => ConfigurationSettings.OutputPath = v
                },
                {
                    "refseqGff=",
                    "RefSeq GFF3 {path}",
                    v => ConfigurationSettings.RefSeqGff3Path = v
                }
            };

            var commandLineExample = $"{command} --in <path> --in2 <path> --out <path>";

            var combiner = new CombineAndUpdateGenesMain("Combines an Ensembl and a RefSeq gene file.", ops, commandLineExample, Constants.Authors);
            combiner.Execute(args);
            return combiner.ExitCode;
        }

        /// <summary>
        /// constructor
        /// </summary>
        private CombineAndUpdateGenesMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }


        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.InputPath, "input genes", "--in");
            CheckInputFilenameExists(ConfigurationSettings.InputPath2, "input genes 2", "--in2");
            CheckInputFilenameExists(ConfigurationSettings.HgncPath, "HGNC", "--hgnc");
            //CheckInputFilenameExists(ConfigurationSettings.MergedGeneInfoPath, "merged gene info", "--gi");

            foreach (var geneInfoPath in ConfigurationSettings.GeneInfoPaths)
            {
                CheckInputFilenameExists(geneInfoPath, "gene info", "--gi");
            }

            CheckOutputFilenameSuffix(ConfigurationSettings.OutputPath, ".gz", "output genes");
            HasRequiredParameter(ConfigurationSettings.OutputPath, "output genes", "--out");
            CheckInputFilenameExists(ConfigurationSettings.RefSeqGff3Path, "RefSeq GFF3", "--refseqGff");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var combiner = new GeneCombiner(ConfigurationSettings.InputPath, ConfigurationSettings.InputPath2,
                ConfigurationSettings.GeneInfoPaths, ConfigurationSettings.HgncPath,
                ConfigurationSettings.RefSeqGff3Path);
            combiner.Write(ConfigurationSettings.OutputPath);
        }
    }
}
