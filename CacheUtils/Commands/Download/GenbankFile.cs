using System.Collections.Generic;
using System.IO;
using CacheUtils.Genbank;
using CacheUtils.Utilities;
using Compression.Utilities;
using IO;

namespace CacheUtils.Commands.Download
{
    public sealed class GenbankFile
    {
        public readonly RemoteFile RemoteFile;
        public readonly Dictionary<string, GenbankEntry> GenbankDict;

        public GenbankFile(int num)
        {
            RemoteFile  = new RemoteFile($"RefSeq Genbank {num} gbff", $"ftp://ftp.ncbi.nlm.nih.gov/refseq/H_sapiens/mRNA_Prot/human.{num}.rna.gbff.gz", false);
            GenbankDict = new Dictionary<string, GenbankEntry>();
        }

        public void Parse()
        {
            Logger.WriteLine($"- parsing {Path.GetFileName(RemoteFile.FilePath)}");

            using (var reader = new GenbankReader(GZipUtilities.GetAppropriateStreamReader(RemoteFile.FilePath)))
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
