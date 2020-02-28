using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;

namespace SAUtils.ExtractMiniXml
{
    public static class ExtractMiniXmlMain
	{
	    private  static string _inputXmlFile;
	    private  static string _accessions;
	    private  static string _outputDir;

	    private static ExitCodes ProgramExecution()
	    {
		    var accessions = GetAccessions(_accessions);
	        if (accessions.Any(x=>x.StartsWith("RCV")))
	        {
		        var rcvExtractor = new RcvXmlExtractor(_inputXmlFile, accessions, _outputDir);
		        rcvExtractor.Extract();
	        }

	        if (accessions.Any(x=>x.StartsWith("VCV")))
	        {
		        var vcvExtractor = new VcvXmlExtractor(_inputXmlFile, accessions, _outputDir);
		        vcvExtractor.Extract();
	        }

	        return ExitCodes.Success;
        }

	    private static List<string> GetAccessions(string accString)
	    {
		    var accessions = new List<string>();
		    if (Directory.Exists(accString))
		    {
			    foreach (var fileName in Directory.EnumerateFiles(accString))
			    {
				    if(fileName.Contains("RCV") || fileName.Contains("VCV")) accessions.Add(Path.GetFileNameWithoutExtension(fileName));
			    }

			    return accessions;
		    }

		    return accString.Split(',').ToList();
		    
	    }

	    public static ExitCodes Run(string command, string[] commandArgs)
        {
			var ops = new OptionSet
			{
				{
					"i|in=",
					"Input XML {file}",
					v => _inputXmlFile = v
				},
				{
					"a|acc=",
					"accessions",
					v => _accessions = v
				},
				{
					"o|out=",
					"Output {dir}",
					v => _outputDir = v
				}
			};

			var commandLineExample = $"{command} --in <xml file> --out <output Directory> --rcv <RCV ID>";

			var exitCode = new ConsoleAppBuilder(commandArgs, ops)
	            .Parse()
	            .CheckInputFilenameExists(_inputXmlFile, "input XML file", "--in")
	            .HasRequiredParameter(_outputDir, "output directory", "--out")
                .HasRequiredParameter(_accessions, "comma separated list of accessions or folder containing mini XML files to update", "--acc")
                .SkipBanner()
                .ShowHelpMenu("Extracts mini supplementary annotations for the given range from Nirvana Supplementary Annotations files.", commandLineExample)
	            .ShowErrors()
	            .Execute(ProgramExecution);
	        
	        return exitCode;
		}
	}
}
