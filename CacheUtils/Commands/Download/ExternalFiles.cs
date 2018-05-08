using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.Genbank;
using CacheUtils.IntermediateIO;
using CacheUtils.Utilities;
using Compression.Utilities;
using Genome;
using IO;
using OptimizedCore;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.Commands.Download
{
    public static class ExternalFiles
    {
        public static readonly RemoteFile CcdsFile     = new RemoteFile("CCDS file (2016-09-08)",   "ftp://ftp.ncbi.nlm.nih.gov/pub/CCDS/current_human/CCDS2Sequence.20160908.txt", false);
        public static readonly RemoteFile LrgFile      = new RemoteFile("latest LRG file",          "http://ftp.ebi.ac.uk/pub/databases/lrgex/list_LRGs_transcripts_xrefs.txt");
        public static readonly RemoteFile HgncFile     = new RemoteFile("latest HGNC gene symbols", "ftp://ftp.ebi.ac.uk/pub/databases/genenames/new/tsv/hgnc_complete_set.txt");
        public static readonly RemoteFile GeneInfoFile = new RemoteFile("latest gene_info",         "ftp://ftp.ncbi.nlm.nih.gov/gene/DATA/gene_info.gz");

        public static readonly RemoteFile AssemblyFile37        = new RemoteFile("assembly report (GRCh37.p13)",    "ftp://ftp.ncbi.nih.gov/genomes/refseq/vertebrate_mammalian/Homo_sapiens/all_assembly_versions/GCF_000001405.25_GRCh37.p13/GCF_000001405.25_GRCh37.p13_assembly_report.txt", false);
        public static readonly RemoteFile EnsemblGtfFile37      = new RemoteFile("Ensembl 75 GTF (GRCh37)",         "ftp://ftp.ensembl.org/pub/release-75/gtf/homo_sapiens/Homo_sapiens.GRCh37.75.gtf.gz", false);
        public static readonly RemoteFile RefSeqGenomeGffFile37 = new RemoteFile("RefSeq genomic GFF (GRCh37.p13)", "ftp://ftp.ncbi.nih.gov/genomes/refseq/vertebrate_mammalian/Homo_sapiens/all_assembly_versions/GCF_000001405.25_GRCh37.p13/GCF_000001405.25_GRCh37.p13_genomic.gff.gz", false);
        public static readonly RemoteFile RefSeqGffFile37       = new RemoteFile("RefSeq GFF3 (GRCh37.p13)",        "ftp://ftp.ncbi.nih.gov/genomes/H_sapiens/ARCHIVE/ANNOTATION_RELEASE.105/GFF/ref_GRCh37.p13_top_level.gff3.gz", false);

        public static readonly RemoteFile AssemblyFile38        = new RemoteFile("assembly report (GRCh38.p11)",    "ftp://ftp.ncbi.nih.gov/genomes/refseq/vertebrate_mammalian/Homo_sapiens/all_assembly_versions/GCF_000001405.37_GRCh38.p11/GCF_000001405.37_GRCh38.p11_assembly_report.txt", false);
        public static readonly RemoteFile EnsemblGtfFile38      = new RemoteFile("Ensembl 90 GTF (GRCh38)",         "ftp://ftp.ensembl.org/pub/release-90/gtf/homo_sapiens/Homo_sapiens.GRCh38.90.gtf.gz", false);
        public static readonly RemoteFile RefSeqGenomeGffFile38 = new RemoteFile("RefSeq genomic GFF (GRCh38.p11)", "ftp://ftp.ncbi.nih.gov/genomes/refseq/vertebrate_mammalian/Homo_sapiens/all_assembly_versions/GCF_000001405.37_GRCh38.p11/GCF_000001405.37_GRCh38.p11_genomic.gff.gz", false);
        public static readonly RemoteFile RefSeqGffFile38       = new RemoteFile("RefSeq GFF3 (GRCh38.p7)",         "ftp://ftp.ncbi.nih.gov/genomes/H_sapiens/GFF/ref_GRCh38.p7_top_level.gff3.gz", false);

        public static readonly string GenbankFilePath = Path.Combine(Path.GetTempPath(), RemoteFile.GetFilename("Genbank.tsv.gz", false));

        public static readonly string UniversalGeneFilePath = Path.Combine(Path.GetTempPath(), RemoteFile.GetFilename("UGA.tsv.gz", false));

        public static void Download(ILogger logger)
        {
            var fileList = new List<RemoteFile>
            {
                CcdsFile,
                LrgFile,
                HgncFile,
                GeneInfoFile,
                AssemblyFile37,
                AssemblyFile38,
                EnsemblGtfFile37,
                EnsemblGtfFile38,
                RefSeqGenomeGffFile37,
                RefSeqGenomeGffFile38,
                RefSeqGffFile37,
                RefSeqGffFile38
            };

            var genbankFiles = GetGenbankFiles(logger, fileList);

            fileList.Execute(logger, "downloads", file => file.Download(logger));

            if (genbankFiles == null) return;

            genbankFiles.Execute(logger, "file parsing", file => file.Parse());
            var genbankEntries = GetIdToGenbankEntryDict(genbankFiles);
            WriteDictionary(logger, genbankEntries);
        }

        private static IEnumerable<GenbankEntry> GetIdToGenbankEntryDict(IEnumerable<GenbankFile> files) =>
            files.SelectMany(file => file.GenbankDict.Values).OrderBy(x => x.TranscriptId).ToList();

        private static List<GenbankFile> GetGenbankFiles(ILogger logger, ICollection<RemoteFile> fileList)
        {
            var genbankFileInfo = new FileInfo(GenbankFilePath);
            if (genbankFileInfo.Exists && GetElapsedDays(genbankFileInfo.CreationTime) < 30.0) return null;

            int numGenbankFiles = GetNumGenbankFiles(logger);
            var genbankFiles    = new List<GenbankFile>(numGenbankFiles);

            for (var i = 0; i < numGenbankFiles; i++)
            {
                var genbankFile = new GenbankFile(logger, i + 1);
                fileList.Add(genbankFile.RemoteFile);
                genbankFiles.Add(genbankFile);
            }

            return genbankFiles;
        }

        public static double GetElapsedDays(DateTime creationTime) => DateTime.Now.Subtract(creationTime).TotalDays;

        private static int GetNumGenbankFiles(ILogger logger)
        {
            var fileList = new RemoteFile("RefSeq filelist", "ftp://ftp.ncbi.nlm.nih.gov/refseq/H_sapiens/mRNA_Prot/human.files.installed");
            fileList.Download(logger);

            var maxNum = 0;

            using (var reader = FileUtilities.GetStreamReader(FileUtilities.GetReadStream(fileList.FilePath)))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;

                    string filename = line.OptimizedSplit('\t')[1];
                    if (!filename.EndsWith(".rna.gbff.gz")) continue;

                    int num = int.Parse(filename.Substring(6, filename.Length - 18));
                    if (num > maxNum) maxNum = num;
                }
            }

            return maxNum;
        }

        private static void WriteDictionary(ILogger logger, IEnumerable<GenbankEntry> entries)
        {
            var header = new IntermediateIoHeader(0, 0, Source.None, GenomeAssembly.Unknown, 0);

            logger.Write($"- writing Genbank file ({Path.GetFileName(GenbankFilePath)})... ");
            using (var writer = new GenbankWriter(GZipUtilities.GetStreamWriter(GenbankFilePath), header))
            {
                foreach (var entry in entries) writer.Write(entry);
            }
            logger.WriteLine("finished.");
        }
    }
}
