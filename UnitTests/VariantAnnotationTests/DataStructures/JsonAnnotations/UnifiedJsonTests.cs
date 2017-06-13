using System.Linq;
using Moq;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.DataStructures.Variants;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.VariantAnnotationTests.DataStructures.JsonAnnotations
{
    public sealed class UnifiedJsonTests
    {

        [Fact]
        public void DuplicatedTranscriptsAreBothAddedForOverlappingTranscripts()
        {
            var variant= new Mock<IVariant>();
            var renamer = new Mock<IChromosomeRenamer>();

            var vcfFields = new[] {"chr1", "105", ".", "A", "<DEL>", ".", ".", "END=120;SVTYPE=DEL"}; //not really in use
            variant.SetupGet(x => x.Fields).Returns(vcfFields);

            var variantFeature = new VariantFeature(variant.Object,renamer.Object,new VID());

            var unifiedJson = new UnifiedJson(variantFeature);

            var allele = new Mock<IAllele>();
            allele.SetupGet(x => x.SuppAltAllele).Returns("A");
            allele.SetupGet(x => x.IsStructuralVariant).Returns(true);
            var jsonVariant = new JsonVariant(allele.Object,variantFeature);


            var transcript = new VariantAnnotation.DataStructures.Transcript.Transcript(1,100,200,CompactId.Convert("ENST1243435"),1,null,BioType.ProteinCoding, new Gene(1,100,200,false,"gene12",1,CompactId.Empty, CompactId.Empty, 1234),5,0,true,null,null,new[] { CdnaCoordinateMap.Null()}, 0,0,TranscriptDataSource.Ensembl  );

            unifiedJson.AnnotatedAlternateAlleles.Add(jsonVariant);
            unifiedJson.AddOverlappingTranscript( transcript,allele.Object);
            unifiedJson.AddOverlappingTranscript(transcript, allele.Object);

            Assert.Equal(2,unifiedJson.AnnotatedAlternateAlleles.First().SvOverlappingTranscripts.Count);

        }


    }
}