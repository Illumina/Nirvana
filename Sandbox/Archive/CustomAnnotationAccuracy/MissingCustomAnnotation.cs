using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Illumina.VariantAnnotation.Utilities;
using NDesk.Options;

namespace CustomAnnotationAccuracy
{
	class MissingCustomAnnotation : AbstractCommandLineHandler
	{
		// constructor
		public MissingCustomAnnotation(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }
		static int Main(string[] args)
		{
			var ops = new OptionSet
			{
				{
					"dir|d=",
					"input VCF {path}",
					v => ConfigurationSettings.CacheDirectory = v
				},
				{
					"in|i=",
					"input VCF {path}",
					v => ConfigurationSettings.VcfPath = v
				},
				{
					"r|ref=",
					"input compressed reference sequence {path}",
					v => ConfigurationSettings.CompressedReferencePath = v
				},
				{
					"g|go",
					"do not stop on difference",
					v => ConfigurationSettings.DoNotStopOnDifference = v != null
				},


			};

			var commandLineExample = "-i <vcf path> -d <cache dir> -r <ref path>";
			var accuracy = new MissingCustomAnnotation("Idetifiying Missing Custom Annotations", ops, commandLineExample, "");
			accuracy.Execute(args);
			return accuracy.ExitCode;
		}

		protected override void ProgramExecution()
		{
			var annotator = new AnnotationComparer(ConfigurationSettings.VcfPath, ConfigurationSettings.CompressedReferencePath,ConfigurationSettings.CacheDirectory);
			annotator.Compare();

		}

		protected override void ValidateCommandLine()
		{
			CheckInputFilenameExists(ConfigurationSettings.VcfPath, "vcf", "--in");
			CheckInputFilenameExists(ConfigurationSettings.CompressedReferencePath, "compressed reference sequence", "--ref");
			CheckDirectoryExists(ConfigurationSettings.CacheDirectory, "cache", "--dir");
		}
	}
}
