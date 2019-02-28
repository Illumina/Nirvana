using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using Genome;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.CreateMitoMapDb
{
    public static class StructVarDb
    {
        private static string _compressedReference;
        private static string _outputDirectory;
        private static readonly List<string> MitoMapFileNames = new List<string>();

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "ref|r=",
                    "compressed reference sequence file",
                    v => _compressedReference = v
                },
                {
                    "in|i=",
                    "MITOMAP structural variants HTML file",
                    v => MitoMapFileNames.Add(v)
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
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
                .HasRequiredParameter(MitoMapFileNames, "MITOMAP structural variants HTML file", "--in")
                .CheckEachFilenameExists(MitoMapFileNames, "MITOMAP structural variants HTML file", "--in")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database with MITOMAP structural variants annotations", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }
        private static ExitCodes ProgramExecution()
        {
            var rootDirectory = new FileInfo(MitoMapFileNames[0]).Directory;
            if (rootDirectory == null) return ExitCodes.PathNotFound;
            var version = DataSourceVersionReader.GetSourceVersion(Path.Combine(rootDirectory.ToString(), "mitoMapSv"));
            var sequenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            var chrom = sequenceProvider.RefNameToChromosome["chrM"];
            sequenceProvider.LoadChromosome(chrom);
            var mitoMapSvReaders = MitoMapFileNames.Select(mitoMapFileName => new MitoMapSvReader(new FileInfo(mitoMapFileName), sequenceProvider)).ToList();
            var mergedMitoMapVarItems = MitoMapSvReader.MergeAndSort(mitoMapSvReaders);

            string outFileName = $"{version.Name}_{version.Version}";
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SiFileSuffix)))
            {
                var nsiWriter = new NsiWriter(new ExtendedBinaryWriter(nsaStream), version, GenomeAssembly.rCRS, SaCommon.MitoMapTag, ReportFor.StructuralVariants, SaCommon.SchemaVersion);
                nsiWriter.Write(mergedMitoMapVarItems);
            }

            return ExitCodes.Success;
        }
    }
}