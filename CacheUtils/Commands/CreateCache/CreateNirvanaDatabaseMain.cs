using System.Collections.Generic;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using CacheUtils.Genes.Utilities;
using CacheUtils.Helpers;
using CacheUtils.IntermediateIO;
using CacheUtils.PredictionCache;
using CacheUtils.TranscriptCache;
using CacheUtils.Utilities;
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
        private static string _inputUgaPath;

        private static string _outputCacheFilePrefix;

        private static ExitCodes ProgramExecution()
        {
            var logger         = new ConsoleLogger();
            var transcriptPath = _inputPrefix + ".transcripts.gz";
            var siftPath       = _inputPrefix + ".sift.gz";
            var polyphenPath   = _inputPrefix + ".polyphen.gz";
            var regulatoryPath = _inputPrefix + ".regulatory.gz";

            var (refIndexToChromosome, refNameToChromosome, numRefSeqs) = SequenceHelper.GetDictionaries(_inputReferencePath);

            using (var transcriptReader = new MutableTranscriptReader(GZipUtilities.GetAppropriateReadStream(transcriptPath), refIndexToChromosome))
            using (var regulatoryReader = new RegulatoryRegionReader(GZipUtilities.GetAppropriateReadStream(regulatoryPath), refIndexToChromosome))
            using (var siftReader       = new PredictionReader(GZipUtilities.GetAppropriateReadStream(siftPath), refIndexToChromosome, IntermediateIoCommon.FileType.Sift))
            using (var polyphenReader   = new PredictionReader(GZipUtilities.GetAppropriateReadStream(polyphenPath), refIndexToChromosome, IntermediateIoCommon.FileType.Polyphen))
            using (var geneReader       = new UgaGeneReader(GZipUtilities.GetAppropriateReadStream(_inputUgaPath), refNameToChromosome))
            {
                var genomeAssembly  = transcriptReader.Header.GenomeAssembly;
                var source          = transcriptReader.Header.Source;
                var vepReleaseTicks = transcriptReader.Header.VepReleaseTicks;

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

                var transcriptBuilder = new TranscriptCacheBuilder(logger, genomeAssembly, source, vepReleaseTicks);
                var transcriptStaging = transcriptBuilder.CreateTranscriptCache(transcripts, regulatoryRegions, geneForest, numRefSeqs);

                logger.Write("- writing transcript cache... ");
                transcriptStaging.Write(FileUtilities.GetCreateStream(CacheConstants.TranscriptPath(_outputCacheFilePrefix)));
                logger.WriteLine("finished.");
            }

            return ExitCodes.Success;
        }

        private static IIntervalForest<UgaGene> CreateGeneForest(IEnumerable<UgaGene> genes, int numRefSeqs, GenomeAssembly genomeAssembly)
        {
            var useGrch37     = genomeAssembly == GenomeAssembly.GRCh37;
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
            var fileList = new List<RemoteFile>();
            var ccdsFile = new RemoteFile("CCDS file (2016-09-08)", "ftp://ftp.ncbi.nlm.nih.gov/pub/CCDS/current_human/CCDS2Sequence.20160908.txt", false);
            var lrgFile  = new RemoteFile("latest LRG file", "http://ftp.ebi.ac.uk/pub/databases/lrgex/list_LRGs_transcripts_xrefs.txt");

            fileList.Add(ccdsFile);
            fileList.Add(lrgFile);

            fileList.Execute(logger, "downloads", file => file.Download(logger));

            var ccdsIdToEnsemblId = CcdsReader.GetCcdsIdToEnsemblId(ccdsFile.FilePath);
            var lrgTranscriptIds  = LrgReader.GetTranscriptIds(lrgFile.FilePath, ccdsIdToEnsemblId);

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
                    "genes|g=",
                    "universal genes archive {filename}",
                    v => _inputUgaPath = v
                },
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

            var commandLineExample = $"{command} --in <prefix> --out <prefix> --genes <path> --ref <path> --lrg <path>";

            return new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .HasRequiredParameter(_inputPrefix, "intermediate cache", "--in")
                .CheckInputFilenameExists(_inputReferencePath, "compressed reference", "--ref")
                .CheckInputFilenameExists(_inputUgaPath, "UGA genes", "--genes")
                .HasRequiredParameter(_outputCacheFilePrefix, "Nirvana", "--out")
                .SkipBanner()
                .ShowHelpMenu("Converts *deserialized* VEP cache files to Nirvana cache format.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
