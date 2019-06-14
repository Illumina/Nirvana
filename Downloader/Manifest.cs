using System.Collections.Generic;
using Genome;

namespace Downloader
{
    public static class Manifest
    {
        public static Dictionary<GenomeAssembly, List<string>> GetRemotePaths(IClient client,
            IEnumerable<GenomeAssembly> genomeAssemblies, string manifestGRCh37, string manifestGRCh38)
        {
            IEnumerable<(GenomeAssembly GenomeAssembly, string ManifestPath)> genomeAssemblyPaths =
                CreateGenomeAssemblyPaths(manifestGRCh37, manifestGRCh38, genomeAssemblies);

            var remotePathsByGenomeAssembly = new Dictionary<GenomeAssembly, List<string>>();

            foreach ((var genomeAssembly, string manifestPath) in genomeAssemblyPaths)
            {
                List<string> remotePaths = client.DownloadLinesAsync(manifestPath).ConfigureAwait(false).GetAwaiter().GetResult();
                remotePathsByGenomeAssembly[genomeAssembly] = remotePaths;
            }

            return remotePathsByGenomeAssembly;
        }

        internal static IEnumerable<(GenomeAssembly GenomeAssembly, string ManifestPath)> CreateGenomeAssemblyPaths(
            string manifestGRCh37, string manifestGRCh38, IEnumerable<GenomeAssembly> genomeAssemblies)
        {
            var genomeAssemblyPaths = new List<(GenomeAssembly, string)>();

            foreach (var genomeAssembly in genomeAssemblies)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (genomeAssembly)
                {
                    case GenomeAssembly.GRCh37:
                        genomeAssemblyPaths.Add((genomeAssembly, manifestGRCh37));
                        break;
                    case GenomeAssembly.GRCh38:
                        genomeAssemblyPaths.Add((genomeAssembly, manifestGRCh38));
                        break;
                }
            }

            return genomeAssemblyPaths;
        }
    }
}
