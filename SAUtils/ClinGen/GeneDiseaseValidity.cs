using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using OptimizedCore;
using SAUtils.InputFileParsers;
using VariantAnnotation.SA;

namespace SAUtils.ClinGen
{
    public static class GeneDiseaseValidity
    {
        private static string _outputDirectory;
        private static string _ugaFile;
        private static string _diseaseValidityFile;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "tsv|t=",
                    "ClinGen gene validity file path",
                    v => _diseaseValidityFile = v
                },
                {
                    "uga|u=",
                    "UGA file path",
                    v => _ugaFile = v
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
                .CheckInputFilenameExists(_diseaseValidityFile, "disease validity TSV file", "--tsv")
                .CheckInputFilenameExists(_ugaFile, "UGA file path", "--uga")
                .SkipBanner()
                .ShowHelpMenu("Creates a gene annotation database from ClinGen gene validity data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var dosageSensitivityVersion = DataSourceVersionReader.GetSourceVersion(_diseaseValidityFile + ".version");

            string outFileName = $"{dosageSensitivityVersion.Name.Replace(' ', '_')}_{dosageSensitivityVersion.Version}";

            // read uga file to get hgnc id to gene symbols dictionary
            using (var diseaseValidityParser = new GeneDiseaseValidityParser(GZipUtilities.GetAppropriateReadStream(_diseaseValidityFile), GetHgncIdToGeneSymbols()))
            using (var stream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.GeneFileSuffix)))
            using (var ngaWriter = new NgaWriter(stream, dosageSensitivityVersion, SaCommon.DiseaseValidityTag, SaCommon.SchemaVersion, true))
            {
                ngaWriter.Write(diseaseValidityParser.GetItems());
            }

            return ExitCodes.Success;
        }

        private static Dictionary<int, string> GetHgncIdToGeneSymbols()
        {
            var idToSymbols = new Dictionary<int, string>();
            
            using (var ugaStream = GZipUtilities.GetAppropriateReadStream(_ugaFile))
            using(var reader = new StreamReader(ugaStream))
            {
                string line= reader.ReadLine();//first line has the count of entries
                while ((line = reader.ReadLine()) != null)
                {
                    var splits = line.OptimizedSplit('\t');
                    var symbol = splits[2];
                    var hgncId = int.Parse(splits[8]);
                    if(hgncId == -1) continue;
                    
                    if (idToSymbols.TryAdd(hgncId, symbol)) continue;
                    if(symbol != idToSymbols[hgncId]) Console.WriteLine($"Different symbol for the same id({hgncId}). Existing: {idToSymbols[hgncId]}. New: {symbol}");

                }
            }

            return idToSymbols;
        }
    }
}