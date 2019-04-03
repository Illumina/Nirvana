using System.IO;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.SAUtils.CreateOmimTsv;
using VariantAnnotation.SA;

namespace SAUtils.Omim
{
    public static class Main
    {
        private static string _geneMap2File;
        private static string _map2GenesFile;
        private static string _universalGeneArchivePath;
        private static string _outputDirectory;
        private static string _inputReferencePath;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "gm2|g=",
                    "genemap2 tsv file",
                    v => _geneMap2File = v
                },
                {
                    "m2g|m=",
                    "map2Genes tsv file",
                    v => _map2GenesFile = v
                },
                {
                    "uga|u=",
                    "universal gene archive {path}",
                    v => _universalGeneArchivePath = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => _inputReferencePath = v
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
                .CheckInputFilenameExists(_map2GenesFile, "Map2Genes file", "--m2g")
                .HasRequiredParameter(_geneMap2File, "genemap2 file", "--gm2")
                .CheckInputFilenameExists(_geneMap2File, "OneKGenSv VCFfile", "--in")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .CheckInputFilenameExists(_inputReferencePath, "compressed reference", "--ref")
                .CheckInputFilenameExists(_universalGeneArchivePath, "universal gene archive", "--uga")
                .SkipBanner()
                .ShowHelpMenu("Creates a gene annotation database from OMIM data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var version = DataSourceVersionReader.GetSourceVersion(_geneMap2File + ".version");

            string outFileName = $"{version.Name}_{version.Version}";

            //create universal gene archive
            var (entrezGeneIdToSymbol, ensemblGeneIdToSymbol) = OmimUtilities.ParseUniversalGeneArchive(_inputReferencePath, _universalGeneArchivePath);
            var geneSymbolUpdater = new GeneSymbolUpdater(entrezGeneIdToSymbol, ensemblGeneIdToSymbol);

            using (var geneMapParser = new OmimParser(GZipUtilities.GetAppropriateStreamReader(_geneMap2File), geneSymbolUpdater))
            using (var map2GeneParser = new OmimParser(GZipUtilities.GetAppropriateStreamReader(_map2GenesFile), geneSymbolUpdater))
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.NgaFileSuffix)))
            using (var ngaWriter = new NgaWriter(nsaStream, version, SaCommon.OmimTag, SaCommon.SchemaVersion, true))
            {
                var omimItems = geneMapParser.GetItems().Concat(map2GeneParser.GetItems());
                ngaWriter.Write(OmimUtilities.GetGeneToOmimEntries(omimItems));
            }


            geneSymbolUpdater.DisplayStatistics();
            using (var writer =new StreamWriter(FileUtilities.GetCreateStream("UpdatedGeneSymbols.txt")))
            {
                geneSymbolUpdater.WriteUpdatedGeneSymbols(writer);
            }
            
            return ExitCodes.Success;
        }

        

    }
}