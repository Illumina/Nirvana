using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using SAUtils.GenericScore;
using SAUtils.GenericScore.GenericScoreParser;
using UnitTests.TestDataStructures;
using VariantAnnotation.GenericScore;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace UnitTests.TestUtilities
{
    public static class TestDataGenerator
    {
        public static void GenerateTestData(
            Dictionary<Chromosome, List<Dictionary<string, object>>> testSetup,
            List<GenericScoreItem> saItems,
            Func<int, int, double> scoreFunc,
            ISequenceProvider sequenceProvider
        )
        {
            foreach ((Chromosome chromosome, List<Dictionary<string, object>> chromosomeTests) in testSetup)
            {
                foreach (Dictionary<string, object> chromosomeTest in chromosomeTests)
                {
                    var startPosition = (int) chromosomeTest["startPosition"];
                    var endPosition   = (int) chromosomeTest["endPosition"];

                    var expectedScores = new List<double>();
                    for (int i = startPosition; i <= endPosition; i++)
                    {
                        double score = scoreFunc(i, endPosition);
                        expectedScores.Add(score);
                        string refAllele = sequenceProvider.Sequence.Substring(i - 1, 1);
                        saItems.Add(new GenericScoreItem(chromosome, i, refAllele, "A", score));
                    }

                    chromosomeTest["expectedScores"] = expectedScores;
                }
            }
        }

        public static void GenerateRandomScoreData(
            Dictionary<Chromosome, List<Dictionary<string, object>>> testSetup,
            List<GenericScoreItem> saItems,
            ISequenceProvider sequenceProvider
        )
        {
            var random = new Random(1);
            GenerateTestData(testSetup, saItems, (_, _) => Math.Round(random.NextDouble(), 8), sequenceProvider);
        }

        public static ISequenceProvider GetSequenceProvider()
        {
            var sequence = new SimpleSequence(new string('A', 99) + "TAGTCGGTTAA" + new string('A', 89) + "GCCCAT");
            return new SimpleSequenceProvider(GenomeAssembly.GRCh37, sequence, ChromosomeUtilities.RefNameToChromosome);
        }

        public static ScoreReader GetScoreReaderWithRandomData(
            Dictionary<Chromosome, List<Dictionary<string, object>>> testSetup
        )
        {
            string[] nucleotides = {"A", "C", "G", "T"};

            var writeStream = new MemoryStream();
            var indexStream = new MemoryStream();
            var saItems     = new List<GenericScoreItem>();
            var version     = new DataSourceVersion("Test", "1", DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).Ticks, "No description");
            var writerSettings = new WriterSettings(
                10_000,
                nucleotides,
                new ScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator()
            );

            // Scoring function to fill random scores
            GenerateRandomScoreData(testSetup, saItems, GetSequenceProvider());

            using var scoreFileWriter = new ScoreFileWriter(
                writerSettings,
                writeStream,
                indexStream,
                version,
                GetSequenceProvider(),
                SaCommon.SchemaVersion,
                leaveOpen: true
            );
            // Write saItems to stream
            scoreFileWriter.Write(saItems);

            // Reset streams in preparation for reading them
            indexStream.Position = 0;
            writeStream.Position = 0;

            // Read the scores
            return ScoreReader.Read(writeStream, indexStream);
        }

        public static (
            List<GenericScoreItem> saItems,
            WriterSettings writerSettings,
            MemoryStream indexStream,
            MemoryStream writeStream,
            DataSourceVersion
            version,
            Dictionary<Chromosome, List<Dictionary<string, object>>> testSetup
            ) GetRandomSingleChromosomeData(Chromosome chromosome, int startPosition, int endPosition)
        {
            const int blockLength = 10_000;

            string[] nucleotides = {"A", "C", "G", "T"};

            var testSetup = new Dictionary<Chromosome, List<Dictionary<string, object>>>
            {
                {
                    chromosome, new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"startPosition", startPosition},
                            {"endPosition", endPosition},
                        }
                    }
                },
            };

            var writeStream = new MemoryStream();
            var indexStream = new MemoryStream();
            var saItems     = new List<GenericScoreItem>();
            var version     = new DataSourceVersion("Test", "1", DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).Ticks, "No description");
            GenerateRandomScoreData(testSetup, saItems, TestDataGenerator.GetSequenceProvider());

            var writerSettings = new WriterSettings(
                blockLength,
                nucleotides,
                new ScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator()
            );

            return (saItems, writerSettings, indexStream, writeStream, version, testSetup);
        }
    }
}