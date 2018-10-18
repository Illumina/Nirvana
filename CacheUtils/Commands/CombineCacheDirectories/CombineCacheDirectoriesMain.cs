using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CacheUtils.PredictionCache;
using CacheUtils.TranscriptCache;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Algorithms;
using Compression.FileHandling;
using ErrorHandling;
using Genome;
using Intervals;
using IO;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Logger;
using VariantAnnotation.Providers;
using VariantAnnotation.Sequence;

namespace CacheUtils.Commands.CombineCacheDirectories
{
    public static class CombineCacheDirectoriesMain
    {
        private static string _inputPrefix;
        private static string _inputPrefix2;
        private static string _outputPrefix;
        private static string _refSequencePath;

        private static ExitCodes ProgramExecution()
        {
            var sequenceData = SequenceHelper.GetDictionaries(_refSequencePath);
            var logger       = new ConsoleLogger();

            var caches = LoadTranscriptCaches(logger, CacheConstants.TranscriptPath(_inputPrefix),
                CacheConstants.TranscriptPath(_inputPrefix2), sequenceData.refIndexToChromosome);

            if (caches.Cache.TranscriptIntervalArrays.Length != caches.Cache2.TranscriptIntervalArrays.Length)
                throw new InvalidDataException($"Expected the number of reference sequences in cache 1 ({caches.Cache.TranscriptIntervalArrays.Length}) and cache 2 ({caches.Cache2.TranscriptIntervalArrays.Length}) to be the same.");

            int numRefSeqs                = caches.Cache.TranscriptIntervalArrays.Length;
            var combinedIntervalArrays    = new IntervalArray<ITranscript>[numRefSeqs];
            var siftPredictionsPerRef     = new Prediction[numRefSeqs][];
            var polyphenPredictionsPerRef = new Prediction[numRefSeqs][];

            PredictionHeader siftHeader;
            PredictionHeader polyphenHeader;

            using (var siftReader       = new PredictionCacheReader(FileUtilities.GetReadStream(CacheConstants.SiftPath(_inputPrefix)), PredictionCacheReader.SiftDescriptions))
            using (var siftReader2      = new PredictionCacheReader(FileUtilities.GetReadStream(CacheConstants.SiftPath(_inputPrefix2)), PredictionCacheReader.SiftDescriptions))
            using (var polyphenReader   = new PredictionCacheReader(FileUtilities.GetReadStream(CacheConstants.PolyPhenPath(_inputPrefix)), PredictionCacheReader.PolyphenDescriptions))
            using (var polyphenReader2  = new PredictionCacheReader(FileUtilities.GetReadStream(CacheConstants.PolyPhenPath(_inputPrefix2)), PredictionCacheReader.PolyphenDescriptions))
            {
                siftHeader     = siftReader.Header;
                polyphenHeader = polyphenReader.Header;

                for (ushort refIndex = 0; refIndex < numRefSeqs; refIndex++)
                {
                    var chromosome = sequenceData.refIndexToChromosome[refIndex];

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    logger.WriteLine($"\n{chromosome.UcscName}:");
                    Console.ResetColor();

                    var sift = CombinePredictions(logger, chromosome, "SIFT", siftReader, siftReader2);
                    siftPredictionsPerRef[refIndex] = sift.Predictions;

                    var polyphen = CombinePredictions(logger, chromosome, "PolyPhen", polyphenReader, polyphenReader2);
                    polyphenPredictionsPerRef[refIndex] = polyphen.Predictions;

                    var transcriptIntervalArray  = caches.Cache.TranscriptIntervalArrays[refIndex];
                    var transcriptIntervalArray2 = caches.Cache2.TranscriptIntervalArrays[refIndex];

                    combinedIntervalArrays[refIndex] = CombineTranscripts(logger, transcriptIntervalArray,
                        transcriptIntervalArray2, sift.Offset, polyphen.Offset);
                }
            }

            logger.WriteLine();
            WritePredictions(logger, "SIFT", CacheConstants.SiftPath(_outputPrefix), siftHeader, siftPredictionsPerRef);
            WritePredictions(logger, "PolyPhen", CacheConstants.PolyPhenPath(_outputPrefix), polyphenHeader, polyphenPredictionsPerRef);
            WriteTranscripts(logger, CloneHeader(caches.Cache.Header), combinedIntervalArrays,
                caches.Cache.RegulatoryRegionIntervalArrays);

            return ExitCodes.Success;
        }

