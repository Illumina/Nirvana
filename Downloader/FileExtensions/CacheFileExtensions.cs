using System.Collections.Generic;
using System.IO;
using Genome;
using VariantAnnotation.IO.Caches;

namespace Downloader.FileExtensions
{
    public static class CacheFileExtensions
    {
        public static List<RemoteFile> AddCacheFiles(this List<RemoteFile> files,
            IEnumerable<GenomeAssembly> genomeAssemblies, string remoteCacheDirectory, string cacheDirectory)
        {
            foreach (var genomeAssembly in genomeAssemblies)
            {
                files.AddCache(genomeAssembly, remoteCacheDirectory, cacheDirectory, "transcripts");
                files.AddCache(genomeAssembly, remoteCacheDirectory, cacheDirectory, "sift");
                files.AddCache(genomeAssembly, remoteCacheDirectory, cacheDirectory, "polyphen");
            }

            return files;
        }

        private static void AddCache(this ICollection<RemoteFile> files, GenomeAssembly genomeAssembly,
            string remoteCacheDirectory, string cacheDirectory, string type)
        {
            string filename    = $"Both.{type}.ndb";
            string remotePath  = $"{remoteCacheDirectory}/{CacheConstants.DataVersion}/{genomeAssembly}/{filename}";
            string localPath   = Path.Combine(cacheDirectory, genomeAssembly.ToString(), filename);
            string description = $"{filename} ({genomeAssembly})";
            files.Add(new RemoteFile(remotePath, localPath, description));
        }
    }
}
