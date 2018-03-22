using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.MiniCache;
using CacheUtils.PredictionCache;
using CacheUtils.Sequence;
using CacheUtils.TranscriptCache;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using CommonUtilities;
using ErrorHandling;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Logger;
using VariantAnnotation.Providers;
using VariantAnnotation.Sequence;
using VariantAnnotation.Utilities;

namespace CacheUtils.Commands.ExtractTranscripts
{
    public static class ExtractTranscriptsMain
    {
        private static string _inputPrefix;
        private static string _inputReferencePath;
        private static string _outputDirectory;

        private static string _referenceName;

        private static int _referencePosition    = -1;
        private static int _referenceEndPosition = -1;

        private static ExitCodes ProgramExecution()
        {
            var logger     = new ConsoleLogger();
            var bundle     = DataBundle.GetDataBundle(_inputReferencePath, _inputPrefix);
            int numRefSeqs = bundle.SequenceReader.NumRefSeqs;
            var chromosome = ReferenceNameUtilities.GetChromosome(bundle.SequenceReader.RefNameToChromosome, _referenceName);
            bundle.Load(chromosome);

            string outputStub  = GetOutputStub(chromosome, bundle.Source);
            var interval    = new ChromosomeInterval(chromosome, _referencePosition, _referenceEndPosition);
            var transcripts = GetTranscripts(logger, bundle, interval);

            var sift              = GetPredictionStaging(logger, "SIFT", transcripts, chromosome, bundle.SiftPredictions, bundle.SiftReader, x => x.SiftIndex, numRefSeqs);
            var polyphen          = GetPredictionStaging(logger, "PolyPhen", transcripts, chromosome, bundle.PolyPhenPredictions, bundle.PolyPhenReader, x => x.PolyPhenIndex, numRefSeqs);
            string referenceBases = GetReferenceBases(logger, bundle.SequenceReader, interval);

            var regulatoryRegionIntervalArrays = GetRegulatoryRegionIntervalArrays(logger, bundle.TranscriptCache, interval, numRefSeqs);
            var transcriptIntervalArrays = PredictionUtilities.UpdateTranscripts(transcripts, bundle.SiftPredictions,
                sift.Predictions, bundle.PolyPhenPredictions, polyphen.Predictions, numRefSeqs);

            var transcriptStaging = GetTranscriptStaging(bundle.TranscriptCacheData.Header, transcriptIntervalArrays, regulatoryRegionIntervalArrays);

            WriteCache(logger, FileUtilities.GetCreateStream(CacheConstants.TranscriptPath(outputStub)), transcriptStaging, "transcript");
            WriteCache(logger, FileUtilities.GetCreateStream(CacheConstants.SiftPath(outputStub)), sift.Staging, "SIFT");
            WriteCache(logger, FileUtilities.GetCreateStream(CacheConstants.PolyPhenPath(outputStub)), polyphen.Staging, "PolyPhen");
            WriteReference(logger, CacheConstants.BasesPath(outputStub), bundle.SequenceReader, chromosome,
                referenceBases, interval.Start);

            return ExitCodes.Success;
        }

        private static TranscriptCacheStaging GetTranscriptStaging(CacheHeader header,
            IntervalArray<ITranscript>[] transcriptIntervalArrays,
            IntervalArray<IRegulatoryRegion>[] regulatoryRegionIntervalArrays) =>
            TranscriptCacheStaging.GetStaging(header, transcriptIntervalArrays, regulatoryRegionIntervalArrays);

        private static void WriteReference(ILogger logger, string outputPath, CompressedSequenceReader reader,
            IChromosome chromosome, string referenceBases, int offset)
        {
            logger.Write("- writing reference bases... ");
            var cytogeneticBands = new CytogeneticBands(reader.CytogeneticBands);

            using (var writer = new CompressedSequenceWriter(FileUtilities.GetCreateStream(outputPath),
                reader.ReferenceMetadataList, cytogeneticBands, reader.Assembly))
            {
                writer.Write(chromosome.EnsemblName, referenceBases, offset);
            }
            logger.WriteLine("finished.");
        }

        private static void WriteCache(ILogger logger, Stream stream, IStaging staging, string description)
        {
            logger.Write($"- writing {description} cache... ");
            staging.Write(stream);
            logger.WriteLine("finished.");
        }

        private static string GetOutputStub(IChromosome chromosome, Source source) => Path.Combine(_outputDirectory,
            $"{chromosome.UcscName}_{_referencePosition}_{_referenceEndPosition}_{GetSource(source)}");

