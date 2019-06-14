using System.Collections.Generic;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Downloader.FileExtensions;
using ErrorHandling;
using Genome;
using VariantAnnotation.Interface;

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
            Dictionary<GenomeAssembly, List<string>> remotePathsByGenomeAssembly =
                Manifest.GetRemotePaths(client, genomeAssemblies, manifestGRCh37, manifestGRCh38);

            (string cacheDir, string referencesDir, string saDir) =
                OutputDirectory.Create(_outputDirectory, genomeAssemblies);

            var fileList = new List<RemoteFile>();
            fileList.AddCacheFiles(genomeAssemblies, remoteCacheDir, cacheDir)
                .AddReferenceFiles(genomeAssemblies, remoteReferencesDir, referencesDir)
                .AddSupplementaryAnnotationFiles(remotePathsByGenomeAssembly, saDir)
                .Download(client);

            return ExitCodes.Success;
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
                .ShowHelpMenu("Downloads the Nirvana data files from S3", "--ga <genome assembly> --out <output directory>")
                .ShowErrors()
                .Execute(ProgramExecution);

            return (int)exitCode;
        }
    }
}