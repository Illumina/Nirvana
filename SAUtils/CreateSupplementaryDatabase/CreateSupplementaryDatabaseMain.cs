using NDesk.Options;
using VariantAnnotation.CommandLine;

namespace SAUtils.CreateSupplementaryDatabase
{
    public class CreateSupplementaryDatabaseMain : AbstractCommandLineHandler
    {
        // constructor
        public CreateSupplementaryDatabaseMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
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
			CheckInputFilenameExists(ConfigurationSettings.InputClinVarFileName, "input ClinVar xml file", "--cvr", false);
			CheckInputFilenameExists(ConfigurationSettings.InputClinvarXml, "no known medical importance vcf file", "--cvr", false);
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
	        // ReSharper disable once UnusedVariable
	        var supplementaryDatabaseCreator =
		        new CreateSupplementaryDatabase(
                    ConfigurationSettings.CompressedReference,
					ConfigurationSettings.OutputSupplementaryDirectory,
			        ConfigurationSettings.InputDbSnpFileName, 
					ConfigurationSettings.InputCosmicVcfFileName, 
					ConfigurationSettings.InputCosmicTsvFileName,
					ConfigurationSettings.InputClinVarFileName,
					ConfigurationSettings.Input1000GFileName,
			        ConfigurationSettings.InputEvsFile, 
					ConfigurationSettings.InputExacFile,
			        ConfigurationSettings.CustomAnnotationFiles, 
					ConfigurationSettings.InputDgvFile,
					ConfigurationSettings.Input1000GSvFileName,
					ConfigurationSettings.InputClinGenFileName,
					ConfigurationSettings.ChromosomeList
					);

			supplementaryDatabaseCreator.CreateDatabase();
			
        }

	}
}
