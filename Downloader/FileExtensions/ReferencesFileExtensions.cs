using System.Collections.Generic;
using System.IO;
using Genome;
using VariantAnnotation.Sequence;

namespace Downloader.FileExtensions
{
    public static class ReferencesFileExtensions
    {
        public static List<RemoteFile> AddReferenceFiles(this List<RemoteFile> files,
            IEnumerable<GenomeAssembly> genomeAssemblies, string remoteReferencesDirectory, string referencesDirectory)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var genomeAssembly in genomeAssemblies)
            {
                string filename    = $"Homo_sapiens.{genomeAssembly}.Nirvana.dat";
                string remotePath  = $"{remoteReferencesDirectory}/{CompressedSequenceCommon.HeaderVersion}/{filename}";
                string localPath   = Path.Combine(referencesDirectory, filename);
                string description = filename;
                files.Add(new RemoteFile(remotePath, localPath, description));
            }

            return files;
        }
    }
}
