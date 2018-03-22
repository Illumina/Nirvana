using System.Collections.Generic;
using System.Linq;
using CacheUtils.Commands.Download;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using CacheUtils.Genes.Utilities;
using CacheUtils.Helpers;
using CacheUtils.IntermediateIO;
using CacheUtils.PredictionCache;
using CacheUtils.TranscriptCache;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Logger;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;

namespace CacheUtils.Commands.CreateCache
{
    public static class CreateNirvanaDatabaseMain
    {
        private static string _inputPrefix;
        private static string _inputReferencePath;

        private static string _outputCacheFilePrefix;

        private static ExitCodes ProgramExecution()
        {
            var logger            = new ConsoleLogger();
            string transcriptPath = _inputPrefix + ".transcripts.gz";
            string siftPath       = _inputPrefix + ".sift.gz";
            string polyphenPath   = _inputPrefix + ".polyphen.gz";
            string regulatoryPath = _inputPrefix + ".regulatory.gz";

            var (refIndexToChromosome, refNameToChromosome, numRefSeqs) = SequenceHelper.GetDictionaries(_inputReferencePath);

            using (var transcriptReader = new MutableTranscriptReader(GZipUtilities.GetAppropriateReadStream(transcriptPath), refIndexToChromosome))
            using (var regulatoryReader = new RegulatoryRegionReader(GZipUtilities.GetAppropriateReadStream(regulatoryPath), refIndexToChromosome))
            using (var siftReader       = new PredictionReader(GZipUtilities.GetAppropriateReadStream(siftPath), refIndexToChromosome, IntermediateIoCommon.FileType.Sift))
            using (var polyphenReader   = new PredictionReader(GZipUtilities.GetAppropriateReadStream(polyphenPath), refIndexToChromosome, IntermediateIoCommon.FileType.Polyphen))
            using (var geneReader       = new UgaGeneReader(GZipUtilities.GetAppropriateReadStream(ExternalFiles.UniversalGeneFilePath), refNameToChromosome))
            {
                var genomeAssembly   = transcriptReader.Header.GenomeAssembly;
                var source           = transcriptReader.Header.Source;
                long vepReleaseTicks = transcriptReader.Header.VepReleaseTicks;
                ushort vepVersion    = transcriptReader.Header.VepVersion;

                logger.Write("- loading universal gene archive file... ");
                var genes      = geneReader.GetGenes();
                var geneForest = CreateGeneForest(genes, numRefSeqs, genomeAssembly);
                logger.WriteLine($"{genes.Length:N0} loaded.");

                logger.Write("- loading regulatory region file... ");
                var regulatoryRegions = regulatoryReader.GetRegulatoryRegions();
                logger.WriteLine($"{regulatoryRegions.Length:N0} loaded.");

                logger.Write("- loading transcript file... ");
                var transcripts = transcriptReader.GetTranscripts();
                var transcriptsByRefIndex = transcripts.GetMultiValueDict(x => x.Chromosome.Index);
                logger.WriteLine($"{transcripts.Length:N0} loaded.");

                MarkCanonicalTranscripts(logger, transcripts);

                var predictionBuilder = new PredictionCacheBuilder(logger, genomeAssembly);
                var predictionCaches  = predictionBuilder.CreatePredictionCaches(transcriptsByRefIndex, siftReader, polyphenReader, numRefSeqs);

                logger.Write("- writing SIFT prediction cache... ");
                predictionCaches.Sift.Write(FileUtilities.GetCreateStream(CacheConstants.SiftPath(_outputCacheFilePrefix)));
                logger.WriteLine("finished.");

                logger.Write("- writing PolyPhen prediction cache... ");
                predictionCaches.PolyPhen.Write(FileUtilities.GetCreateStream(CacheConstants.PolyPhenPath(_outputCacheFilePrefix)));
                logger.WriteLine("finished.");

                var transcriptBuilder = new TranscriptCacheBuilder(logger, genomeAssembly, source, vepReleaseTicks, vepVersion);
                var transcriptStaging = transcriptBuilder.CreateTranscriptCache(transcripts, regulatoryRegions, geneForest, numRefSeqs);

                logger.Write("- writing transcript cache... ");
                transcriptStaging.Write(FileUtilities.GetCreateStream(CacheConstants.TranscriptPath(_outputCacheFilePrefix)));
                logger.WriteLine("finished.");
            }

            return ExitCodes.Success;
        }

        private static IIntervalForest<UgaGene> CreateGeneForest(IEnumerable<UgaGene> genes, int numRefSeqs, GenomeAssembly genomeAssembly)
        {
            bool useGrch37    = genomeAssembly == GenomeAssembly.GRCh37;
            var intervalLists = new List<Interval<UgaGene>>[numRefSeqs];

            for (var i = 0; i < numRefSeqs; i++) intervalLists[i] = new List<Interval<UgaGene>>();

            foreach (var gene in genes)
            {
                var coords = useGrch37 ? gene.GRCh37 : gene.GRCh38;
                if (coords.Start == -1 && coords.End == -1) continue;
                intervalLists[gene.Chromosome.Index].Add(new Interval<UgaGene>(coords.Start, coords.End, gene));
            }

            var refIntervalArrays = new IntervalArray<UgaGene>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++)
            {
                refIntervalArrays[i] = new IntervalArray<UgaGene>(intervalLists[i].OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray());
            }

            return new IntervalForest<UgaGene>(refIntervalArrays);
        }

        private static void MarkCanonicalTranscripts(ILogger logger, MutableTranscript[] transcripts)
        {
            var ccdsIdToEnsemblId = CcdsReader.GetCcdsIdToEnsemblId(ExternalFiles.CcdsFile.FilePath);
            var lrgTranscriptIds  = LrgReader.GetTranscriptIds(ExternalFiles.LrgFile.FilePath, ccdsIdToEnsemblId);

            logger.Write("- marking canonical transcripts... ");
            var canonical = new CanonicalTranscriptMarker(lrgTranscriptIds);
            int numCanonicalTranscripts = canonical.MarkTranscripts(transcripts);
            logger.WriteLine($"{numCanonicalTranscripts:N0} marked.");
        }

        public static ExitCodes Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input filename {prefix}",
                    v => _inputPrefix = v
                },
                {
                    "out|o=",
                    "output cache file {prefix}",
                    v => _outputCacheFilePrefix = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => _inputReferencePath = v
                }
            };

            string commandLineExample = $"{command} --in <prefix> --out <prefix> --ref <path>";

            return new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .HasRequiredParameter(_inputPrefix, "intermediate cache", "--in")
                .CheckInputFilenameExists(_inputReferencePath, "compressed reference", "--ref")
                .HasRequiredParameter(_outputCacheFilePrefix, "Nirvana", "--out")
                .SkipBanner()
                .ShowHelpMenu("Converts *deserialized* VEP cache files to Nirvana cache format.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
