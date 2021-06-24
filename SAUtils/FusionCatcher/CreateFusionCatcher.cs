using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using Genome;
using IO;
using VariantAnnotation.Caches;
using VariantAnnotation.GeneFusions.IO;
using VariantAnnotation.GeneFusions.SA;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.FusionCatcher
{
    public static class CreateFusionCatcher
    {
        private static string _transcriptCache37Path;
        private static string _transcriptCache38Path;
        private static string _dataDirectory;
        private static string _reference38Path;
        private static string _outputDirectory;

        private static ExitCodes ProgramExecution()
        {
            var geneKeyToFusion = new Dictionary<ulong, GeneFusionSourceBuilder>();
            var knownGenes      = new HashSet<string>();
            var oncoGenes       = new HashSet<uint>();

            IDictionary<ushort, IChromosome> refIndexToChromosome = GetReferences(_reference38Path);

            AddGenes(_transcriptCache37Path, refIndexToChromosome, knownGenes, "GRCh37");
            AddGenes(_transcriptCache38Path, refIndexToChromosome, knownGenes, "GRCh38");

            DataSourceVersion version = CreateDataSourceVersion(Path.Combine(_dataDirectory, "version.txt"));

            // relationships
            FusionCatcherDataSource.Parse(GetStream("pairs_pseudogenes.txt"), GeneFusionSource.Pseudogene, CollectionType.Relationships,
                geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("paralogs.txt"), GeneFusionSource.Paralog, CollectionType.Relationships, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("readthroughs.txt"), GeneFusionSource.Readthrough, CollectionType.Relationships, geneKeyToFusion,
                knownGenes);

            // oncogenes
            FusionCatcherOncogenes.Parse(GetStream("cancer_genes.txt"), "Bushman", oncoGenes, knownGenes);
            FusionCatcherOncogenes.Parse(GetStream("oncogenes_more.txt"),    "ONGENE",  oncoGenes, knownGenes);
            FusionCatcherOncogenes.Parse(GetStream("tumor_genes.txt"),  "UniProt", oncoGenes, knownGenes);
            Console.WriteLine($"- found a total of {oncoGenes.Count:N0} oncogenes.");

            // germline fusions
            FusionCatcherDataSource.Parse(GetStream("1000genomes.txt"), GeneFusionSource.OneK_Genomes_Project, CollectionType.Germline,
                geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("banned.txt"), GeneFusionSource.Healthy_strong_support, CollectionType.Germline, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("bodymap2.txt"), GeneFusionSource.Illumina_BodyMap2, CollectionType.Germline, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("cacg.txt"),     GeneFusionSource.CACG,     CollectionType.Germline, geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("conjoing.txt"), GeneFusionSource.ConjoinG, CollectionType.Germline, geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("cortex.txt"), GeneFusionSource.Healthy_prefrontal_cortex, CollectionType.Germline,
                geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("dgd.txt"), GeneFusionSource.Duplicated_Genes_Database, CollectionType.Germline, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("gtex.txt"), GeneFusionSource.GTEx_healthy_tissues, CollectionType.Germline, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("healthy.txt"), GeneFusionSource.Healthy, CollectionType.Germline, geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("hpa.txt"), GeneFusionSource.Human_Protein_Atlas, CollectionType.Germline, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("non-cancer_tissues.txt"), GeneFusionSource.Babiceanu_NonCancerTissues, CollectionType.Germline,
                geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("non-tumor_cells.txt"), GeneFusionSource.NonTumorCellLines, CollectionType.Germline,
                geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("tcga-normal.txt"), GeneFusionSource.TumorFusions_normal, CollectionType.Germline, geneKeyToFusion,
                knownGenes);

            // somatic fusions
            FusionCatcherDataSource.Parse(GetStream("18cancers.txt"), GeneFusionSource.Alaei_Mahabadi_18_Cancers, CollectionType.Somatic,
                geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("ccle.txt"),  GeneFusionSource.CCLE,       CollectionType.Somatic, geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("ccle2.txt"), GeneFusionSource.CCLE_Klign, CollectionType.Somatic, geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("ccle3.txt"), GeneFusionSource.CCLE_Vellichirammal, CollectionType.Somatic, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("cgp.txt"), GeneFusionSource.Cancer_Genome_Project, CollectionType.Somatic, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("chimerdb4kb.txt"), GeneFusionSource.ChimerKB_4, CollectionType.Somatic, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("chimerdb4pub.txt"), GeneFusionSource.ChimerPub_4, CollectionType.Somatic, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("chimerdb4seq.txt"), GeneFusionSource.ChimerSeq_4, CollectionType.Somatic, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("cosmic.txt"), GeneFusionSource.COSMIC, CollectionType.Somatic, geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("gliomas.txt"), GeneFusionSource.Bao_gliomas, CollectionType.Somatic, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("known.txt"), GeneFusionSource.Known, CollectionType.Somatic, geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("mitelman.txt"), GeneFusionSource.Mitelman_DB, CollectionType.Somatic, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("oesophagus.txt"), GeneFusionSource.TCGA_oesophageal_carcinomas, CollectionType.Somatic,
                geneKeyToFusion, knownGenes);
            // FusionCatcherDataSource.Parse(GetStream("oncokb.txt"), GeneFusionSource.OncoKB, CollectionType.Somatic, geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("pancreases.txt"), GeneFusionSource.Bailey_pancreatic_cancers, CollectionType.Somatic,
                geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("pcawg.txt"), GeneFusionSource.PCAWG, CollectionType.Somatic, geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("prostate_cancer.txt"), GeneFusionSource.Robinson_prostate_cancers, CollectionType.Somatic,
                geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("tcga.txt"), GeneFusionSource.TCGA, CollectionType.Somatic, geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("tcga-cancer.txt"), GeneFusionSource.TumorFusions_tumor, CollectionType.Somatic, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("tcga2.txt"), GeneFusionSource.TCGA_Gao, CollectionType.Somatic, geneKeyToFusion, knownGenes);
            FusionCatcherDataSource.Parse(GetStream("tcga3.txt"), GeneFusionSource.TCGA_Vellichirammal, CollectionType.Somatic, geneKeyToFusion,
                knownGenes);
            FusionCatcherDataSource.Parse(GetStream("ticdb.txt"), GeneFusionSource.TICdb, CollectionType.Somatic, geneKeyToFusion, knownGenes);

            (GeneFusionSourceCollection[] index, GeneFusionIndexEntry[] indexEntries) = IndexBuilder.Convert(geneKeyToFusion);
            Console.WriteLine($"- created {index.Length:N0} index entries.");

            uint[] oncogeneKeys = oncoGenes.OrderBy(x => x).ToArray();
            
            WriteGeneFusions(_outputDirectory, oncogeneKeys, index, indexEntries, version);

            Console.WriteLine();
            Console.WriteLine($"Total: {geneKeyToFusion.Count:N0} gene pairs in database.");

            return ExitCodes.Success;
        }
        
        private static IDictionary<ushort, IChromosome> GetReferences(string referencePath)
        {
            Console.Write("- loading reference sequence... ");
            var sequenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(referencePath));
            Console.WriteLine("finished.");

            return sequenceProvider.RefIndexToChromosome;
        }

        private static void AddGenes(string cachePath, IDictionary<ushort, IChromosome> refIndexToChromosome, ISet<string> knownGenes,
            string description)
        {
            Console.Write($"- loading known genes ({description})... ");
            int startCount = knownGenes.Count;

            using (var reader = new TranscriptCacheReader(FileUtilities.GetReadStream(cachePath)))
            {
                TranscriptCacheData cacheData = reader.Read(refIndexToChromosome);

                foreach (IGene gene in cacheData.Genes)
                {
                    string ensemblId = gene.EnsemblId.WithoutVersion;
                    if (string.IsNullOrEmpty(ensemblId)) continue;
                    knownGenes.Add(ensemblId);
                }
            }

            int numAdded = knownGenes.Count - startCount;
            Console.WriteLine($"added {numAdded:N0} Ensembl gene IDs.");
        }

        private static void WriteGeneFusions(string outputDirectory, uint[] oncogeneKeys, GeneFusionSourceCollection[] index,
            // ReSharper disable once SuggestBaseTypeForParameter
            GeneFusionIndexEntry[] indexEntries, DataSourceVersion version)
        {
            Console.Write("- writing gene fusions SA file... ");
            string    outputPath = Path.Combine(outputDirectory, $"FusionCatcher_{version.Version}{SaCommon.GeneFusionSourceSuffix}");
            using var writer     = new GeneFusionSourceWriter(FileUtilities.GetCreateStream(outputPath), "fusionCatcher", version);
            writer.Write(oncogeneKeys, index, indexEntries);
            Console.WriteLine("finished.");
        }

        private static DataSourceVersion CreateDataSourceVersion(string filePath)
        {
            var fi = new FileInfo(filePath);
            long releaseDateTicks = fi.CreationTime.Ticks;
            
            // const string description =
            using var reader = new StreamReader(FileUtilities.GetReadStream(filePath));
            string    line   = reader.ReadLine();
            if (line == null) throw new InvalidDataException("Could not extract the first line from version.txt");

            int    spacePos = line.LastIndexOf(' ');
            string version  = line.Substring(spacePos + 1);
            
            return new DataSourceVersion("FusionCatcher", version, releaseDateTicks, "known germline and somatic gene fusions");
        }

        private static Stream GetStream(string filename) => GZipUtilities.GetAppropriateReadStream(Path.Combine(_dataDirectory, filename));

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "cache37=",
                    "transcript cache {path} for GRCh37",
                    v => _transcriptCache37Path = v
                },
                {
                    "cache38=",
                    "transcript cache {path} for GRCh38",
                    v => _transcriptCache38Path = v
                },
                {
                    "in|i=",
                    "FusionCatcher data {directory}",
                    v => _dataDirectory = v
                },
                {
                    "out|o=",
                    "output {directory}",
                    v => _outputDirectory = v
                },
                {
                    "ref|r=",
                    "input reference sequence {path} for GRCh38",
                    v => _reference38Path = v
                }
            };

            var commandLineExample = $"{command} [options]";

            ExitCodes exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_reference38Path,       "reference sequence (GRCh38)", "--ref")
                .CheckInputFilenameExists(_transcriptCache37Path, "transcript cache (GRCh37)",   "--cache37")
                .CheckInputFilenameExists(_transcriptCache38Path, "transcript cache (GRCh38)",   "--cache38")
                .CheckDirectoryExists(_dataDirectory,   "FusionCatcher data directory", "--in")
                .CheckDirectoryExists(_outputDirectory, "output directory",             "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database with FusionCatcher annotations", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }
    }
}