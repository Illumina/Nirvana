using VariantAnnotation.FileHandling;
using NDesk.Options;
using VariantAnnotation.CommandLine;

namespace SAUtils.CreateSupplementaryDatabase
{
    public class CreateSupplementaryDb : AbstractCommandLineHandler
    {
        public static int Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                     "r|ref=",
                     "compressed reference sequence file",
                     v => ConfigurationSettings.CompressedReference = v
                 },

                 {
                     "d|dbs=",
                     "input dbSNP vcf.gz file",
                     v => ConfigurationSettings.InputDbSnpFileName = v
                 },

                 {
                     "c|csm=",
                     "input COSMIC vcf file",
                     v => ConfigurationSettings.InputCosmicVcfFileName = v
                 },
                 {
                     "tsv=",
                     "input COSMIC TSV file",
                     v => ConfigurationSettings.InputCosmicTsvFileName = v
                 },
                 {
                     "V|cvr=",
                     "input ClinVar file",
                     v => ConfigurationSettings.InputClinVarFileName= v
                 },
                 {
                     "pub=",
                     "input ClinVar file with pubmed ids",
                     v => ConfigurationSettings.InputClinVarPubmedFileName= v
                 },
                 {
                     "eval=",
                     "input ClinVar file with last evaluated date",
                     v => ConfigurationSettings.InputClinVarEvaluatedDateFileName= v
                 },
                 {
                     "k|onek=",
                     "input 1000 Genomes AF file",
                     v => ConfigurationSettings.Input1000GFileName= v
                 },
                 {
                     "e|evs=",
                     "input EVS file",
                     v => ConfigurationSettings.InputEvsFile= v
                 },
                 {
                     "x|exac=",
                     "input ExAc file",
                     v => ConfigurationSettings.InputExacFile= v
                 },
                 {
                     "g|dgv=",
                     "input Dgv file",
                     v => ConfigurationSettings.InputDgvFile= v
                 },
                 {
                     "t|cust=",
                     "input Custom annotation file",
                     v => ConfigurationSettings.CustomAnnotationFiles.Add(v)
                 },
                {
                    "s|onekSv=",
                    "input 1000 Genomes Structural file",
                    v => ConfigurationSettings.Input1000GSvFileName = v
                },
                {
                    "l|clinGen=",
                    "input ClinGen file",
                    v => ConfigurationSettings.InputClinGenFileName = v
                },
                {
                    "o|out=",
                    "output Nirvana Supplementary directory",
                    v => ConfigurationSettings.OutputSupplementaryDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var converter = new CreateSupplementaryDb("Reads provided supplemetary data files and populates the combined nirvana supplementary database file", ops, commandLineExample, VariantAnnotation.DataStructures.Constants.Authors);
            converter.Execute(commandArgs);
            return converter.ExitCode;
        }

        // constructor
        public CreateSupplementaryDb(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.CompressedReference, "Compressed reference sequence file name", "--ref");
            CheckDirectoryExists(ConfigurationSettings.OutputSupplementaryDirectory, "output Supplementary directory", "--out");

            CheckInputFilenameExists(ConfigurationSettings.InputDbSnpFileName, "input VCF file containing dbSNP scores", "--dbs", false);
			CheckInputFilenameExists(ConfigurationSettings.InputCosmicVcfFileName, "input unified COSMIC file", "--csm", false);
			CheckInputFilenameExists(ConfigurationSettings.InputCosmicTsvFileName, "input cosmic tsv file", "--tsv", false);
			CheckInputFilenameExists(ConfigurationSettings.InputClinVarFileName, "input ClinVar vcf file", "--cvr", false);
			CheckInputFilenameExists(ConfigurationSettings.InputClinVarPubmedFileName, "input ClinVar tsv file with pubmed ids", "--pub", false);
			CheckInputFilenameExists(ConfigurationSettings.InputClinVarEvaluatedDateFileName, "input ClinVar tsv file with last evaluated date", "--eval", false);
			CheckInputFilenameExists(ConfigurationSettings.Input1000GFileName, "input 1000 Genomes AF file", "--onek", false);
            CheckInputFilenameExists(ConfigurationSettings.InputEvsFile, "input EVS file", "--evs", false);
            CheckInputFilenameExists(ConfigurationSettings.InputExacFile, "input Exac file", "--exac", false);
			CheckInputFilenameExists(ConfigurationSettings.InputDgvFile, "input DGV file", "--dgv", false);
			CheckInputFilenameExists(ConfigurationSettings.Input1000GSvFileName, "input DGV file", "--onekSv", false);

			foreach (var customFiles in ConfigurationSettings.CustomAnnotationFiles)
            {
                CheckInputFilenameExists(customFiles, "Custom Annotation file name", "--cust", false);
            }

            CheckNonZero(ConfigurationSettings.NumberOfProvidedInputFiles(), "supplementary data source");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            // load the reference sequence
	        AnnotationLoader.Instance.LoadCompressedSequence(ConfigurationSettings.CompressedReference);

	        // ReSharper disable once UnusedVariable
	        var supplementaryDatabaseCreator =
		        new CreateSupplementaryDatabase(
					ConfigurationSettings.OutputSupplementaryDirectory,
			        ConfigurationSettings.InputDbSnpFileName, 
					ConfigurationSettings.InputCosmicVcfFileName, 
					ConfigurationSettings.InputCosmicTsvFileName,
					ConfigurationSettings.InputClinVarFileName,
					ConfigurationSettings.InputClinVarPubmedFileName,
					ConfigurationSettings.InputClinVarEvaluatedDateFileName,
					ConfigurationSettings.Input1000GFileName,
			        ConfigurationSettings.InputEvsFile, 
					ConfigurationSettings.InputExacFile,
			        ConfigurationSettings.CustomAnnotationFiles, 
					ConfigurationSettings.InputDgvFile,
					ConfigurationSettings.Input1000GSvFileName,
					ConfigurationSettings.InputClinGenFileName
					);

			supplementaryDatabaseCreator.CreateDatabase();
			
        }

	}
}
