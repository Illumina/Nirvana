using System.Collections.Generic;
using System.IO;
using CacheUtils.Genbank;
using CacheUtils.Utilities;
using Compression.Utilities;
using VariantAnnotation.Interface;

namespace CacheUtils.Commands.UniversalGeneArchive
{
    public sealed class GenbankFile
    {
        private readonly ILogger _logger;
        private readonly RemoteFile _remoteFile;
        public readonly Dictionary<string, GenbankEntry> GenbankDict;

        public GenbankFile(ILogger logger, int num)
        {
            _logger     = logger;
            _remoteFile = new RemoteFile($"RefSeq Genbank {num}", $"ftp://ftp.ncbi.nlm.nih.gov/refseq/H_sapiens/mRNA_Prot/human.{num}.rna.gbff.gz", false);
            GenbankDict = new Dictionary<string, GenbankEntry>();
        }

        public void Download() => _remoteFile.Download(_logger);

        public void Parse()
        {
            _logger.WriteLine($"- parsing {Path.GetFileName(_remoteFile.FilePath)}");

            using (var reader = new GenbankReader(GZipUtilities.GetAppropriateStreamReader(_remoteFile.FilePath)))
            {
                while (true)
                {
                    var entry = reader.GetGenbankEntry();
                    if (entry == null) break;
                    GenbankDict[entry.TranscriptId] = entry;
                }
            }
        }
    }
}
