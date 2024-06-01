using System.Collections;
using System.Linq;
using SAUtils.InputFileParsers.Cosmic;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class MergedCosmicReaderTests
    {
        [Fact]
        public void TwoTumorCosmicCoding()
        {
            var seqProvider = ParserTestUtils.GetSequenceProvider(35416, "A", 'C', ChromosomeUtilities.RefNameToChromosome);
            var cosmicReader = new MergedCosmicReader(Resources.TopPath("cosm5428243.vcf"), Resources.TopPath("cosm5428243.tsv"), seqProvider);

            var cosmicItem = cosmicReader.GetItems().ToList()[0];

            var tumors = cosmicItem.Tumors.ToList();

            Assert.Equal("2205513", tumors[0].Id);
            Assert.Equal("haematopoietic and lymphoid tissue" , tumors[0].Site);
            Assert.Equal("haematopoietic neoplasm", tumors[0].Histology);
            //Assert.Equal(new [] { "haematopoietic neoplasm", "acute myeloid leukaemia" }, tumor.Histologies);

            Assert.Equal("2205513", tumors[1].Id);
            Assert.Equal("haematopoietic;lymphoid tissue", tumors[1].Site);
            Assert.Equal("haematopoietic neoplasm", tumors[1].Histology);
            //Assert.Equal(new[] { "haematopoietic_neoplasm", "acute_myeloid_leukaemia" }, tumor.Histologies);
        }

        [Fact]
        public void IndelWithNoLeadingBase()
        {
            var seqProvider = ParserTestUtils.GetSequenceProvider(10188320, "GGTACTGAC", 'A', ChromosomeUtilities.RefNameToChromosome);
            //the files provided are just for the sake of construction. The main aim is to test the VCF line parsing capabilities
            var cosmicReader = new MergedCosmicReader(Resources.TopPath("cosm5428243.vcf"), Resources.TopPath("cosm5428243.tsv"), seqProvider);

            const string vcfLine1 = "3	10188320	COSM14426	GGTACTGAC	A	.	.	GENE=VHL;STRAND=+;CDS=c.463G>A;AA=p.?;CNT=2";
            const string vcfLine2 = "3	10188320	COSM18152	G	A	.	.	GENE=VHL;STRAND=+;CDS=c.463G>A;AA=p.V155M;CNT=7";

            var items = cosmicReader.ExtractCosmicItems(vcfLine1);
            Assert.Equal("GGTACTGAC", items[0].RefAllele);
            Assert.Equal("A", items[0].AltAllele);
            Assert.Equal(10188320, items[0].Position);

            var items2 = cosmicReader.ExtractCosmicItems(vcfLine2);
            Assert.Equal("G", items2[0].RefAllele);
            Assert.Equal("A", items2[0].AltAllele);
            Assert.Equal(10188320, items2[0].Position);
        }

        /// <summary>
        /// testing if cosmic alternate allele is correctly output
        /// </summary>
        [Fact]
        public void CosmicAltAllele()
        {
            var seqProvider = ParserTestUtils.GetSequenceProvider(6928019, "C", 'A', ChromosomeUtilities.RefNameToChromosome);
            var cosmicReader = new MergedCosmicReader(Resources.TopPath("COSM983708.vcf"), Resources.TopPath("COSM983708.tsv"), seqProvider);
            var items = cosmicReader.GetItems().ToList();

            Assert.Single((IEnumerable) items);
            Assert.Contains("\"refAllele\":\"-\"", items[0].GetJsonString());
        }

        [Fact]
        public void CosmicAlleleSpecificIndel()
        {
            //10188320
            var seqProvider = ParserTestUtils.GetSequenceProvider(10188320, "G", 'A', ChromosomeUtilities.RefNameToChromosome);
            var cosmicReader = new MergedCosmicReader(Resources.TopPath("COSM18152.vcf"), Resources.TopPath("COSM18152.tsv"), seqProvider);
            var items = cosmicReader.GetItems();

            Assert.Single(items);
        }
    }
}
