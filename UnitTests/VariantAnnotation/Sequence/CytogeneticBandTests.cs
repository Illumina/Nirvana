using System.IO;
using System.Text;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.Sequence
{
    public sealed class CytogeneticBandTests
    {
		private CytogeneticBands GetCytogeneticBands()
        {
            var bands = new Band[25][];

            for (int i = 0; i < 25; i++)
            {
                if (i == 10)
                {
                    bands[i] = new Band[2];
                    bands[i][0] = new Band(88300001, 92800000, "q14.3");
                    bands[i][1] = new Band(92800001, 97200000, "q21");
                }
                else
                {
                    bands[i] = new Band[0];
                }
            }

            return new CytogeneticBands(bands);
        }

        [Fact]
        public void Serialization()
        {
            ICytogeneticBands expectedCytogeneticBands = GetCytogeneticBands();
            ICytogeneticBands observedCytogeneticBands;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    expectedCytogeneticBands.Write(writer);
                }

                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    var bands = CytogeneticBands.Read(reader);
                    observedCytogeneticBands = new CytogeneticBands(bands);
                }
            }

			var chromosome = new Chromosome("chr11","11",10);
            var expectedCytogeneticBand = expectedCytogeneticBands.GetCytogeneticBand(chromosome, 88400000, 92900000);
            var observedCytogeneticBand = observedCytogeneticBands.GetCytogeneticBand(chromosome, 88400000, 92900000);
            Assert.NotNull(observedCytogeneticBand);
            Assert.Equal(expectedCytogeneticBand, observedCytogeneticBand);
        }

        [Theory]
        [InlineData(88400000, 92900000, "11q14.3-q21")]
        [InlineData(88400000, 92400000, "11q14.3")]
        [InlineData(92820001, 92900000, "11q21")]
        [InlineData(92820001, 92820001, "11q21")]
        [InlineData(1, 1, null)]
        [InlineData(97000000, 98200000, null)]
        public void GetCytogeneticBand_Range(int start, int end, string expectedCytogeneticBand)
        {
            var cytogeneticBands = GetCytogeneticBands();
	        var chromosome = new Chromosome("chr11", "11", 10);
			var observedCytogeneticBand = cytogeneticBands.GetCytogeneticBand(chromosome, start, end);

            Assert.Equal(expectedCytogeneticBand, observedCytogeneticBand);
        }

        [Fact]
		public void GetCytogeneticBand_UnknownReference_ReturnNull()
        {
            var cytogeneticBands = GetCytogeneticBands();
	        var chromosome = new Chromosome("12", "chr12", 11);
			var observedCytogeneticBand = cytogeneticBands.GetCytogeneticBand(chromosome, 100, 200);
            Assert.Null(observedCytogeneticBand);
        }

        [Fact]
        public void GetCytogeneticBand_UnknownReferenceIndex_ReturnNull()
        {
            var cytogeneticBands = GetCytogeneticBands();
            var chromosome = new Chromosome("bob", "bob", CompressedSequenceReader.UnknownReferenceIndex);
            var observedCytogeneticBand = cytogeneticBands.GetCytogeneticBand(chromosome, 100, 200);
            Assert.Null(observedCytogeneticBand);
        }

        [Theory(Skip = "need a functional annotator to execute them")]
		[InlineData("ENST00000368232_chr1_Ensembl84", "chr1\t156565049\t.\tA\tAAC\t3070.00\tPASS\t.", "1q23.1")]
        [InlineData("ENST00000416839_chr1_Ensembl84", "chr1\t220603308\t.\tTGTGTGA\tT,TGT\t40.00\tLowGQXHetDel\t.", "1q41")]
        [InlineData("ENST00000600779_chr1_Ensembl84", "chr1\t2258668\t.\tGACACAGAAAC\tG\t688.00\tPASS\t.", "1p36.33")]
        [InlineData("ENST00000464439_chr1_Ensembl84", "chr1\t47280746\t.\tGAT\tG\t98.00\tPASS\t.", "1p33")]
        [InlineData("ENST00000391369_chr1_Ensembl84", "chr1\t32379996\t.\tCTATT\tC\t98.00\tPASS\t.", "1p35.2")]
		public void EndToEnd(string cacheFileName, string vcfLine, string expectedCytogeneticBand)
        {
            //var annotatedVariant = AnnotationUtilities.GetVariant(Resources.CacheGRCh37(cacheFileName), null, vcfLine);
            //Assert.NotNull(annotatedVariant);

            //var expectedJsonEntry = $"\"cytogeneticBand\":\"{expectedCytogeneticBand}\"";
            //Assert.Contains(expectedJsonEntry, annotatedVariant.ToString());
        }
    }
}
