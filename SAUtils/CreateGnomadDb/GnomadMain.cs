using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using ErrorHandling.Exceptions;
using IO;
using OptimizedCore;
using SAUtils.InputFileParsers;
using SAUtils.NsaConcatenator;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.CreateGnomadDb
{
    public sealed class GnomadMain
    {
        private static string _genomeDirectory;
        private static string _exomeDirectory;
        private static string _compressedReference;
        private static string _outputDirectory;
        private static string _tempDirectory;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var creator = new GnomadMain();
            var ops = new OptionSet
            {
                {
                     "ref|r=",
                     "compressed reference sequence file",
                     v => _compressedReference = v
                 },
                {
                    "genome|g=",
                    "input directory containing VCF (and .version) files with genomic frequencies",
                    v => _genomeDirectory = v
                },
                {
                    "exome|e=",
                    "input directory containing VCF (and .version) files with exomic frequencies",
                    v => _exomeDirectory = v
                },
                {
                    "temp|t=",
                    "output temp directory for intermediate (per chrom) NSA files",
                    v => _tempDirectory = v
                },
                {
                    "out|o=",
                    "output directory for NSA file",
                    v => _outputDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
                .CheckDirectoryExists(_genomeDirectory, "input directory containing genome vcf files", "--genome")
                .CheckDirectoryExists(_exomeDirectory, "input directory containing exome vcf files", "--exome")
                .CheckDirectoryExists(_outputDirectory, "output Supplementary directory", "--out")
                .CheckDirectoryExists(_tempDirectory, "output temp directory for intermediate (per chrom) NSA files", "--temp")
                .SkipBanner()
                .ShowHelpMenu("Reads provided supplementary data files and populates tsv files", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }
        
        private static ExitCodes ProgramExecution()
        {
            //clearing temp directory
            Console.WriteLine($"Cleaning {SaCommon.SaFileSuffix} and {SaCommon.IndexSufix} files from temp directory {_tempDirectory}");
            foreach (var file in Directory.GetFiles(_tempDirectory, $"*{SaCommon.SaFileSuffix}"))
            {
                File.Delete(file);
            }
            foreach (var file in Directory.GetFiles(_tempDirectory, $"*{SaCommon.SaFileSuffix}{SaCommon.IndexSufix}"))
            {
                File.Delete(file);
            }

            var version     = GetVersion();

            var genomeFiles = GetVcfFiles(_genomeDirectory);
            var exomeFiles = GetVcfFiles(_exomeDirectory);
            const int degOfParalleleism = 4; //hard coding since we are IO bound and stressing the disk doesn't help
            Console.WriteLine($"Creating merged gnomAD database file from {genomeFiles.Length + exomeFiles.Length} input files. Degree of parallelism {degOfParalleleism}");

            Parallel.ForEach(
                genomeFiles,
                new ParallelOptions { MaxDegreeOfParallelism = degOfParalleleism },
                genomeFile => CreateNsa(exomeFiles, genomeFile, version)
                );
            string outFileName = Path.Combine(_outputDirectory, $"{version.Name}_{version.Version}");

            //concat the nsa files
            Console.WriteLine("Concatenating per chromosome nsa files");
            var tempNsaFiles = Directory.GetFiles(_tempDirectory, $"*{SaCommon.SaFileSuffix}");
            ConcatUtilities.ConcatenateNsaFiles(tempNsaFiles, outFileName);

            return ExitCodes.Success;
        }

        private static void CreateNsa(string[] exomeFiles, string genomeFile, DataSourceVersion version) {
            Console.WriteLine($"Processing file: {genomeFile}");
            var outName = Path.GetFileNameWithoutExtension(genomeFile);

            using (var exomeReader = GetExomeReader(exomeFiles, genomeFile))
            using (var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference)))
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_tempDirectory, outName + SaCommon.SaFileSuffix)))
            using (var indexStream = FileUtilities.GetCreateStream(Path.Combine(_tempDirectory, outName + SaCommon.SaFileSuffix + SaCommon.IndexSufix)))
            using (var nsaWriter = new NsaWriter(nsaStream, indexStream, version, referenceProvider, SaCommon.GnomadTag, true, false, SaCommon.SchemaVersion, false))
            using (var reader = GZipUtilities.GetAppropriateStreamReader(genomeFile))
            {
                var gnomadReader = new GnomadReader(reader, exomeReader, referenceProvider);
                var count = nsaWriter.Write(gnomadReader.GetCombinedItems());
                Console.WriteLine($"Wrote {count} items to NSA file.");
            }
        }

        private static StreamReader GetExomeReader(string[] exomeFileNames, string genomeFileName)
        {
            if (exomeFileNames == null || exomeFileNames.Length == 0) return null;
            string chromName = GetChromName(genomeFileName);
            string exomeFileName = null;
            foreach (string fileName in exomeFileNames)
            {
                string exomeChrom = GetChromName(fileName);
                if (chromName != exomeChrom) continue;
                exomeFileName = fileName;
                break;
            }
            return string.IsNullOrEmpty(exomeFileName) ? null : GZipUtilities.GetAppropriateStreamReader(exomeFileName);
        }

        private static string GetChromName(string filePath)
        {
            // the files are named in a consistent format that allows us to match files by chrom names
            // e.g. gnomad.exomes.r2.1.sites.grch38.chr1_noVEP.vcf.gz or chr18.vcf.bgz
            var fileName = Path.GetFileName(filePath);
            foreach (var component in fileName.OptimizedSplit('.'))
            {
                if (component.StartsWith("chr")) return component.OptimizedSplit('_')[0];
            }

            return null;
        }

        private static DataSourceVersion GetVersion()
        {
            var genomeVersionFiles = Directory.GetFiles(_genomeDirectory, "*.version");
            if (genomeVersionFiles.Length != 1)
                throw new InvalidDataException($"Only one .version file should exist in: {_genomeDirectory}");
            var genomeVersion = DataSourceVersionReader.GetSourceVersion(genomeVersionFiles[0]);

            var exomeVersionFiles = Directory.GetFiles(_exomeDirectory, "*.version");
            if (exomeVersionFiles.Length != 1)
                throw new InvalidDataException($"Only one .version file should exist in: {_exomeDirectory}");
            var exomeVersion = DataSourceVersionReader.GetSourceVersion(genomeVersionFiles[0]);

            if (genomeVersion.Version != exomeVersion.Version)
                throw new DataMisalignedException(
                    $"Version mismatch! Genome version: {genomeVersion.Version}, Exome Version: {exomeVersion.Version}.");
            return genomeVersion;
        }

        private static string[] GetVcfFiles(string directory)
        {
            // the files might have gz or bgz extensions
            var files = Directory.GetFiles(directory, "*.vcf.bgz");
            if(files.Length == 0)
                files = Directory.GetFiles(directory, "*.vcf.gz");

            if (files.Length == 0)
                throw new UserErrorException($"{directory} does not contain any VCF files");

            return files;
        }
    }
}