using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using ErrorHandling.Exceptions;
using IO;
using OptimizedCore;
using SAUtils.InputFileParsers;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
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
                    "out|o=",
                    "output directory for TSVs",
                    v => _outputDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
                .HasRequiredParameter(_genomeDirectory, "input directory containing genome vcf files", "--genome")
                .CheckDirectoryExists(_genomeDirectory, "input directory containing genome vcf files", "--genome")
                .HasRequiredParameter(_exomeDirectory, "input directory containing exome vcf files", "--exome")
                .CheckDirectoryExists(_genomeDirectory, "input directory containing exome vcf files", "--exome")
                .HasRequiredParameter(_outputDirectory, "output Supplementary directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output Supplementary directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Reads provided supplementary data files and populates tsv files", commandLineExample)
                .ShowErrors()
                .Execute(creator.ProgramExecution);

            return exitCode;
        }
        private ExitCodes ProgramExecution()
        {
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));

            var genomeFiles = GetVcfFiles(_genomeDirectory);
            var exomeFiles  = GetVcfFiles(_exomeDirectory);
            var version     = GetVersion();
            
            Console.WriteLine($"Creating merged gnomAD database file from {genomeFiles.Length + exomeFiles.Length} input files");
            
            string outFileName = $"{version.Name}_{version.Version}";
            
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix)))
            using (var indexStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix + SaCommon.IndexSufix)))
            using (var nsaWriter = new NsaWriter(new ExtendedBinaryWriter(nsaStream), new ExtendedBinaryWriter(indexStream), version, referenceProvider, SaCommon.GnomadTag, true, false, SaCommon.SchemaVersion, false))
            {
                nsaWriter.Write(GetItems(genomeFiles,exomeFiles, referenceProvider));
            }

            return ExitCodes.Success;
        }

        private static IEnumerable<ISupplementaryDataItem> GetItems(string[] genomeFiles, string[] exomeFiles, ISequenceProvider referenceProvider)
        {
            foreach (var fileName in genomeFiles)
            {
                var exomeReader  = GetExomeReader(exomeFiles, fileName);
                var genomeReader = GZipUtilities.GetAppropriateStreamReader(fileName);
                var reader = new GnomadReader(genomeReader, exomeReader, referenceProvider);

                foreach (var item in reader.GetCombinedItems())
                {
                    yield return item;
                }
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