        private static void WriteTranscripts(ILogger logger, CacheHeader header,
            IntervalArray<ITranscript>[] transcriptIntervalArrays,
            IntervalArray<IRegulatoryRegion>[] regulatoryRegionIntervalArrays)
        {
            var staging = TranscriptCacheStaging.GetStaging(header, transcriptIntervalArrays, regulatoryRegionIntervalArrays);

            logger.Write("- writing transcripts... ");
            staging.Write(FileUtilities.GetCreateStream(CacheConstants.TranscriptPath(_outputPrefix)));
            logger.WriteLine("finished.");
        }

        private static void WritePredictions(ILogger logger, string description, string filePath,
            PredictionHeader header, Prediction[][] predictionsPerRef)
        {
            logger.Write($"- writing {description} predictions... ");

            using (var stream = new BlockStream(new Zstandard(), FileUtilities.GetCreateStream(filePath), CompressionMode.Compress))
            using (var writer = new PredictionCacheWriter(stream, CloneHeader(header)))
            {
                writer.Write(header.LookupTable, predictionsPerRef);
            }

            logger.WriteLine("finished.");
        }

        private static IntervalArray<ITranscript> CombineTranscripts(ILogger logger,
            IntervalArray<ITranscript> intervalArray, IntervalArray<ITranscript> intervalArray2,
            int siftOffset, int polyphenOffset)
        {
            logger.Write("- combine transcripts... ");

            int numCombinedTranscripts = GetNumCombinedTranscripts(intervalArray, intervalArray2);
            var combinedIntervals      = new Interval<ITranscript>[numCombinedTranscripts];

            var combinedIndex = 0;
            CopyItems(intervalArray?.Array,  combinedIntervals, ref combinedIndex, interval => interval);
            CopyItems(intervalArray2?.Array, combinedIntervals, ref combinedIndex, interval => GetUpdatedTranscript(interval, siftOffset, polyphenOffset));

            logger.WriteLine("finished.");

            return new IntervalArray<ITranscript>(combinedIntervals.OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray());
        }

        private static int GetNumCombinedTranscripts<T>(IntervalArray<T> intervalArray,
            IntervalArray<T> intervalArray2)
        {
            int numIntervals  = intervalArray?.Array.Length ?? 0;
            int numIntervals2 = intervalArray2?.Array.Length ?? 0;
            return numIntervals + numIntervals2;
        }

        // ReSharper disable SuggestBaseTypeForParameter
        private static void CopyItems<T>(T[] src, T[] dest, ref int destIndex, Func<T, T> updateFunc)
        // ReSharper restore SuggestBaseTypeForParameter
        {
            if (src == null) return;
            foreach (var item in src) dest[destIndex++] = updateFunc(item);
        }

        private static Interval<ITranscript> GetUpdatedTranscript(Interval<ITranscript> interval, int siftOffset,
            int polyphenOffset)
        {
            var transcript = interval.Value;
            if (transcript.SiftIndex == -1 && transcript.PolyPhenIndex == -1) return interval;

            int newSiftIndex     = transcript.SiftIndex     == -1 ? -1 : transcript.SiftIndex + siftOffset;
            int newPolyphenIndex = transcript.PolyPhenIndex == -1 ? -1 : transcript.PolyPhenIndex + polyphenOffset;

            var updatedTranscript = transcript.UpdatePredictions(newSiftIndex, newPolyphenIndex);
            return new Interval<ITranscript>(transcript.Start, transcript.End, updatedTranscript);
        }

