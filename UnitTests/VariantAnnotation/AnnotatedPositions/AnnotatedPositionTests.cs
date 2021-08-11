using Moq;
using OptimizedCore;
using UnitTests.SAUtils.InputFileParsers;
using UnitTests.TestUtilities;
using VariantAnnotation;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Pools;
using Variants;
using Vcf;
using Vcf.Info;
using Vcf.Sample;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedPositionTests
    {
        [Fact]
        public void GetJsonString_DifferentOriginalChromosomeName()
        {
            const string originalChromosomeName = "originalChr1";

            IVariant[] variants = GetVariants();
            ISample[] samples   = GetSamples();
            IAnnotatedVariant[] annotatedVariants = Annotator.GetAnnotatedVariants(variants);

            var position          = GetPosition(originalChromosomeName, variants, samples);
            var annotatedPosition = AnnotatedPositionPool.Get(position, annotatedVariants);
            
            var    sb             = annotatedPosition.GetJsonStringBuilder();
            var    observedResult = sb.ToString();
            StringBuilderPool.Return(sb);
            PositionPool.Return((Position)annotatedPosition.Position);
            AnnotatedPositionPool.Return(annotatedPosition);

            Assert.NotNull(observedResult);
            Assert.Contains($"\"chromosome\":\"{originalChromosomeName}\"", observedResult);
        }

        [Fact]
        public void GetJsonString_NullAnnotatedVariants()
        {
            const string originalChromosomeName = "originalChr1";

            var position          = GetPosition(originalChromosomeName, null, null);
            var annotatedPosition = AnnotatedPositionPool.Get(position, null);

            var sb= annotatedPosition.GetJsonStringBuilder();
            AnnotatedPositionPool.Return(annotatedPosition);
            
            Assert.Null(sb);
        }
        
        //21    9411410    .    C    T    9.51    DRAGENSnpHardQUAL    AC=2;AF=1.000;AN=2;DP=2;FS=0.000;MQ=100.00;QD=9.51;SOR=1.609    GT:AD:AF:DP:GQ:FT:F1R2:F2R1:PL:GP:PP    ./.:.:.:0:0:.:.:.    ./.:.:.:0:0:.:.:.    1/1:0,1:1.000:1:3:PASS:0,1:0,0:45,3,0:1.0415e+01,3.4301e+00,3.4199e+00:45,3,0
        [Fact]
        public void GetJsonString_fisherStrand()
        {
            const string vcfLine = "21\t9411410\t.\tC\tT\t9.51\tDRAGENSnpHardQUAL\tAC=2;AF=1.000;AN=2;DP=2;FS=0.000;MQ=100.00;QD=9.51;SOR=1.609";

            var refMinorProvider = new Mock<IRefMinorProvider>();
            var seqProvider      = ParserTestUtils.GetSequenceProvider(9411410, "C", 'A', ChromosomeUtilities.RefNameToChromosome);
            var variantFactory   = new VariantFactory(seqProvider.Sequence, new VariantId());

            var position = AnnotationUtilities.ParseVcfLine(vcfLine, refMinorProvider.Object, seqProvider, null, variantFactory);

            IVariant[]          variants          = GetVariants();
            IAnnotatedVariant[] annotatedVariants = Annotator.GetAnnotatedVariants(variants);
            var                 annotatedPosition = AnnotatedPositionPool.Get(position, annotatedVariants);

            var sb             = annotatedPosition.GetJsonStringBuilder();
            var observedResult = sb.ToString();
            StringBuilderPool.Return(sb);
            AnnotatedPositionPool.Return(annotatedPosition);

            Assert.NotNull(observedResult);
            Assert.Contains("\"fisherStrandBias\":0", observedResult);
        }

        [Fact]
        public void GetJsonString_StrelkaSomatic()
        {
            const string vcfLine = "chr1	13813	.	T	G	.	LowQscore	SOMATIC;QSS=33;TQSS=1;NT=ref;QSS_NT=16;TQSS_NT=1;SGT=TT->GT;DP=266;MQ=23.89;MQ0=59;ALTPOS=69;ALTMAP=37;ReadPosRankSum=1.22;SNVSB=5.92;PNOISE=0.00;PNOISE2=0.00;VQSR=1.93;FS=12.123";

            var refMinorProvider = new Mock<IRefMinorProvider>();
            var seqProvider = ParserTestUtils.GetSequenceProvider(13813, "T", 'C', ChromosomeUtilities.RefNameToChromosome);
            var variantFactory = new VariantFactory(seqProvider.Sequence, new VariantId());

            var position = AnnotationUtilities.ParseVcfLine(vcfLine, refMinorProvider.Object, seqProvider, null, variantFactory);

            IVariant[]          variants          = GetVariants();
            IAnnotatedVariant[] annotatedVariants = Annotator.GetAnnotatedVariants(variants);
            var                 annotatedPosition = AnnotatedPositionPool.Get(position, annotatedVariants);

            var sb             = annotatedPosition.GetJsonStringBuilder();
            var observedResult = sb.ToString();
            StringBuilderPool.Return(sb);
            AnnotatedPositionPool.Return(annotatedPosition);

            Assert.NotNull(observedResult);
            Assert.Contains("\"jointSomaticNormalQuality\":16", observedResult);
            Assert.Contains("\"recalibratedQuality\":1.93", observedResult);
            Assert.Contains("\"mappingQuality\":23.89", observedResult);
            Assert.Contains("\"fisherStrandBias\":12.123", observedResult);
        }
        
        [Fact]
        public void GetJsonString_BreakEndEventId()
        {
            const string vcfLine = "1\t38432782\tMantaBND:2312:0:1:0:0:0:0\tG\tG]6:28863899]\t971\tPASS\tSVTYPE=BND;MATEID=MantaBND:2312:0:1:0:0:0:1;EVENT=MantaBND:2312:0:1:0:0:0:0;JUNCTION_QUAL=716;BND_DEPTH=52;MATE_BND_DEPTH=56";

            var refMinorProvider = new Mock<IRefMinorProvider>();
            var seqProvider      = ParserTestUtils.GetSequenceProvider(38432782, "G", 'C', ChromosomeUtilities.RefNameToChromosome);
            var variantFactory   = new VariantFactory(seqProvider.Sequence, new VariantId());

            var position = AnnotationUtilities.ParseVcfLine(vcfLine, refMinorProvider.Object, seqProvider, null, variantFactory);

            IVariant[]          variants          = GetVariants();
            IAnnotatedVariant[] annotatedVariants = Annotator.GetAnnotatedVariants(variants);
            var                 annotatedPosition = AnnotatedPositionPool.Get(position, annotatedVariants);

            var sb             = annotatedPosition.GetJsonStringBuilder();
            var observedResult = sb.ToString();
            StringBuilderPool.Return(sb);
            PositionPool.Return((Position)annotatedPosition.Position);
            AnnotatedPositionPool.Return(annotatedPosition);

            Assert.NotNull(observedResult);
            Assert.Contains("\"breakendEventId\":\"MantaBND:2312:0:1:0:0:0:0\"", observedResult);
        }

        private static ISample[] GetSamples() => new ISample[] { Sample.EmptySample };

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

        private static IPosition GetPosition(string originalChromosomeName, IVariant[] variants, ISample[] samples)
        {
            var vcfFields = new string[8];
            vcfFields[0] = originalChromosomeName;
            InfoData infoData = new InfoDataBuilder().Create();

            return PositionPool.Get(ChromosomeUtilities.Chr1, 949523, 949523, "C", new[] {"T"}, null, null, variants, samples, infoData,
                vcfFields, new[] { false }, false);
        }
    }
}
