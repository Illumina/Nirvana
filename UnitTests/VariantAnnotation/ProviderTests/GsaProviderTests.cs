using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using Genome;
using Moq;
using OptimizedCore;
using SAUtils.GenericScore;
using SAUtils.GenericScore.GenericScoreParser;
using UnitTests.TestUtilities;
using UnitTests.VariantAnnotation.ScoreFile;
using VariantAnnotation.GenericScore;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Pools;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Variants;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.VariantAnnotation.ProviderTests
{
    public sealed class GsaProviderTests
    {
        private static (ScoreReader, Dictionary<Chromosome, List<Dictionary<string, object>>>) GetScoreReaderWithData()
        {
            var testSetup = new Dictionary<Chromosome, List<Dictionary<string, object>>>
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
                            {"startPosition", 11_001},
                            {"endPosition", 23_500},
                        }
                    }
                },
            };

            return (TestDataGenerator.GetScoreReaderWithRandomData(testSetup), testSetup);
        }

        private static (ScoreProvider provider, Dictionary<Chromosome, List<Dictionary<string, object>>> providerTestData) GetScoreProvider()
        {
            (ScoreReader scoreReader, Dictionary<Chromosome, List<Dictionary<string, object>>> testData) = GetScoreReaderWithData();

            var provider = new ScoreProvider(new[] {scoreReader});
            return (provider, testData);
        }

        private static IAnnotatedPosition GetPosition(Chromosome chrom, int start, string refAllele, string[] altAlleles)
        {
            var position          = new Mock<IAnnotatedPosition>();
            var annotatedVariants = new List<IAnnotatedVariant>();
            foreach (string altAllele in altAlleles)
            {
                VariantType type = SmallVariantCreator.GetVariantType(refAllele, altAllele);
                int         end  = start + altAllele.Length - 1;

                var variant = VariantPool.Get(chrom, start, end, refAllele, altAllele, type, null, false, false, false,
                    null, AnnotationBehavior.SmallVariants, false);
                annotatedVariants.Add(AnnotatedVariantPool.Get(variant));
            }

            position.SetupGet(x => x.AnnotatedVariants).Returns(annotatedVariants.ToArray);
            return position.Object;
        }

        [Fact]
        public void TestAnnotateUsingScoreProvider()
        {
            (IAnnotationProvider provider, Dictionary<Chromosome, List<Dictionary<string, object>>> testSetup) = GetScoreProvider();

            foreach ((Chromosome chromosome, List<Dictionary<string, object>> chromosomeTests) in testSetup)
            {
                foreach (Dictionary<string, object> chromosomeTest in chromosomeTests)
                {
                    var expectedScores = (List<double>) chromosomeTest["expectedScores"];
                    var startPosition  = (int) chromosomeTest["startPosition"];
                    for (var i = 0; i < expectedScores.Count; i++)
                    {
                        IAnnotatedPosition position = GetPosition(chromosome, startPosition + i, "T", new[] {"A"});
                        provider.Annotate(position);

                        var sb         = position.AnnotatedVariants[0].GetJsonStringBuilder(chromosome.UcscName);
                        var jsonString = sb.ToString();
                        StringBuilderPool.Return(sb);
                        var expectedScore = $"{Math.Round(expectedScores[i], 2):0.##}";
                        var expectedString =
                            "{\"chromosome\":\"" + $"{chromosome.UcscName}\"," +
                            "\"begin\":"         + $"{startPosition + i},"     +
                            "\"end\":"           + $"{startPosition + i},"     +
                            "\"refAllele\":"     + "\"T\","                    +
                            "\"altAllele\":"     + "\"A\","                    +
                            "\"variantType\":"   + "\"SNV\","                  +
                            "\"TestKey\":{"      +
                            "\"TestSubKey\":"    + $"{expectedScore}" +
                            "}}";

                        Assert.Equal(expectedString, jsonString);
                    }
                }
            }
        }

        [Fact]
        private void TestUnknownPosition()
        {
            (IAnnotationProvider provider, Dictionary<Chromosome, List<Dictionary<string, object>>> testSetup) = GetScoreProvider();
            IAnnotatedPosition position = GetPosition(ChromosomeUtilities.Chr1, 5_000, "T", new[] {"A"});
            provider.Annotate(position);
            Assert.Empty(position.AnnotatedVariants[0].SaList);

            // Unknown Chromosome
            position = GetPosition(ChromosomeUtilities.Chr7, 5_000, "T", new[] {"A"});
            provider.Annotate(position);
            Assert.Empty(position.AnnotatedVariants[0].SaList);
        }

        [Fact]
        private void TestUnknownAssembly()
        {
            var      version     = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");
            string[] nucleotides = {"A", "C", "G", "T"};
            var writerSettings = new WriterSettings(
                10_000,
                nucleotides,
                new ScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator()
            );

            var position = 10_010;
            using (var dataStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            {
                using (var saWriter = new ScoreFileWriter(
                           writerSettings,
                           dataStream,
                           indexStream,
                           version,
                           GenericScoreTests.GetAllASequenceProvider(GenomeAssembly.Unknown),
                           SaCommon.SchemaVersion,
                           skipIncorrectRefEntries: false,
                           leaveOpen: true
                       ))
                {
                    IEnumerable<GenericScoreItem> items = new List<GenericScoreItem>
                    {
                        new(ChromosomeUtilities.Chr1, position, "A", "C", 0.5),
                    };
                    saWriter.Write(items);
                }

                dataStream.Position  = 0;
                indexStream.Position = 0;

                ScoreReader scoreReader = ScoreReader.Read(dataStream, indexStream);
                Assert.Throws<UserErrorException>(() => new ScoreProvider(new[] {scoreReader}));
            }
        }
    }
}