        private static VariantAnnotation.IO.Caches.Header CloneBaseHeader(VariantAnnotation.IO.Caches.Header header) =>
            new VariantAnnotation.IO.Caches.Header(CacheConstants.Identifier, header.SchemaVersion, header.DataVersion,
                Source.BothRefSeqAndEnsembl, DateTime.Now.Ticks, header.Assembly);

        private static PredictionHeader CloneHeader(PredictionHeader header) =>
            new PredictionHeader(CloneBaseHeader(header), header.Custom, header.LookupTable);

        private static CacheHeader CloneHeader(CacheHeader header) =>
            new CacheHeader(CloneBaseHeader(header), header.Custom);

        private static (Prediction[] Predictions, int Offset) CombinePredictions(ILogger logger, IChromosome chromosome,
            string description, PredictionCacheReader reader, PredictionCacheReader reader2)
        {
            logger.Write($"- load {description} predictions... ");
            var predictions  = reader.GetPredictions(chromosome.Index);
            var predictions2 = reader2.GetPredictions(chromosome.Index);
            logger.WriteLine("finished.");

            var combinedPredictions = CombinePredictions(logger, description, predictions, predictions2);
            return (combinedPredictions, predictions.Length);
        }

        private static Prediction[] CombinePredictions(ILogger logger, string description, Prediction[] predictions,
            Prediction[] predictions2)
        {
            logger.Write($"- combine {description} predictions... ");

            int numCombinedPredictions = predictions.Length + predictions2.Length;
            var combinedPredictions    = new Prediction[numCombinedPredictions];

            var combinedIndex = 0;
            CopyItems(predictions, combinedPredictions, ref combinedIndex, x => x);
            CopyItems(predictions2, combinedPredictions, ref combinedIndex, x => x);

            logger.WriteLine("finished.");

            return combinedPredictions;
        }

        private static (TranscriptCacheData Cache, TranscriptCacheData Cache2) LoadTranscriptCaches(ILogger logger,
            string transcriptPath, string transcriptPath2, IDictionary<ushort, IChromosome> refIndexToChromosome)
        {
            TranscriptCacheData cache;
            TranscriptCacheData cache2;

            logger.Write("- loading transcript caches... ");

            using (var transcriptReader  = new TranscriptCacheReader(FileUtilities.GetReadStream(transcriptPath)))
            using (var transcriptReader2 = new TranscriptCacheReader(FileUtilities.GetReadStream(transcriptPath2)))
            {
                cache  = transcriptReader.Read(refIndexToChromosome);
                cache2 = transcriptReader2.Read(refIndexToChromosome);
            }

            logger.WriteLine("finished.");
            return (cache, cache2);
        }

        public static ExitCodes Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|1=",
                    "input cache {prefix}",
                    v => _inputPrefix = v
                },
                {
                    "in2|2=",
                    "input cache 2 {prefix}",
                    v => _inputPrefix2 = v
                },
                {
                    "out|o=",
                    "output cache {prefix}",
                    v => _outputPrefix = v
                },
                {
                    "ref|r=",
                    "input reference {path}",
                    v => _refSequencePath = v
                }
            };

            string commandLineExample = $"{command} --in <cache prefix> --in2 <cache prefix> --out <cache prefix> --ref <reference path>";

            return new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .CheckInputFilenameExists(_refSequencePath, "reference sequence", "--ref")
                .HasRequiredParameter(_inputPrefix, "input cache", "--in")
                .HasRequiredParameter(_inputPrefix2, "input cache 2", "--in2")
                .HasRequiredParameter(_outputPrefix, "output cache", "--out")
                .SkipBanner()
                .ShowHelpMenu("Combines two cache sets into one cache.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
