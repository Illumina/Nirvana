using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CacheUtils.Genes;
using CacheUtils.Genes.DataStores;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using CacheUtils.Helpers;
using CacheUtils.Utilities;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using Compression.FileHandling;
using Compression.Utilities;
using ErrorHandling;
using VariantAnnotation.Providers;
using Microsoft.Extensions.Configuration;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Logger;
using VariantAnnotation.Utilities;

namespace CacheUtils.Commands.UniversalGeneArchive
{
    public static class UniversalGeneArchiveMain
    {
        private static string _configPath;
        private static string _outputPath;

        private static ExitCodes ProgramExecution()
        {
            var jsonPath  = string.IsNullOrEmpty(_configPath) ? "CacheUtils.dll.gene.json" : _configPath;
            var filePaths = GetFilePaths(jsonPath);
            var logger    = new ConsoleLogger();

            if (!_outputPath.EndsWith(".gz")) _outputPath += ".gz";

            DownloadFiles(logger, filePaths);
            var ds = LoadDataStores(logger, filePaths);

            var grch37GenesByRef = ds.Assembly37.UpdateHgncIds(ds.Hgnc).MergeByHgnc(true);            
            var grch38GenesByRef = ds.Assembly38.UpdateHgncIds(ds.Hgnc).MergeByHgnc(false);

            var universalGenes = CombineGenomeAssemblies(logger, grch37GenesByRef, grch38GenesByRef).UpdateGeneSymbols(logger,
                ds.Hgnc.HgncIdToSymbol, ds.GeneInfoData.EntrezGeneIdToSymbol,
                ds.Assembly38.EnsemblGtf.EnsemblIdToSymbol, ds.Assembly37.RefSeqGff.EntrezGeneIdToSymbol);

            WriteGenes(logger, universalGenes);
            
            return ExitCodes.Success;
        }

        private static void DownloadFiles(ILogger logger, FilePaths filePaths)
        {
            var fileList = new List<RemoteFile>();
            AddCommonFiles(fileList, filePaths);
            AddGrch37Files(fileList, filePaths.GRCh37);
            AddGrch38Files(fileList, filePaths.GRCh38);

            fileList.Execute(logger, "downloads", file => file.Download(logger));
        }

        private static void AddCommonFiles(ICollection<RemoteFile> fileList, FilePaths filePaths)
        {
            var hgncFile     = new RemoteFile("latest HGNC gene symbols", "ftp://ftp.ebi.ac.uk/pub/databases/genenames/new/tsv/hgnc_complete_set.txt");
            var geneInfoFile = new RemoteFile("latest gene_info", "ftp://ftp.ncbi.nlm.nih.gov/gene/DATA/gene_info.gz");

            filePaths.HgncPath     = hgncFile.FilePath;
            filePaths.GeneInfoPath = geneInfoFile.FilePath;

            fileList.Add(hgncFile);
            fileList.Add(geneInfoFile);
        }

        private static void AddGrch37Files(ICollection<RemoteFile> fileList, FilePaths.AssemblySpecificPaths grch37)
        {
            var assemblyFile        = new RemoteFile("assembly report (GRCh37.p13)",    "ftp://ftp.ncbi.nih.gov/genomes/refseq/vertebrate_mammalian/Homo_sapiens/all_assembly_versions/GCF_000001405.25_GRCh37.p13/GCF_000001405.25_GRCh37.p13_assembly_report.txt", false);
            var ensemblGtfFile      = new RemoteFile("Ensembl 75 GTF (GRCh37)",         "ftp://ftp.ensembl.org/pub/release-75/gtf/homo_sapiens/Homo_sapiens.GRCh37.75.gtf.gz", false);
            var refSeqGenomeGffFile = new RemoteFile("RefSeq genomic GFF (GRCh37.p13)", "ftp://ftp.ncbi.nih.gov/genomes/refseq/vertebrate_mammalian/Homo_sapiens/all_assembly_versions/GCF_000001405.25_GRCh37.p13/GCF_000001405.25_GRCh37.p13_genomic.gff.gz", false);
            var refSeqGffFile       = new RemoteFile("RefSeq GFF3 (GRCh37.p13)",        "ftp://ftp.ncbi.nih.gov/genomes/H_sapiens/ARCHIVE/ANNOTATION_RELEASE.105/GFF/ref_GRCh37.p13_top_level.gff3.gz", false);

            grch37.AssemblyInfoPath    = assemblyFile.FilePath;
            grch37.EnsemblGtfPath      = ensemblGtfFile.FilePath;
            grch37.RefSeqGenomeGffPath = refSeqGenomeGffFile.FilePath;
            grch37.RefSeqGffPath       = refSeqGffFile.FilePath;

            fileList.Add(assemblyFile);
            fileList.Add(ensemblGtfFile);
            fileList.Add(refSeqGenomeGffFile);
            fileList.Add(refSeqGffFile);
        }

