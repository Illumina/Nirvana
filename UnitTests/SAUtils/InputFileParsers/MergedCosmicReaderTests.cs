using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Genome;
using SAUtils.InputFileParsers.Cosmic;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class MergedCosmicReaderTests
    {
        private readonly IDictionary<string, IChromosome> _refChromDict;

        /// <summary>
        /// constructor
        /// </summary>
        public MergedCosmicReaderTests()
        {
            _refChromDict = new Dictionary<string, IChromosome>
            {
                {"1",new Chromosome("chr1", "1",0) },
                {"3",new Chromosome("chr3", "3", 2) },
                {"17",new Chromosome("chr17", "17", 16) }
            };
        }
        [Fact]
        public void TwoStudyCosmicCoding()
        {
            var seqProvider = ParserTestUtils.GetSequenceProvider(35416, "A", 'C', _refChromDict);
            var cosmicReader = new MergedCosmicReader(Resources.TopPath("cosm5428243.vcf"), Resources.TopPath("cosm5428243.tsv"), seqProvider);

            var cosmicItem = cosmicReader.GetItems().ToList()[0];

            var studies = cosmicItem.Studies.ToList();

            Assert.Equal("544", studies[0].Id);
            Assert.Equal(new[] { "haematopoietic and lymphoid tissue" }, studies[0].Sites);
            Assert.Equal(new[] { "haematopoietic neoplasm" }, studies[0].Histologies);
            //Assert.Equal(new [] { "haematopoietic neoplasm", "acute myeloid leukaemia" }, study.Histologies);

            Assert.Equal("544", studies[1].Id);
            Assert.Equal(new[] { "haematopoietic;lymphoid tissue" }, studies[1].Sites);
            Assert.Equal(new[] { "haematopoietic neoplasm" }, studies[1].Histologies);
            //Assert.Equal(new[] { "haematopoietic_neoplasm", "acute_myeloid_leukaemia" }, study.Histologies);
        }

        [Fact]
        public void IndelWithNoLeadingBase()
        {
            var seqProvider = ParserTestUtils.GetSequenceProvider(10188320, "GGTACTGAC", 'A', _refChromDict);
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
            var seqProvider = ParserTestUtils.GetSequenceProvider(6928019, "C", 'A', _refChromDict);
            var cosmicReader = new MergedCosmicReader(Resources.TopPath("COSM983708.vcf"), Resources.TopPath("COSM983708.tsv"), seqProvider);
            var items = cosmicReader.GetItems().ToList();

            Assert.Single((IEnumerable) items);
            Assert.Contains("\"refAllele\":\"-\"", items[0].GetJsonString());
        }

        [Fact]
        public void CosmicAlleleSpecificIndel()
        {
            //10188320
            var seqProvider = ParserTestUtils.GetSequenceProvider(10188320, "G", 'A', _refChromDict);
            var cosmicReader = new MergedCosmicReader(Resources.TopPath("COSM18152.vcf"), Resources.TopPath("COSM18152.tsv"), seqProvider);
            var items = cosmicReader.GetItems();

            Assert.Single(items);
        }
    }
}
