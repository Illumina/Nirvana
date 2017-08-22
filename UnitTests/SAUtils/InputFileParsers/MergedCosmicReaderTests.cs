using System.Collections.Generic;
using System.Linq;
using SAUtils.InputFileParsers.Cosmic;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.SaUtilsTests.InputFileParsers
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
                {"4",new Chromosome("chr4", "4", 3) }
            };
        }

        [Fact]
        public void TwoStudyCosmicCoding()
        {
            var cosmicReader = new MergedCosmicReader(Resources.TopPath("cosm5428243.vcf"), Resources.TopPath("cosm5428243.tsv"), _refChromDict);

            var enumerator = cosmicReader.GetEnumerator();
            enumerator.MoveNext();
            var cosmicItem = enumerator.Current;

            foreach (var study in cosmicItem.Studies)
            {
                Assert.Equal("544", study.ID);
                Assert.Equal("haematopoietic_and_lymphoid_tissue", study.PrimarySite);
                Assert.Equal("haematopoietic_neoplasm", study.Histology);
            }

            enumerator.MoveNext();
            cosmicItem = enumerator.Current;
            foreach (var study in cosmicItem.Studies)
            {
                Assert.Equal("544", study.ID);
                Assert.Equal("haematopoietic;lymphoid_tissue", study.PrimarySite);
                Assert.Equal("haematopoietic_neoplasm", study.Histology);
            }

            enumerator.Dispose();
        }

        [Fact(Skip = "new SA")]
        public void IndelWithNoLeadingBase()
        {
            //var cosmicReader = new MergedCosmicReader();

            //const string vcfLine1 = "3	10188320	COSM14426	GGTACTGAC	A	.	.	GENE=VHL;STRAND=+;CDS=c.463G>A;AA=p.?;CNT=2";
            //const string vcfLine2 = "3	10188320	COSM18152	G	A	.	.	GENE=VHL;STRAND=+;CDS=c.463G>A;AA=p.V155M;CNT=7";

            //var sa = new SupplementaryAnnotationPosition(10188320);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //foreach (var cosmicItem in cosmicReader.ExtractCosmicItems(vcfLine1))
            //{
            //    cosmicItem.SetSupplementaryAnnotations(saCreator);
            //}

            //Assert.Equal("9A", sa.CosmicItems[0].SaAltAllele);

            //foreach (var cosmicItem in cosmicReader.ExtractCosmicItems(vcfLine2))
            //{
            //    cosmicItem.SetSupplementaryAnnotations(saCreator);
            //}

            //Assert.Equal("A", sa.CosmicItems[1].SaAltAllele);
        }

        /// <summary>
        /// testing if cosmic alternate allele is correctly output
        /// </summary>
        [Fact(Skip="currently trim occurs in writeTSV")]
        public void CosmicAltAllele()
        {
            var cosmicReader = new MergedCosmicReader(Resources.TopPath("COSM983708.vcf"),
                Resources.TopPath("COSM983708.tsv"), _refChromDict);
            var items = cosmicReader.ToList();

            Assert.Equal(1,items.Count);
            Assert.Contains("\"refAllele\":\"-\"",items[0].GetJsonString());

        }

        [Fact]
        public void CosmicAlleleSpecificIndel()
        {

            var cosmicReader = new MergedCosmicReader(Resources.TopPath("COSM18152.vcf"),
                Resources.TopPath("COSM18152.tsv"), _refChromDict);
            var items = cosmicReader.ToList();

            Assert.Equal(3, items.Count);


        }

    }
}
