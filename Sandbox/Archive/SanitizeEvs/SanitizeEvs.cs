using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Illumina.VariantAnnotation.FileHandling;
using Illumina.VariantAnnotation.Utilities;
using NDesk.Options;

namespace SanitizeEvs
{
	public class SanitizeEvs : AbstractCommandLineHandler
	{
		static int Main(string[] args)
		{
			var ops = new OptionSet
			{
				{
					"in|i=",
					"input cache {directory}",
					v => ConfigurationSettings.InputEvsDirectory= v
				},
				{
					"out|o=",
					"output {file name}",
					v => ConfigurationSettings.OutputFileName = v
				}
			};

			var commandLineExample = "--in <evs directory> --out <merged and sanitized file name>";

			var sanitizer = new SanitizeEvs("Outputs exon coordinates for all transcripts in a database.", ops, commandLineExample, Illumina.VariantAnnotation.DataStructures.Constants.Authors);
			sanitizer.Execute(args);
			return sanitizer.ExitCode;
		}

		public SanitizeEvs(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null) : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
		{
		}

		protected override void ValidateCommandLine()
		{
			CheckDirectoryExists(ConfigurationSettings.InputEvsDirectory, "input evs directory", "--in");
		}

		protected override void ProgramExecution()
		{
			var evsFiles = Directory.GetFiles(ConfigurationSettings.InputEvsDirectory, "*.vcf.gz").Select(Path.GetFileName).ToList();
			evsFiles.Sort();

			using (
				var evsOutFile = new GZipStream(new FileStream(ConfigurationSettings.OutputFileName, FileMode.Create),
					CompressionMode.Compress))
			{
				foreach (var evsFile in evsFiles)
				{
					Console.WriteLine("Sanitizing "+evsFile);

					var evsInFile = GZipUtilities.GetAppropriateStreamReader(Path.Combine(ConfigurationSettings.InputEvsDirectory,evsFile));
					var evsLine = evsInFile.ReadLine();
					while (evsLine != null)
					{
						if (!evsLine.StartsWith("#"))
						{
							if (evsLine.Contains("GRCh38_POSITION=") )
							{
								var vcfFields = evsLine.Split('\t');
								var grch38LocIndex = evsLine.IndexOf("GRCh38_POSITION=", StringComparison.Ordinal) + "GRCh38_POSITION=".Length;
								var grch38Pos = evsLine.Substring(grch38LocIndex);

                                var grch38Loc = grch38Pos.Contains(":")? grch38Pos.Split(':')[1]: grch38Pos;
								if (grch38Loc != null)
								{
									vcfFields[VcfCommon.PosIndex] = grch38Loc;
									var newVcfLine = string.Join("\t", vcfFields) + "\n";
									evsOutFile.Write(Encoding.ASCII.GetBytes(newVcfLine), 0, newVcfLine.Length);
								}
								
							}
							
						}
						evsLine = evsInFile.ReadLine();
					}
					evsInFile.Close();
				}
			}
			
		}
	}
}
