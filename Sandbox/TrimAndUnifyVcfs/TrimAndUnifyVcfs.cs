using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using VariantAnnotation.DataStructures;

namespace TrimAndUnifyVcfs
{
	public sealed class TrimAndUnifyVcfs : AbstractCommandLineHandler
	{
		static void Main(string[] args)
		{
			var ops = new OptionSet
			{
				{
					"inDir=",
					"input {directory} for 1000 genome vcf files ",
					v => ConfigurationSettings.InputDirectory = v
				},
				{
					"svOut=",
					"output {file} for structural variant",
					v => ConfigurationSettings.StructuralVariantOutputFile = v
				},
				{
					"snvOut=",
					"output {file} for snv and small indels",
					v => ConfigurationSettings.SmallVariantOutputFile = v
				},
				{
					"sample=",
					"sample information {file}",
					v => ConfigurationSettings.SampleInfoFile = v
				},{
					"pop=",
					"population information {file}",
					v => ConfigurationSettings.PopulationInfoFile = v
				}
			};

			var commandLineExample = "--inDir <directory> --svOut <filename> --snvOut <filename> --sample <filename> --pop <filename> ";

			var parser = new TrimAndUnifyVcfs("preprocess 1000 genome vcf files", ops, commandLineExample, Constants.Authors);
			parser.Execute(args);

		}

	    private TrimAndUnifyVcfs(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
			: base(programDescription, ops, commandLineExample, programAuthors)
		{
		}

		protected override void ValidateCommandLine()
		{
			HasRequiredParameter(ConfigurationSettings.SmallVariantOutputFile, "output file for snv and small indels", "--snvOut");
			HasRequiredParameter(ConfigurationSettings.StructuralVariantOutputFile, "output file for structural variant", "--svOut");
			CheckDirectoryExists(ConfigurationSettings.InputDirectory,"input vcf directories", "--inDir");
			CheckInputFilenameExists(ConfigurationSettings.SampleInfoFile, "sample infor", "--sample");
			CheckInputFilenameExists(ConfigurationSettings.PopulationInfoFile, "population infor", "--pop");
		}

		protected override void ProgramExecution()
		{
			var oneKGenProcessor = new OneKGenVcfProcessor(ConfigurationSettings.InputDirectory,
				ConfigurationSettings.SmallVariantOutputFile, ConfigurationSettings.StructuralVariantOutputFile,
				ConfigurationSettings.SampleInfoFile, ConfigurationSettings.PopulationInfoFile);
			oneKGenProcessor.DividAndUnifyVcfFiles();
		}
		
	}
}
