using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CacheUtils.Commands.Download;
using CacheUtils.Genes;
using CacheUtils.Genes.DataStores;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using Compression.FileHandling;
using ErrorHandling;
using IO;
using Microsoft.Extensions.Configuration;
using ReferenceSequence.Utilities;
using VariantAnnotation.Providers;

namespace CacheUtils.Commands.UniversalGeneArchive
{
    public static class UniversalGeneArchiveMain
    {
        private static string _referencesPath;
        private static string _intermediateCachePath;

        private static ExitCodes ProgramExecution()
        {
            if (UniversalGeneArchiveCurrent())
            {
                Logger.WriteLine("- universal gene archive is already up-to-date.");
                return ExitCodes.Success;
            }

            const string jsonPath = "CacheUtils.dll.gene.json";
            var filePaths = GetFilePaths(jsonPath);
            
            var ds = LoadDataStores(filePaths);

            var grch37GenesByRef = ds.Assembly37.UpdateHgncIds(ds.Hgnc).MergeByHgnc(true);            
            var grch38GenesByRef = ds.Assembly38.UpdateHgncIds(ds.Hgnc).MergeByHgnc(false);

            var universalGenes = CombineGenomeAssemblies(grch37GenesByRef, grch38GenesByRef).UpdateGeneSymbols(
                ds.Hgnc.HgncIdToSymbol, ds.GeneInfoData.EntrezGeneIdToSymbol,
                ds.Assembly38.EnsemblGtf.EnsemblIdToSymbol, ds.Assembly37.RefSeqGff.EntrezGeneIdToSymbol);

            WriteGenes(universalGenes);
            
            return ExitCodes.Success;
        }

        private static bool UniversalGeneArchiveCurrent()
        {
            var fileInfo = new FileInfo(ExternalFiles.UniversalGeneFilePath);
            return fileInfo.Exists && ExternalFiles.GetElapsedDays(fileInfo.CreationTime) < 1.0;
        }

        private static (GeneInfoData GeneInfoData, AssemblyDataStore Assembly37, AssemblyDataStore Assembly38, Hgnc Hgnc)
            LoadDataStores(FilePaths filePaths)
        {
            Logger.Write("- loading datastores... ");
            var loadBenchmark = new Benchmark();

            var (_, refNameToChromosome, _) = SequenceHelper.GetDictionaries(filePaths.GRCh38.ReferencePath);

            var geneInfoData = GeneInfoData.Create(ExternalFiles.GeneInfoFile.FilePath);
            var dataStore37  = AssemblyDataStore.Create("GRCh37", filePaths.GRCh37, refNameToChromosome, true);
            var dataStore38  = AssemblyDataStore.Create("GRCh38", filePaths.GRCh38, refNameToChromosome, false);
            var hgnc         = Hgnc.Create(ExternalFiles.HgncFile.FilePath, refNameToChromosome);

            Logger.WriteLine($"{Benchmark.ToHumanReadable(loadBenchmark.GetElapsedTime())}");

            return (geneInfoData, dataStore37, dataStore38, hgnc);
        }

        private static UgaGene[] CombineGenomeAssemblies(Dictionary<ushort, List<UgaGene>> genesByRef37, Dictionary<ushort, List<UgaGene>> genesByRef38)
        {
            Logger.WriteLine("\n*** Global ***");
            Logger.Write("- combining genes from GRCh37 and GRCh38... ");
            var combinedGenes = UgaAssemblyCombiner.Combine(genesByRef37, genesByRef38);
            Logger.WriteLine($"{combinedGenes.Length} genes.");

            return combinedGenes;
        }

        private static UgaGene[] UpdateGeneSymbols(this UgaGene[] genes, Dictionary<int, string> hgncIdToSymbol, Dictionary<string, string> entrezGeneIdToSymbol,
            Dictionary<string, string> ensemblIdToSymbol, Dictionary<string, string> refseqGeneIdToSymbol)
        {
            var updater = new GeneSymbolUpdater(hgncIdToSymbol, entrezGeneIdToSymbol, ensemblIdToSymbol, refseqGeneIdToSymbol);
            updater.Update(genes);
            return genes;
        }

        private static void WriteGenes(UgaGene[] genes)
        {
            Logger.Write($"- writing genes to {Path.GetFileName(ExternalFiles.UniversalGeneFilePath)}... ");

            using (var stream = new BlockGZipStream(FileUtilities.GetCreateStream(ExternalFiles.UniversalGeneFilePath), CompressionMode.Compress))
            using (var writer = new UgaGeneWriter(stream))
            {
                writer.Write(genes);
            }

            Logger.WriteLine("finished");
        }

        private static FilePaths GetFilePaths(string jsonPath)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile(jsonPath);

            var configuration = builder.Build();

            var filePaths = new FilePaths();
            configuration.Bind(filePaths);

            UpdatePaths(filePaths.GRCh37);
            UpdatePaths(filePaths.GRCh38);

            CheckPaths(filePaths.GRCh37);
            CheckPaths(filePaths.GRCh38);

            return filePaths;
        }

        private static void UpdatePaths(FilePaths.AssemblySpecificPaths paths)
        {
            paths.EnsemblCachePath = Path.Combine(_intermediateCachePath, paths.EnsemblCachePath);
            paths.RefSeqCachePath  = Path.Combine(_intermediateCachePath, paths.RefSeqCachePath);
            paths.ReferencePath    = Path.Combine(_referencesPath, paths.ReferencePath);
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
                    "icache|i=",
                    "intermediate cache {dir}",
                    v => _intermediateCachePath = v
                },
                {
                    "ref|r=",
                    "reference {dir}",
                    v => _referencesPath = v
                }
            };

            string commandLineExample = $"{command} -i <intermediate cache dir> -r <reference dir>";

            return new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .CheckDirectoryExists(_intermediateCachePath, "intermediate cache", "--icache")
                .CheckDirectoryExists(_referencesPath, "reference", "--ref")
                .SkipBanner()
                .ShowHelpMenu("Creates the universal gene archive", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
