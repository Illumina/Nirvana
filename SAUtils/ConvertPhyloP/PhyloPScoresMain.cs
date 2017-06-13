using System;
using System.IO;
using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using SAUtils.InputFileParsers;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.Phylop;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;

namespace SAUtils.ConvertPhyloP
{
	sealed class PhyloPScoresMain : AbstractCommandLineHandler
    {
        // constructor
        private PhyloPScoresMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.InputWigFixFile, "input wigFix file", "--in");
            CheckDirectoryExists(ConfigurationSettings.OutputNirvanaDirectory, "Nirvana PhyloP Directory", "--out");
			HasRequiredParameter(ConfigurationSettings.GenomeAssembly, "Genome Assembly version", "--genome");
		}

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
			Console.WriteLine("Reading file: {0}", ConfigurationSettings.InputWigFixFile);

			var timer = new Benchmark();

			var version = GetDataVersion();

	        using (var nirvanaPhylopDatabaseCreator = new PhylopWriter(ConfigurationSettings.InputWigFixFile, version, GenomeAssemblyUtilities.Convert(ConfigurationSettings.GenomeAssembly), ConfigurationSettings.OutputNirvanaDirectory))
			{
				nirvanaPhylopDatabaseCreator.ExtractPhylopScores();
			}
			Console.WriteLine("Time:{0}", timer.GetElapsedTime());

        }

		private static DataSourceVersion GetDataVersion()
		{
			var versionFileName = ConfigurationSettings.InputWigFixFile + ".version";

			if (!File.Exists(versionFileName))
			{
				throw new FileNotFoundException(versionFileName);
			}

			var versionReader = new DataSourceVersionReader(versionFileName);
			var version = versionReader.GetVersion();
			return version;
		}

		public static int Run(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "i|in=",
                    "input wigFix file",
                    v => ConfigurationSettings.InputWigFixFile = v
                },
				{
					"g|genome=",
					"genome Assembly: GRCh37 or GRCh38",
					v => ConfigurationSettings.GenomeAssembly = v
				},
				{
                    "o|out=",
                    "output Nirvana Phylop directory",
                    v => ConfigurationSettings.OutputNirvanaDirectory = v
                }
            };

            var commandLineExample = "--in <input directory containig wigFix files> --genome <GRCh37, GRCh38 >--out <Nirvana Phylop directory>";

            var phyloP = new PhyloPScoresMain("Converts PhyloP scores presented in WigFix files to Nirvana native format", ops, commandLineExample, Constants.Authors);
            phyloP.Execute(args);
            return phyloP.ExitCode;
        }
    }
}