        private static void AddGrch38Files(ICollection<RemoteFile> fileList, FilePaths.AssemblySpecificPaths grch38)
        {
            var assemblyFile        = new RemoteFile("assembly report (GRCh38.p11)",    "ftp://ftp.ncbi.nih.gov/genomes/refseq/vertebrate_mammalian/Homo_sapiens/all_assembly_versions/GCF_000001405.37_GRCh38.p11/GCF_000001405.37_GRCh38.p11_assembly_report.txt", false);
            var ensemblGtfFile      = new RemoteFile("Ensembl 90 GTF (GRCh38)",         "ftp://ftp.ensembl.org/pub/release-90/gtf/homo_sapiens/Homo_sapiens.GRCh38.90.gtf.gz", false);
            var refSeqGenomeGffFile = new RemoteFile("RefSeq genomic GFF (GRCh38.p11)", "ftp://ftp.ncbi.nih.gov/genomes/refseq/vertebrate_mammalian/Homo_sapiens/all_assembly_versions/GCF_000001405.37_GRCh38.p11/GCF_000001405.37_GRCh38.p11_genomic.gff.gz", false);
            var refSeqGffFile       = new RemoteFile("RefSeq GFF3 (GRCh38.p7)",         "ftp://ftp.ncbi.nih.gov/genomes/H_sapiens/GFF/ref_GRCh38.p7_top_level.gff3.gz", false);

            grch38.AssemblyInfoPath    = assemblyFile.FilePath;
            grch38.EnsemblGtfPath      = ensemblGtfFile.FilePath;
            grch38.RefSeqGenomeGffPath = refSeqGenomeGffFile.FilePath;
            grch38.RefSeqGffPath       = refSeqGffFile.FilePath;

            fileList.Add(assemblyFile);
            fileList.Add(ensemblGtfFile);
            fileList.Add(refSeqGenomeGffFile);
            fileList.Add(refSeqGffFile);
        }

        private static (GeneInfoData GeneInfoData, AssemblyDataStore Assembly37, AssemblyDataStore Assembly38, Hgnc Hgnc)
            LoadDataStores(ILogger logger, FilePaths filePaths)
        {
            logger.Write("- loading datastores... ");
            var loadBenchmark = new Benchmark();

            var dicts = GetSequenceDictionaries(filePaths.GRCh38.ReferencePath, filePaths.GRCh37.AssemblyInfoPath, filePaths.GRCh38.AssemblyInfoPath);

            var geneInfoData = GeneInfoData.Create(filePaths.GeneInfoPath);
            var dataStore37  = AssemblyDataStore.Create("GRCh37", logger, filePaths.GRCh37, dicts.RefNameToChromosome, dicts.Accession37);
            var dataStore38  = AssemblyDataStore.Create("GRCh38", logger, filePaths.GRCh38, dicts.RefNameToChromosome, dicts.Accession38);
            var hgnc         = Hgnc.Create(filePaths.HgncPath, dicts.RefNameToChromosome);

            logger.WriteLine($"{Benchmark.ToHumanReadable(loadBenchmark.GetElapsedTime())}");

            return (geneInfoData, dataStore37, dataStore38, hgnc);
        }

