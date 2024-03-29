﻿using System.Collections.Generic;
using System.IO;
using Genome;

namespace Downloader.FileExtensions
{
    public static class SupplementaryAnnotationFileExtensions
    {
        private static readonly HashSet<string> NeedsIndexSet = new HashSet<string>();

        static SupplementaryAnnotationFileExtensions()
        {
            NeedsIndexSet.Add(".nsa");
            NeedsIndexSet.Add(".npd");
            NeedsIndexSet.Add(".rma");
            NeedsIndexSet.Add(".gsa");
        }
        
        public static void AddSupplementaryAnnotationFiles(this List<RemoteFile> files,
            Dictionary<GenomeAssembly, List<string>> remotePathsByGenomeAssembly, string saDirectory)
        {
            foreach ((var genomeAssembly, List<string> remotePaths) in remotePathsByGenomeAssembly)
            {
                files.AddDataSources(remotePaths, genomeAssembly, saDirectory);
            }
        }

        private static void AddDataSources(this ICollection<RemoteFile> files, IEnumerable<string> remotePaths, GenomeAssembly genomeAssembly, string saDirectory)
        {
            foreach (string path in remotePaths)
            {
                files.AddFile(genomeAssembly, saDirectory, path);
                string extension = Path.GetExtension(path);
                if (NeedsIndexSet.Contains(extension)) files.AddFile(genomeAssembly, saDirectory, path + ".idx");
            }
        }

        private static void AddFile(this ICollection<RemoteFile> files, GenomeAssembly genomeAssembly, string saDirectory, string path)
        {
            string filename    = Path.GetFileName(path);
            string remotePath  = path;
            string localPath   = Path.Combine(saDirectory, genomeAssembly.ToString(), filename);
            string description = $"{filename} ({genomeAssembly})";
            files.Add(new RemoteFile(remotePath, localPath, description));
        }
    }
}
