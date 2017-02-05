using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;
using VariantAnnotation.Utilities;

namespace GenerateMustGenotypeVcf
{
    sealed class GenerateMustGenotypeVcfsMain : AbstractCommandLineHandler
	{
		public static int Main(string[] args)
		{
			var ops = new OptionSet
			{
				{
					"onek=",
					"input 1000Genomes vcf file",
					v => ConfigurationSettings.OneKGenomeVcf= v
				},
				{
					"cvr=",
					"input clinvar vcf file",
					v => ConfigurationSettings.ClinVarVcf= v
				},
				{
					"cos=",
					"input cosmic vcf file",
					v => ConfigurationSettings.CosmicVcf= v
				},
				{
					"ref=",
					"genome assembly",
					v => ConfigurationSettings.Assembly= v
				}

			};

			var commandLineExample = "--onek <input 1000 genomes vcf file> --cos <cosmic vcf file> --cvr <clinvar vcf file> --out <Output file name> --ref <GRCh37/GRCh38>";
			var generateMustGenotype = new GenerateMustGenotypeVcfsMain("Generates a must genotype vcf containing all ref minor positions in 1000 Genomes",ops, commandLineExample, Constants.Authors);
			generateMustGenotype.Execute(args);

			return generateMustGenotype.ExitCode;
		}

		public GenerateMustGenotypeVcfsMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null) : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
		{
		}

		protected override void ValidateCommandLine()
		{
			CheckInputFilenameExists(ConfigurationSettings.OneKGenomeVcf, "input 1000 genomes vcf", "--onek", false);
			CheckInputFilenameExists(ConfigurationSettings.ClinVarVcf, "input clinvar vcf", "--cvr",false);
			CheckInputFilenameExists(ConfigurationSettings.CosmicVcf, "input cosmic vcf", "--cos", false);
			HasRequiredParameter(ConfigurationSettings.Assembly, "Genome assembly", "--gen");
		}

		protected override void ProgramExecution()
		{
			using (var refMinorExtractor = new MustGenotypeExtractor(ConfigurationSettings.Assembly,ConfigurationSettings.OneKGenomeVcf,
				ConfigurationSettings.ClinVarVcf,
				ConfigurationSettings.CosmicVcf
				))
			{
				refMinorExtractor.ExtractEntries();
			}
			
		}
	}
}
