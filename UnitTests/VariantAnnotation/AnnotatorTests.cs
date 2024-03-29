﻿using ErrorHandling.Exceptions;
using Genome;
using Moq;
using UnitTests.TestUtilities;
using VariantAnnotation;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation
{
    public sealed class AnnotatorTest
    {
        private static IVariant[] GetVariants()
        {
            var variant = new Mock<IVariant>();
            variant.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr1);
            variant.SetupGet(x => x.Type).Returns(VariantType.SNV);
            variant.SetupGet(x => x.Start).Returns(949523);
            variant.SetupGet(x => x.End).Returns(949523);
            variant.SetupGet(x => x.RefAllele).Returns("C");
            variant.SetupGet(x => x.AltAllele).Returns("T");
            variant.SetupGet(x => x.Behavior).Returns(AnnotationBehavior.SmallVariants);
            return new[] { variant.Object };
        }

        private static IVariant[] GetMitoVariants()
        {
            var variant = new Mock<IVariant>();
            variant.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.ChrM);
            variant.SetupGet(x => x.Type).Returns(VariantType.SNV);
            variant.SetupGet(x => x.Start).Returns(9495);
            variant.SetupGet(x => x.End).Returns(9495);
            variant.SetupGet(x => x.RefAllele).Returns("C");
            variant.SetupGet(x => x.AltAllele).Returns("T");
            variant.SetupGet(x => x.Behavior).Returns(AnnotationBehavior.SmallVariants);
            return new[] { variant.Object };
        }

        [Fact]
        public void Annotate_conservation_annotation()
        {
            var position = new Mock<IPosition>();
            position.SetupGet(x => x.Variants).Returns(GetMitoVariants);
            position.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr1);

            var csProvider = new Mock<IAnnotationProvider>();
            csProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            csProvider.Setup(x => x.Annotate(It.IsAny<IAnnotatedPosition>())).
                Callback((IAnnotatedPosition x) => { x.CytogeneticBand = "testCytoBand"; });

            var taProvider = new Mock<IAnnotationProvider>();
            taProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            taProvider.Setup(x => x.Annotate(It.IsAny<IAnnotatedPosition>())).Callback((IAnnotatedPosition x) => { });//do nothing

            var annotator = new Annotator(taProvider.Object, null, null, csProvider.Object, null, null, null, null);

            var annotatedPosition = annotator.Annotate(position.Object);

            Assert.Equal("testCytoBand", annotatedPosition.CytogeneticBand);
        }

        [Fact]
        public void Annotate_mito_hg19()
        {
            var position = new Mock<IPosition>();
            position.SetupGet(x => x.Variants).Returns(GetVariants);
            position.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.ChrM);

            var csProvider = new Mock<IAnnotationProvider>();
            csProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            csProvider.Setup(x => x.Annotate(It.IsAny<IAnnotatedPosition>())).
                Callback((IAnnotatedPosition x) => { x.CytogeneticBand = "testCytoBand"; });

            var taProvider = new Mock<IAnnotationProvider>();
            taProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            taProvider.Setup(x => x.Annotate(It.IsAny<IAnnotatedPosition>())).Callback((IAnnotatedPosition x) => { });//do nothing

            var annotator = new Annotator(taProvider.Object, null, null, csProvider.Object, null, null, null, null);

            var annotatedPosition = annotator.Annotate(position.Object);

            Assert.Null(annotatedPosition.CytogeneticBand);
        }

        [Fact]
        public void Annotate_mito_GRCh37()
        {
            var position = new Mock<IPosition>();
            position.SetupGet(x => x.Variants).Returns(GetVariants);
            position.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.ChrM);

            var csProvider = new Mock<IAnnotationProvider>();
            csProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            csProvider.Setup(x => x.Annotate(It.IsAny<IAnnotatedPosition>())).
                Callback((IAnnotatedPosition x) => { x.CytogeneticBand = "testCytoBand"; });

            var taProvider = new Mock<IAnnotationProvider>();
            taProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            taProvider.Setup(x => x.Annotate(It.IsAny<IAnnotatedPosition>())).Callback((IAnnotatedPosition x) => { });//do nothing

            var annotator = new Annotator(taProvider.Object, null, null, csProvider.Object, null, null, null, null);
            annotator.EnableMitochondrialAnnotation();

            var annotatedPosition = annotator.Annotate(position.Object);

            Assert.NotNull(annotatedPosition.CytogeneticBand);
        }




        [Fact]
        public void Annotate_null_position()
        {
            var annotator         = new Annotator(null, null, null, null, null, null, null, null);
            var annotatedPosition = annotator.Annotate(null);

            Assert.Null(annotatedPosition);
        }

        //[Fact]
        //public void TrackAffectedGenes()
        //{
        //    var taProvider = new Mock<IAnnotationProvider>();
        //    taProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
        //    taProvider.Setup(x => x.Annotate(It.IsAny<IAnnotatedPosition>())).Callback((IAnnotatedPosition x) => { });//do nothing
        //    var geneAnnotationProvider = new Mock<IGeneAnnotationProvider>();
        //    geneAnnotationProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

        //    var annotator          = new Annotator(taProvider.Object, null, null, null, geneAnnotationProvider.Object);
        //    var annotatedPosition  = new Mock<IAnnotatedPosition>();
        //    var annotatedVariant   = new Mock<IAnnotatedVariant>();
        //    var ensembleTranscript = new Mock<IAnnotatedTranscript>();
        //    annotatedVariant.SetupGet(x => x.EnsemblTranscripts)
        //        .Returns(new List<IAnnotatedTranscript> { ensembleTranscript.Object });
        //    ensembleTranscript.SetupGet(x => x.Transcript.Gene.Symbol).Returns("ensembl1");

        //    var refSeqTranscript = new Mock<IAnnotatedTranscript>();
        //    annotatedVariant.SetupGet(x => x.RefSeqTranscripts)
        //        .Returns(new List<IAnnotatedTranscript> { refSeqTranscript.Object });
        //    refSeqTranscript.SetupGet(x => x.Transcript.Gene.Symbol).Returns("refseq1");

        //    annotatedPosition.SetupGet(x => x.AnnotatedVariants).Returns(new[] { annotatedVariant.Object });

        //    annotator.TrackAffectedGenes(annotatedPosition.Object);

        //    var geneAnnotation = new Mock<IAnnotatedGene>();
        //    geneAnnotationProvider.Setup(x => x.Annotate("ensembl1")).Returns(geneAnnotation.Object);
        //    geneAnnotationProvider.Setup(x => x.Annotate("refseq1")).Returns((string)null);

        //    var annotatedGenes = annotator.GetGeneAnnotations();
        //    Assert.Equal(1, annotatedGenes.Count);
        //}
        
        [Fact]
        public void CheckAssemblyConsistency_Consistent()
        {
            var taProvider = new Mock<IAnnotationProvider>();
            taProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var saProvider = new Mock<IAnnotationProvider>();
            saProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var csProvider = new Mock<IAnnotationProvider>();
            csProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var omimProvider = new Mock<IGeneAnnotationProvider>();
            omimProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var annotator = new Annotator(taProvider.Object, null, saProvider.Object, csProvider.Object, null, omimProvider.Object, null, null);

            Assert.NotNull(annotator);
        }

        [Fact]
        public void CheckAssemblyConsistency_Inconsistent()
        {
            var taProvider = new Mock<IAnnotationProvider>();
            taProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var saProvider = new Mock<IAnnotationProvider>();
            saProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            var csProvider = new Mock<IAnnotationProvider>();
            csProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh38);

            var omimProvider = new Mock<IGeneAnnotationProvider>();
            omimProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);

            Assert.Throws<UserErrorException>(() => new Annotator(taProvider.Object, null, saProvider.Object, csProvider.Object, null, omimProvider.Object, null, null));
        }
    }
}