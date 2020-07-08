using System;
using System.IO;
using Genome;
using Moq;
using SAUtils.gnomAD;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.VariantAnnotation.ProviderTests
{
    public class LcrProviderTests
    {
        private Stream GetNsiStream()
        {
            var stream = new MemoryStream();
            var version = new DataSourceVersion("test", "June_2020", DateTime.Now.Ticks, "dummy");
            using (var writer = new NsiWriter(stream, version, GenomeAssembly.GRCh37, SaCommon.LowComplexityRegionTag,
                ReportFor.AllVariants,
                SaCommon.NsiSchemaVersion, true))
            {
                writer.Write(new []
                {
                    new LcrInterval(ChromosomeUtilities.Chr1, 100, 150),
                    new LcrInterval(ChromosomeUtilities.Chr1, 300, 450),
                    new LcrInterval(ChromosomeUtilities.Chr1, 600, 650),
                    new LcrInterval(ChromosomeUtilities.Chr2, 100, 150),
                    new LcrInterval(ChromosomeUtilities.Chr2, 300, 450),
                    new LcrInterval(ChromosomeUtilities.Chr2, 600, 650),
                });
            }

            stream.Position = 0;

            return stream;
        }

        private IAnnotatedVariant GetAnnotatedVariant(IChromosome chromosome, int start, int end)
        {
            var annoVariant = new Mock<IAnnotatedVariant>();
            annoVariant.SetupGet(x => x.Variant.Chromosome).Returns(chromosome);
            annoVariant.SetupGet(x => x.Variant.Start).Returns(start);
            annoVariant.SetupGet(x => x.Variant.End).Returns(end);
            annoVariant.SetupProperty(x => x.InLowComplexityRegion);
            return annoVariant.Object;
        }

        private IAnnotatedPosition GetAnnotatedPosition(IChromosome chromosome, int start, int end)
        {
            var annoPosition = new Mock<IAnnotatedPosition>();
            annoPosition.SetupGet(x => x.AnnotatedVariants).Returns(
                new []
                {
                    GetAnnotatedVariant(chromosome, start, end)
                }
                );
            
            return annoPosition.Object;
        }

        [Fact]
        public void AddAnnotationsTest()
        {
            using (var provider = new LcrProvider(GetNsiStream()))
            {
                var position = GetAnnotatedPosition(ChromosomeUtilities.Chr1, 50, 70);
                provider.Annotate(position);

                Assert.False(position.AnnotatedVariants[0].InLowComplexityRegion);
                
                position = GetAnnotatedPosition(ChromosomeUtilities.Chr1, 110, 160);
                provider.Annotate(position);

                Assert.True(position.AnnotatedVariants[0].InLowComplexityRegion);
                
                position = GetAnnotatedPosition(ChromosomeUtilities.Chr2, 110, 160);
                provider.Annotate(position);

                Assert.True(position.AnnotatedVariants[0].InLowComplexityRegion);
            }
        }
    }
}