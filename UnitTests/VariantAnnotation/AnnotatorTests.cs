using ErrorHandling.Exceptions;
using Genome;
using Moq;
using UnitTests.TestUtilities;
using VariantAnnotation;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Providers;
using Variants;
using Vcf;
using Xunit;

namespace UnitTests.VariantAnnotation
{
    public sealed class AnnotatorTest
    {
        private static IVariant[] GetVariants()
        {
            var behavior = new AnnotationBehavior(false, false, false, false, false);
            var variant = new Mock<IVariant>();
            variant.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr1);
            variant.SetupGet(x => x.Type).Returns(VariantType.SNV);
            variant.SetupGet(x => x.Start).Returns(949523);
            variant.SetupGet(x => x.End).Returns(949523);
            variant.SetupGet(x => x.RefAllele).Returns("C");
            variant.SetupGet(x => x.AltAllele).Returns("T");
            variant.SetupGet(x => x.Behavior).Returns(behavior);
            return new[] { variant.Object };
        }

        private static IVariant[] GetMitoVariants()
        {
            var behavior = new AnnotationBehavior(false, false, false, false, false);
            var variant = new Mock<IVariant>();
            variant.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.ChrM);
            variant.SetupGet(x => x.Type).Returns(VariantType.SNV);
            variant.SetupGet(x => x.Start).Returns(9495);
            variant.SetupGet(x => x.End).Returns(9495);
            variant.SetupGet(x => x.RefAllele).Returns("C");
            variant.SetupGet(x => x.AltAllele).Returns("T");
            variant.SetupGet(x => x.Behavior).Returns(behavior);
            return new[] { variant.Object };
        }

        [Fact]
        public void Annotate_conservation_annotation()
        {
            var csProvider = new Mock<IAnnotationProvider>();
            csProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            csProvider.Setup(x => x.Annotate(It.IsAny<AnnotatedPosition>())).
                Callback((AnnotatedPosition x) => { x.CytogeneticBand = "testCytoBand"; });

            var taProvider = new Mock<IAnnotationProvider>();
            taProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            taProvider.Setup(x => x.Annotate(It.IsAny<AnnotatedPosition>())).Callback((AnnotatedPosition x) => { });//do nothing

            var annotator = new Annotator(taProvider.Object, null, null, csProvider.Object, null);

            var position = new Position(ChromosomeUtilities.Chr1, 100, 200, null, null, null, null, GetMitoVariants(),
                null, null, null, null, false);
            
            var annotatedPosition = annotator.Annotate(position);

            Assert.Equal("testCytoBand", annotatedPosition.CytogeneticBand);
        }

        [Fact]
        public void Annotate_mito_hg19()
        {
            var csProvider = new Mock<IAnnotationProvider>();
            csProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            csProvider.Setup(x => x.Annotate(It.IsAny<AnnotatedPosition>())).
                Callback((AnnotatedPosition x) => { x.CytogeneticBand = "testCytoBand"; });

            var taProvider = new Mock<IAnnotationProvider>();
            taProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            taProvider.Setup(x => x.Annotate(It.IsAny<AnnotatedPosition>())).Callback((AnnotatedPosition x) => { });//do nothing

            var annotator = new Annotator(taProvider.Object, null, null, csProvider.Object, null);

            var position = new Position(ChromosomeUtilities.ChrM, 100, 200, null, null, null, null, GetVariants(),
                null, null, null, null, false);
            
            var annotatedPosition = annotator.Annotate(position);

            Assert.Null(annotatedPosition.CytogeneticBand);
        }

        [Fact]
        public void Annotate_mito_GRCh37()
        {
            var csProvider = new Mock<IAnnotationProvider>();
            csProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            csProvider.Setup(x => x.Annotate(It.IsAny<AnnotatedPosition>())).
                Callback((AnnotatedPosition x) => { x.CytogeneticBand = "testCytoBand"; });

            var taProvider = new Mock<IAnnotationProvider>();
            taProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            taProvider.Setup(x => x.Annotate(It.IsAny<AnnotatedPosition>())).Callback((AnnotatedPosition x) => { });//do nothing

            var annotator = new Annotator(taProvider.Object, null, null, csProvider.Object, null);
            annotator.EnableMitochondrialAnnotation();
            
            var position = new Position(ChromosomeUtilities.ChrM, 100, 200, null, null, null, null, GetVariants(),
                null, null, null, null, false);

            var annotatedPosition = annotator.Annotate(position);

            Assert.NotNull(annotatedPosition.CytogeneticBand);
        }

        [Fact]
        public void Annotate_null_position()
        {
            var annotator = new Annotator(null, null, null, null, null);
            var annotatedPosition = annotator.Annotate(null);

            Assert.Null(annotatedPosition);
        }

        [Fact]
        public void CheckAssemblyConsistency_consistent()
        {
            var taProvider = new Mock<IAnnotationProvider>();
            taProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var saProvider = new Mock<ISaAnnotationProvider>();
            saProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var csProvider = new Mock<IAnnotationProvider>();
            csProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var omimProvider = new Mock<IGeneAnnotationProvider>();
            omimProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var annotator = new Annotator(taProvider.Object, null, saProvider.Object, csProvider.Object, omimProvider.Object);

            Assert.NotNull(annotator);
        }

        [Fact]
        public void CheckAssemblyConsistency_inconsistent()
        {
            var taProvider = new Mock<IAnnotationProvider>();
            taProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var saProvider = new Mock<ISaAnnotationProvider>();
            saProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var csProvider = new Mock<IAnnotationProvider>();
            csProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh38);

            var omimProvider = new Mock<IGeneAnnotationProvider>();
            omimProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            Assert.Throws<UserErrorException>(() => new Annotator(taProvider.Object, null, saProvider.Object, csProvider.Object, omimProvider.Object));
        }
    }
}