using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using SAUtils.GenericScore;
using SAUtils.GenericScore.GenericScoreParser;
using UnitTests.TestUtilities;
using VariantAnnotation.GenericScore;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.VariantAnnotation.ScoreFile
{
    public sealed class GenericScoreEndToEndTests
    {
        [Fact]
        public void ScoreWriterTestRandomData()
        {
            const int blockLength = 10_000;
            const int places      = 2;
            double    tol         = Math.Pow(10, -places);

            string[] nucleotides = {"A", "C", "G", "T"};

            var testSetup = new Dictionary<IChromosome, List<Dictionary<string, object>>>
            {
                // Normal Chromosome
                {
                    ChromosomeUtilities.Chr1, new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"startPosition", 10_001},
                            {"endPosition", 23_000},
                        }
                    }
                },
                // Chromosome with large gaps
                {
                    ChromosomeUtilities.Chr2, new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"startPosition", 24_001},
                            {"endPosition", 100_000},
                        },
                        // 5 Block gap
                        new Dictionary<string, object>
                        {
                            {"startPosition", 154_001},
                            {"endPosition", 200_000},
                        },
                    }
                },
                // Next chromosome starting at immediately next position to last chromosome ending position
                {
                    ChromosomeUtilities.Chr3, new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"startPosition", 200_001},
                            {"endPosition", 210_000},
                        },
                        new Dictionary<string, object>
                        {
                            {"startPosition", 210_001},
                            {"endPosition", 214_000},
                        },
                        // Short gap but still within the same block
                        new Dictionary<string, object>
                        {
                            {"startPosition", 215_001},
                            {"endPosition", 216_000},
                        },
                        // Larger gap to go to next block
                        new Dictionary<string, object>
                        {
                            {"startPosition", 221_001},
                            {"endPosition", 235_000},
                        },
                    }
                },
                // New chromosome with positions that preceed others
                {
                    ChromosomeUtilities.Chr4, new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"startPosition", 10_001},
                            {"endPosition", 21_000},
                        }
                    }
                },
            };

            var writeStream = new MemoryStream();
            var indexStream = new MemoryStream();
            var saItems     = new List<GenericScoreItem>();
            var version     = new DataSourceVersion("Test", "1", DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).Ticks, "No description");
            var writerSettings = new WriterSettings(
                blockLength,
                nucleotides,
                new ScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator()
            );

            // Scoring function to fill random scores
            TestDataGenerator.GenerateRandomScoreData(testSetup, saItems, TestDataGenerator.GetSequenceProvider());

            using (var scoreFileWriter = new ScoreFileWriter(
                       writerSettings,
                       writeStream,
                       indexStream,
                       version,
                       TestDataGenerator.GetSequenceProvider(),
                       SaCommon.SchemaVersion,
                       leaveOpen: true
                   ))
            {
                // Write saItems to stream
                scoreFileWriter.Write(saItems);

                // Reset streams in preparation for reading them
                indexStream.Position = 0;
                writeStream.Position = 0;

                // Read the scores
                ScoreReader scoreReader = ScoreReader.Read(writeStream, indexStream);

                // Assert scores are equal to what was set in test data
                AssertTestData(testSetup, scoreReader, blockLength, places, tol);

                // Scores in the gap
                Assert.Equal(double.NaN, scoreReader.GetScore(2, 100_001, "A"));

                // Scores for unspecified Allele
                Assert.Equal(double.NaN, scoreReader.GetScore(2, 100_001, "C"));
            }
        }

        [Fact]
        public void ScoreWriterTestDeterministicData()
        {
            const int blockLength = 10_000;
            const int places      = 2;
            double    tol         = Math.Pow(10, -places);

            string[] nucleotides = {"A", "C", "G", "T"};
            var testSetup = new Dictionary<IChromosome, List<Dictionary<string, object>>>
            {
                {
                    ChromosomeUtilities.Chr1, new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"startPosition", 10_001},
                            {"endPosition", 23_000},
                        }
                    }
                },
                {
                    ChromosomeUtilities.Chr2, new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"startPosition", 24_001},
                            {"endPosition", 100_000},
                        }
                    }
                },
            };

            var saItems     = new List<GenericScoreItem>();
            var writeStream = new MemoryStream();
            var indexStream = new MemoryStream();
            var version     = new DataSourceVersion("Test", "1", DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).Ticks, "No description");
            var writerSettings = new WriterSettings(
                10_000,
                nucleotides,
                new ScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator()
            );

            // Scoring function to fill scores from position
            double ScoreFunction(int i, int endPosition) => (double) i / endPosition;
            TestDataGenerator.GenerateTestData(testSetup, saItems, ScoreFunction, TestDataGenerator.GetSequenceProvider());

            using (var scoreFileWriter = new ScoreFileWriter(
                       writerSettings,
                       writeStream,
                       indexStream,
                       version,
                       TestDataGenerator.GetSequenceProvider(),
                       SaCommon.SchemaVersion,
                       leaveOpen: true
                   ))
            {
                // Write saItems to stream
                scoreFileWriter.Write(saItems);

                // Reset streams in preparation for reading them
                indexStream.Position = 0;
                writeStream.Position = 0;

                // Read the scores
                var scoreReader = ScoreReader.Read(writeStream, indexStream);

                // Assert scores are equal to what was set in test data
                AssertTestData(testSetup, scoreReader, blockLength, places, tol);
            }
        }

        private static void AssertTestData(Dictionary<IChromosome, List<Dictionary<string, object>>> testSetup, ScoreReader scoreReader,
            int blockLength,
            int places, double tol)
        {
            foreach ((IChromosome chromosome, List<Dictionary<string, object>> chromosomeTests) in testSetup)
            {
                foreach (Dictionary<string, object> chromosomeTest in chromosomeTests)
                {
                    var expectedScores = (List<double>) chromosomeTest["expectedScores"];
                    var startPosition  = (int) chromosomeTest["startPosition"];
                    for (var i = 0; i < expectedScores.Count; i++)
                    {
                        // Read score at position
                        double score = scoreReader.GetScore(chromosome.Index, startPosition + i, "A");
                        Assert.True(Math.Round(Math.Abs(expectedScores[i] - score), places) <= tol);
                    }
                }

                var chromosomeStartPosition = (int) chromosomeTests[0]["startPosition"];
                var chromosomeEndPosition   = (int) chromosomeTests[^1]["endPosition"];

                Assert.Equal(double.NaN, scoreReader.GetScore(chromosome.Index, chromosomeStartPosition - 1, "A"));
                Assert.Equal(double.NaN, scoreReader.GetScore(chromosome.Index, chromosomeEndPosition   + 1, "A"));
            }
        }
    }
}