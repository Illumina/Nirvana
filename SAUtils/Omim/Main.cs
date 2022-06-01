using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using IO;
using Newtonsoft.Json.Linq;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.SA;

namespace SAUtils.Omim
{
    public static class Main
    {

        private static string _mimToGeneFile;
        private static string _omimJsonFile;
        private static string _outputDirectory;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "m2g|m=",
                    "MimToGeneSymbol tsv file",
                    v => _mimToGeneFile = v
                },
                {
                    "json|j=",
                    "OMIM entry json file",
                    v => _omimJsonFile = v
                },
                {
                    "out|o=",
                    "output directory",
                    v => _outputDirectory = v
                }
            };

            string commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .CheckInputFilenameExists(_mimToGeneFile, "MimToGeneSymbol tsv file", "--m2g")
                .CheckInputFilenameExists(_omimJsonFile, "OMIM entry json file", "--json")
                .SkipBanner()
                .ShowHelpMenu("Creates a gene annotation database from OMIM data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var omimSchema = OmimSchema.Get();

            var omimParser = new OmimParser(_mimToGeneFile, _omimJsonFile, omimSchema);
            var version = omimParser.GetVersion();
            string outFileName = $"{version.Name}_{version.Version}";
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.GeneFileSuffix)))
            using (var ngaWriter = new NgaWriter(nsaStream, version, SaCommon.OmimTag, SaCommon.SchemaVersion, true))
            using (var saJsonSchemaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.GeneFileSuffix + SaCommon.JsonSchemaSuffix)))
            using (var schemaWriter = new StreamWriter(saJsonSchemaStream))
            {
                IEnumerable<OmimItem>                           omimItems   = omimParser.GetItems();
                Dictionary<string, List<ISuppGeneItem>> geneToItems = OmimUtilities.GetGeneToOmimEntriesAndSchema(omimItems);
                ngaWriter.Write(geneToItems);
                schemaWriter.Write(omimSchema);
                
                JObject jo = JObject.Parse(omimParser.OmimStats.ToString());
                Console.WriteLine(jo); //pretty printing json
            }

            return ExitCodes.Success;
        }
    }
}