        private static (IDictionary<string, IChromosome> RefNameToChromosome, IDictionary<string, IChromosome>
            Accession37, IDictionary<string, IChromosome> Accession38) GetSequenceDictionaries(string referencePath,
                string assemblyInfo37Path, string assemblyInfo38Path)
        {
            var (_, refNameToChromosome, _) = SequenceHelper.GetDictionaries(referencePath);
            var accession37Dict = AssemblyReader.GetAccessionToChromosome(GZipUtilities.GetAppropriateStreamReader(assemblyInfo37Path), refNameToChromosome);
            var accession38Dict = AssemblyReader.GetAccessionToChromosome(GZipUtilities.GetAppropriateStreamReader(assemblyInfo38Path), refNameToChromosome);
            return (refNameToChromosome, accession37Dict, accession38Dict);
        }

        private static UgaGene[] CombineGenomeAssemblies(ILogger logger, Dictionary<ushort, List<UgaGene>> genesByRef37, Dictionary<ushort, List<UgaGene>> genesByRef38)
        {
            logger.WriteLine();
            logger.WriteLine("*** Global ***");
            logger.Write("- combining genes from GRCh37 and GRCh38... ");
            var combinedGenes = UgaAssemblyCombiner.Combine(genesByRef37, genesByRef38);
            logger.WriteLine($"{combinedGenes.Length} genes.");

            return combinedGenes;
        }

        private static UgaGene[] UpdateGeneSymbols(this UgaGene[] genes, ILogger logger,
            Dictionary<int, string> hgncIdToSymbol, Dictionary<string, string> entrezGeneIdToSymbol,
            Dictionary<string, string> ensemblIdToSymbol, Dictionary<string, string> refseqGeneIdToSymbol)
        {
            var updater = new GeneSymbolUpdater(logger, hgncIdToSymbol, entrezGeneIdToSymbol, ensemblIdToSymbol, refseqGeneIdToSymbol);
            updater.Update(genes);
            return genes;
        }

        private static void WriteGenes(ILogger logger, UgaGene[] genes)
        {
            logger.Write($"- writing genes to {Path.GetFileName(_outputPath)}... ");

            using (var stream = new BlockGZipStream(FileUtilities.GetCreateStream(_outputPath), CompressionMode.Compress))
            using (var writer = new UgaGeneWriter(stream))
            {
                writer.Write(genes);
            }

            logger.WriteLine("finished");
        }

        private static FilePaths GetFilePaths(string jsonPath)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile(jsonPath);

            var configuration = builder.Build();

            var filePaths = new FilePaths();
            configuration.Bind(filePaths);

            CheckPaths(filePaths.GRCh37);
            CheckPaths(filePaths.GRCh38);

            return filePaths;
        }

        private static void CheckPath(string filePath, string description)
        {
            if (string.IsNullOrEmpty(filePath)) throw new InvalidDataException($"No value was found for the {description} key.");
            if (!File.Exists(filePath)) throw new FileNotFoundException($"Unable to find the following file: {filePath}");
        }

        private static void CheckPaths(FilePaths.AssemblySpecificPaths paths)
        {
            CheckPath(paths.EnsemblCachePath, "Ensembl intermediate cache");
            CheckPath(paths.RefSeqCachePath,  "RefSeq intermediate cache");
            CheckPath(paths.ReferencePath,    "reference");
        }

        public static ExitCodes Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "config|c=",
                    "config {path}",
                    v => _configPath = v
                },
                {
                    "out|o=",
                    "output {path}",
                    v => _outputPath = v
                }
            };

            var commandLineExample = $"{command} [options]";

            return new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .HasRequiredParameter(_outputPath, "output", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates the universal gene archive", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
