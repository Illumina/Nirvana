using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Downloader.FileExtensions;
using Downloader.Utilities;
using Genome;

namespace Downloader
{
    public static class OutputDirectory
    {
        public static (string Cache, string Reference, string SupplementaryAnnotation, List<string> OutputDirectories) Create(string outputDirectory, List<GenomeAssembly> genomeAssemblies)
        {
            string cacheDirectory      = Path.Combine(outputDirectory, "Cache");
            string referencesDirectory = Path.Combine(outputDirectory, "References");
            string saDirectory         = Path.Combine(outputDirectory, "SupplementaryAnnotation");

            var outputDirectories = new List<string> {referencesDirectory};

            CreateGenomeAssemblySubdirectories(cacheDirectory, genomeAssemblies, outputDirectories);
            CreateGenomeAssemblySubdirectories(saDirectory,    genomeAssemblies, outputDirectories);
            Directory.CreateDirectory(referencesDirectory);

            return (cacheDirectory, referencesDirectory, saDirectory, outputDirectories);
        }

        private static void CreateGenomeAssemblySubdirectories(string topLevelDirectory, IEnumerable<GenomeAssembly> genomeAssemblies, ICollection<string> outputDirectories)
        {
            foreach (var genomeAssembly in genomeAssemblies)
            {
                string directory = Path.Combine(topLevelDirectory, genomeAssembly.ToString());
                outputDirectories.Add(directory);
                Directory.CreateDirectory(directory);
            }
        }

        public static void Cleanup(IEnumerable<RemoteFile> files, IEnumerable<string> outputDirectories, string referencesDirectory)
        {
            IEnumerable<string> existingFiles  = GetExistingFiles(outputDirectories);
            IEnumerable<string> referenceFiles = GetReferenceFiles(referencesDirectory);
            List<string>        desiredFiles   = files.Select(x => x.LocalPath).ToList();
            List<string>        filesToDelete  = existingFiles.Except(desiredFiles).Except(referenceFiles).ToList();

            if (filesToDelete.Count == 0) return;

            Console.WriteLine("- removing extra files in output directories");

            foreach (string file in filesToDelete)
            {
                Console.WriteLine($"  - deleting extra file: {file}");
                File.Delete(file);
            }

            Console.WriteLine();
        }

        private static IEnumerable<string> GetReferenceFiles(string referencesDirectory) => new List<string>
        {
            Path.Combine(referencesDirectory, ReferencesFileExtensions.GetFilename(GenomeAssembly.GRCh37)),
            Path.Combine(referencesDirectory, ReferencesFileExtensions.GetFilename(GenomeAssembly.GRCh38))
        };

        private static IEnumerable<string> GetExistingFiles(IEnumerable<string> outputDirectories)
        {
            var existingFiles = new List<string>();

            foreach (string outputDir in outputDirectories)
            {
                string[] files = Directory.GetFiles(outputDir, "*", SearchOption.TopDirectoryOnly);

                foreach (string localPath in files)
                {
                    if (!localPath.StartsWith(outputDir)) continue;
                    existingFiles.Add(localPath);
                }
            }

            return existingFiles;
        }

        public static void RemoveOldFiles(IEnumerable<RemoteFile> files)
        {
            var filesToDelete = new List<RemoteFile>();

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file.LocalPath);
                if (!fileInfo.Exists || file.Skipped) continue;

                if (HasDifferentFileSize(fileInfo.Length, file.FileSize) ||
                    HasOlderFile(fileInfo.CreationTimeUtc, file.LastModified))
                {
                    filesToDelete.Add(file);
                    continue;
                }

                // these files already exist and can be skipped
                file.Skipped = true;
            }

            if (filesToDelete.Count == 0) return;

            Console.WriteLine("- removing old files:");
            foreach (var file in filesToDelete)
            {
                Console.WriteLine($"  - deleting {file.Description}");
                File.Delete(file.LocalPath);
            }

            Console.WriteLine();
        }

        private static bool HasOlderFile(in DateTimeOffset localOffset, DateTimeOffset remoteOffset) =>
            DateTimeOffset.Compare(remoteOffset, localOffset) == 1;

        private static bool HasDifferentFileSize(long localLength, long remoteLength) => localLength != remoteLength;

        public static long GetNumDownloadBytes(IEnumerable<RemoteFile> files)
        {
            long numBytes = 0;
            foreach (var file in files) numBytes += file.FileSize;
            return numBytes;
        }

        public static List<RemoteFile> RemoveSkippedFiles(List<RemoteFile> files)
        {
            var filesToDownload = new List<RemoteFile>(files.Count);

            foreach (var file in files.OrderBy(x => x.FileSize))
            {
                if (file.Skipped) continue;
                filesToDownload.Add(file);
            }

            return filesToDownload;
        }

        public static void CheckFiles(IEnumerable<RemoteFile> files)
        {
            var divider = new string('-', 75);
            
            Console.WriteLine("Description                                                     Status");
            Console.WriteLine(divider);
            
            foreach (var file in files.OrderBy(x => x.Description))
            {
                string description = GetPaddedField(file.Description, 58);
                Console.Write($"{description} ");
                PrintStatus(file);
                Console.WriteLine();
            }
            
            Console.WriteLine(divider);
        }
        
        private static string GetPaddedField(string s, int fieldLength)
        {
            if (s.Length > fieldLength) return s.Substring(0, fieldLength - 3) + "...";
            return s.PadRight(fieldLength, ' ');
        }

        private static void PrintStatus(RemoteFile file)
        {
            if (file.Missing)
            {
                ConsoleEmbellishments.PrintWarning("Missing (server)");
                return;
            }
            
            var fileInfo = new FileInfo(file.LocalPath);

            if (!fileInfo.Exists)
            {
                ConsoleEmbellishments.PrintError("Missing (local)");
                return;
            }

            if (fileInfo.Length < file.FileSize)
            {
                ConsoleEmbellishments.PrintError("    Truncated");
                return;
            }
            
            if (fileInfo.Length > file.FileSize)
            {
                ConsoleEmbellishments.PrintError("    Too large");
                return;
            }
            
            ConsoleEmbellishments.PrintSuccess("       OK");
            file.Pass = true;
        }
    }
}
