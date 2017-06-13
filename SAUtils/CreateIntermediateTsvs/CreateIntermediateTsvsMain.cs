using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using CommandLine.VersionProvider;
using VariantAnnotation.DataStructures;

namespace SAUtils.CreateIntermediateTsvs
{
	public class CreateIntermediateTsvsMain : AbstractCommandLineHandler
	{
		private CreateIntermediateTsvsMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null) : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
		{
		}

		protected override void ValidateCommandLine()
		{
			CheckInputFilenameExists(ConfigurationSettings.CompressedReference, "Compressed reference sequence file name", "--ref");
			CheckAndCreateDirectory(ConfigurationSettings.OutputSupplementaryDirectory, "output Supplementary directory", "--out");

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

			foreach (var customFiles in ConfigurationSettings.CustomIntervalFiles)
			{
				CheckInputFilenameExists(customFiles, "Custom interval file name", "--bed", false);
			}

			CheckNonZero(ConfigurationSettings.NumberOfProvidedInputFiles(), "supplementary data source");
		}

		protected override void ProgramExecution()
		{
			// load the reference sequence
			
			var interimTsvCreator =
				new CreateInterimFiles(
					ConfigurationSettings.CompressedReference,
					ConfigurationSettings.OutputSupplementaryDirectory,
					ConfigurationSettings.InputDbSnpFileName,
					ConfigurationSettings.InputCosmicVcfFileName,
					ConfigurationSettings.InputCosmicTsvFileName,
					ConfigurationSettings.InputClinVarFileName,
					ConfigurationSettings.Input1000GFileName,
					ConfigurationSettings.InputEvsFile,
					ConfigurationSettings.InputExacFile,
					ConfigurationSettings.InputDgvFile,
					ConfigurationSettings.Input1000GSvFileName,
					ConfigurationSettings.InputClinGenFileName,
					ConfigurationSettings.CustomAnnotationFiles,
					ConfigurationSettings.CustomIntervalFiles
					);

			interimTsvCreator.CreateTsvs();

		}


		public static int Run(string command, string[] commandArgs)
		{
			var ops = new OptionSet
			{
				{
					 "ref|r=",
					 "compressed reference sequence file",
					 v => ConfigurationSettings.CompressedReference = v
				 },
				 {
					 "dbs|d=",
					 "input dbSNP vcf.gz file",
					 v => ConfigurationSettings.InputDbSnpFileName = v
				 },
				 {
					 "csm|c=",
					 "input COSMIC vcf file",
					 v => ConfigurationSettings.InputCosmicVcfFileName = v
				 },
				 {
					 "tsv=",
					 "input COSMIC TSV file",
					 v => ConfigurationSettings.InputCosmicTsvFileName = v
				 },
				 {
					 "cvr|V=",
					 "input ClinVar file",
					 v => ConfigurationSettings.InputClinVarFileName= v
				 },
				 {
					 "vcf=",
					 "input ClinVar no known medical importance file",
					 v => ConfigurationSettings.InputClinvarXml= v
				 },
				 {
					 "onek|k=",
					 "input 1000 Genomes AF file",
					 v => ConfigurationSettings.Input1000GFileName= v
				 },
				 {
					 "evs|e=",
					 "input EVS file",
					 v => ConfigurationSettings.InputEvsFile= v
				 },
				 {
					 "exac|x=",
					 "input ExAc file",
					 v => ConfigurationSettings.InputExacFile= v
				 },
				 {
					 "dgv|g=",
					 "input Dgv file",
					 v => ConfigurationSettings.InputDgvFile= v
				 },
				 {
					 "cust|t=",
					 "input Custom annotation file",
					 v => ConfigurationSettings.CustomAnnotationFiles.Add(v)
				 },
				 {
					 "bed=",
					 "input Custom interval file",
					 v => ConfigurationSettings.CustomIntervalFiles.Add(v)
				 },
				{
					"onekSv|s=",
					"input 1000 Genomes Structural file",
					v => ConfigurationSettings.Input1000GSvFileName = v
				},
				{
					"clinGen|l=",
					"input ClinGen file",
					v => ConfigurationSettings.InputClinGenFileName = v
				},
				{
					"out|o=",
					"output Nirvana Supplementary directory",
					v => ConfigurationSettings.OutputSupplementaryDirectory = v
				}
			};

			var commandLineExample = $"{command} [options]";

			var converter = new CreateIntermediateTsvsMain("Reads provided supplementary data files and populates tsv files", ops, commandLineExample, Constants.Authors);

			converter.Execute(commandArgs);
			return converter.ExitCode;
		}
	}
}