        private static string GetSource(Source source) =>
            source != Source.BothRefSeqAndEnsembl ? source.ToString() : "Both";

        private static string GetReferenceBases(ILogger logger, CompressedSequenceReader reader, IChromosomeInterval interval)
        {
            logger.Write("- retrieving reference bases... ");
            reader.GetCompressedSequence(interval.Chromosome);
            string referenceBases = reader.Sequence.Substring(interval.Start, interval.End - interval.Start + 1);
            logger.WriteLine($"{referenceBases.Length} bases extracted.");

            return referenceBases;
        }

        private static (PredictionCacheStaging Staging, Prediction[] Predictions) GetPredictionStaging(ILogger logger,
            string description, IEnumerable<ITranscript> transcripts, IChromosome chromosome, IReadOnlyList<Prediction> oldPredictions,
            PredictionCacheReader reader, Func<ITranscript, int> indexFunc, int numRefSeqs)
        {
            logger.Write($"- retrieving {description} predictions... ");

            var indexSet          = GetUniqueIndices(transcripts, indexFunc);
            var lut               = reader.Header.Lut;
            var predictionsPerRef = GetPredictions(indexSet, chromosome, numRefSeqs, oldPredictions);
            var staging           = new PredictionCacheStaging(reader.Header.Header, lut, predictionsPerRef);

            logger.WriteLine($"found {indexSet.Count} predictions.");
            return (staging, predictionsPerRef[chromosome.Index]);
        }

        private static Prediction[][] GetPredictions(ICollection<int> indexSet, IChromosome chromosome, int numRefSeqs,
            IReadOnlyList<Prediction> oldPredictions)
        {
            var refPredictions = new Prediction[indexSet.Count];

            var predIdx = 0;
            foreach (int index in indexSet) refPredictions[predIdx++] = oldPredictions[index];

            var predictions = new Prediction[numRefSeqs][];
            predictions[chromosome.Index] = refPredictions;
            return predictions;
        }

        private static HashSet<int> GetUniqueIndices(IEnumerable<ITranscript> transcripts, Func<ITranscript, int> indexFunc)
        {
            var indexSet = new HashSet<int>();
            foreach (var transcript in transcripts)
            {
                int index = indexFunc(transcript);
                if (index == -1) continue;
                indexSet.Add(index);
            }
            return indexSet;
        }

        private static IntervalArray<IRegulatoryRegion>[] GetRegulatoryRegionIntervalArrays(ILogger logger,
            ITranscriptCache cache, IChromosomeInterval interval, int numRefSeqs)
        {
            logger.Write("- retrieving regulatory regions... ");
            var regulatoryRegions = cache.GetOverlappingRegulatoryRegions(interval);
            logger.WriteLine($"found {regulatoryRegions.Length} regulatory regions.");
            return regulatoryRegions.ToIntervalArrays(numRefSeqs);
        }

        private static List<ITranscript> GetTranscripts(ILogger logger, DataBundle bundle, IChromosomeInterval interval)
        {
            logger.Write("- retrieving transcripts... ");
            var transcripts = TranscriptCacheUtilities.GetTranscripts(bundle, interval);
            logger.WriteLine($"found {transcripts.Count} transcripts.");

            if (transcripts.Count == 0) throw new InvalidDataException("Expected at least one transcript, but found none.");
            return transcripts;
        }

        public static ExitCodes Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input cache {prefix}",
                    v => _inputPrefix = v
                },
                {
                    "name|n=",
                    "reference {name}",
                    v => _referenceName = v
                },
                {
                    "out|o=",
                    "output {directory}",
                    v => _outputDirectory = v
                },
                {
                    "pos|p=",
                    "reference {position}",
                    (int v) => _referencePosition = v
                },
                {
                    "endpos=",
                    "reference end {position}",
                    (int v) => _referenceEndPosition = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => _inputReferencePath = v
                }
            };

            string commandLineExample = $"{command} --in <prefix> --out <dir> -r <ref path> --chr <name> -p <pos> --endpos <pos>\n";

            return new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .HasRequiredParameter(_inputPrefix, "Nirvana cache", "--in")
                .CheckInputFilenameExists(_inputReferencePath, "compressed reference sequence", "--ref")
                .CheckDirectoryExists(_outputDirectory, "output cache", "--out")
                .SkipBanner()
                .ShowHelpMenu("Extracts transcripts from Nirvana cache files.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
