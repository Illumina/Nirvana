using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using Downloader.FileExtensions;
using ErrorHandling;
using Genome;
using VariantAnnotation.Interface;
using GenomeAssemblyHelper = Downloader.Utilities.GenomeAssemblyHelper;

namespace Downloader
{
    public static class DownloaderMain
    {
        private static string _genomeAssembly;
        private static string _outputDirectory;

        private static ExitCodes ProgramExecution()
        {
            (string hostName, string remoteCacheDir, string remoteReferencesDir, string manifestGRCh37,
                string manifestGRCh38) = Configuration.Load();

            List<GenomeAssembly> genomeAssemblies = GenomeAssemblyHelper.GetGenomeAssemblies(_genomeAssembly);

            var client = new Client(hostName);
            
            Console.Write("- downloading manifest... ");
            
            Dictionary<GenomeAssembly, List<string>> remotePathsByGenomeAssembly =
                Manifest.GetRemotePaths(client, genomeAssemblies, manifestGRCh37, manifestGRCh38);

            (string cacheDir, string referencesDir, string saDir, List<string> outputDirectories) =
                OutputDirectory.Create(_outputDirectory, genomeAssemblies);

            var fileList = new List<RemoteFile>();
            fileList.AddCacheFiles(genomeAssemblies, remoteCacheDir, cacheDir)
                .AddReferenceFiles(genomeAssemblies, remoteReferencesDir, referencesDir)
                .AddSupplementaryAnnotationFiles(remotePathsByGenomeAssembly, saDir);

            Console.WriteLine($"{fileList.Count} files.\n");
            
            // get rid of extra files in the output directories
            OutputDirectory.Cleanup(fileList, outputDirectories);
            
            // get length, checksum, and checks existence
            Console.WriteLine("- downloading file metadata:");
            AnnotationRepository.DownloadMetadata(client, fileList);
            
            // remove obsolete files from the output directory
            OutputDirectory.RemoveOldFiles(fileList);
            
            // remove skipped files from our list
            List<RemoteFile> filesToDownload = OutputDirectory.RemoveSkippedFiles(fileList);
            
            // download the latest files
            if (filesToDownload.Count > 0)
            {
                long numBytesToDownload = OutputDirectory.GetNumDownloadBytes(filesToDownload);
                Console.WriteLine($"- downloading files ({MemoryUtilities.ToHumanReadable(numBytesToDownload)}):");
                
                AnnotationRepository.DownloadFiles(client, filesToDownload);
            }
            
            // sanity check
            OutputDirectory.CheckFiles(fileList);

            bool foundError = fileList.Any(x => !x.Pass);
            return foundError ? ExitCodes.InvalidData : ExitCodes.Success;
        }

        public static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "ga=",
                    "genome assembly {version}",
                    v => _genomeAssembly = v
                },
                {
                    "out|o=",
                    "top-level output {directory}",
                    v => _outputDirectory = v
                }
            };

            var exitCode = new ConsoleAppBuilder(args, ops)
                .Parse()
                .HasRequiredParameter(_genomeAssembly, "genome assembly", "--ga")
                .CheckDirectoryExists(_outputDirectory, "top-level output directory", "--out")
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Downloads the Nirvana data files from S3",
                    "--ga <genome assembly> --out <output directory>")
                .ShowErrors()
                .Execute(ProgramExecution);

            return (int) exitCode;
        }
    }
}