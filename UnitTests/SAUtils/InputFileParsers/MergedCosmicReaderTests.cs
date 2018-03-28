using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;
using SAUtils.InputFileParsers.Cosmic;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    [Collection("ChromosomeRenamer")]
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
            var vcfReader = GZipUtilities.GetAppropriateStreamReader(Resources.TopPath("cosm5428243.vcf"));
            var tsvReader = GZipUtilities.GetAppropriateStreamReader(Resources.TopPath("cosm5428243.tsv"));
            var cosmicReader = new MergedCosmicReader(vcfReader, tsvReader, _refChromDict);

            var cosmicItems = cosmicReader.GetCosmicItems();
            var count = 0;
            foreach (var cosmicItem in cosmicItems)
            {
                switch (count)
                {
                    case 0:
                        foreach (var study in cosmicItem.Studies)
                        {
                            Assert.Equal("544", study.Id);
                            Assert.Equal(new [] {"haematopoietic_and_lymphoid_tissue"}, study.Sites);
                            Assert.Equal(new [] { "haematopoietic_neoplasm", "acute_myeloid_leukaemia" }, study.Histologies);
                        }
                        break;
                    case 1:
                        foreach (var study in cosmicItem.Studies)
                        {
                            Assert.Equal("544", study.Id);
                            Assert.Equal(new[] { "haematopoietic;lymphoid_tissue"}, study.Sites);
                            Assert.Equal(new[] { "haematopoietic_neoplasm", "acute_myeloid_leukaemia" }, study.Histologies);
                        }
                        break;
                }

                count++;
            }
        }

        [Fact]
        public void IndelWithNoLeadingBase()
        {
            var tsvReader = new StreamReader(new MemoryStream());
            var vcfReader = new StreamReader(new MemoryStream());
            var cosmicReader = new MergedCosmicReader(vcfReader, tsvReader, _refChromDict);

            const string vcfLine1 = "3	10188320	COSM14426	GGTACTGAC	A	.	.	GENE=VHL;STRAND=+;CDS=c.463G>A;AA=p.?;CNT=2";
            const string vcfLine2 = "3	10188320	COSM18152	G	A	.	.	GENE=VHL;STRAND=+;CDS=c.463G>A;AA=p.V155M;CNT=7";

            var items = cosmicReader.ExtractCosmicItems(vcfLine1);
            Assert.Equal("GGTACTGAC", items[0].ReferenceAllele);
            Assert.Equal("A", items[0].AlternateAllele);
            Assert.Equal(10188320, items[0].Start);

            var items2 = cosmicReader.ExtractCosmicItems(vcfLine2);
            Assert.Equal("G", items2[0].ReferenceAllele);
            Assert.Equal("A", items2[0].AlternateAllele);
            Assert.Equal(10188320, items2[0].Start);
        }

        /// <summary>
        /// testing if cosmic alternate allele is correctly output
        /// </summary>
        [Fact]
        public void CosmicAltAllele()
        {
            var vcfReader = GZipUtilities.GetAppropriateStreamReader(Resources.TopPath("COSM983708.vcf"));
            var tsvReader = GZipUtilities.GetAppropriateStreamReader(Resources.TopPath("COSM983708.tsv"));
            var cosmicReader = new MergedCosmicReader(vcfReader, tsvReader, _refChromDict);
            var items = cosmicReader.GetCosmicItems().ToList();

            Assert.Single(items);
            Assert.Contains("\"refAllele\":\"C\"", items[0].GetJsonString());
        }

        [Fact]
        public void CosmicAlleleSpecificIndel()
        {

            var vcfReader = GZipUtilities.GetAppropriateStreamReader(Resources.TopPath("COSM18152.vcf"));
            var tsvReader = GZipUtilities.GetAppropriateStreamReader(Resources.TopPath("COSM18152.tsv"));
            var cosmicReader = new MergedCosmicReader(vcfReader, tsvReader, _refChromDict);
            var items = cosmicReader.GetCosmicItems();

            Assert.Single(items);
        }
    }
}
