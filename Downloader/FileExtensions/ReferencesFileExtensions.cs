using System.Collections.Generic;
using System.IO;
using Genome;
using ReferenceSequence;

namespace Downloader.FileExtensions
{
    public static class ReferencesFileExtensions
    {
        public static List<RemoteFile> AddReferenceFiles(this List<RemoteFile> files, IEnumerable<GenomeAssembly> genomeAssemblies,
            string remoteReferencesDirectory, string referencesDirectory)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (GenomeAssembly genomeAssembly in genomeAssemblies)
            {
                string filename   = GetFilename(genomeAssembly);
                var    remotePath = $"{remoteReferencesDirectory}/{ReferenceSequenceCommon.HeaderVersion}/{filename}";
                string localPath  = Path.Combine(referencesDirectory, filename);
                files.Add(new RemoteFile(remotePath, localPath, filename));
            }

            return files;
        }

        public static string GetFilename(GenomeAssembly genomeAssembly) => $"Homo_sapiens.{genomeAssembly}.Nirvana.dat";
    }
}