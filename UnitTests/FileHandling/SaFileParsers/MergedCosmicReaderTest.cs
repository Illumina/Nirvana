using SAUtils.InputFileParsers.Cosmic;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.FileHandling.SaFileParsers
{
	public sealed class MergedCosmicReaderTest
	{
		[Fact]
		public void TwoStudyCosmicCoding()
		{
			var cosmicReader =
				new MergedCosmicReader(@"Resources\cosm5428243.vcf", @"Resources\cosm5428243.tsv");

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
			
		}

		[Fact]
		public void IndelWithNoLeadingBase()
		{
			var cosmicReader = new MergedCosmicReader(null, null);

			const string vcfLine1 = "3	10188320	COSM14426	GGTACTGAC	A	.	.	GENE=VHL;STRAND=+;CDS=c.463G>A;AA=p.?;CNT=2";
			const string vcfLine2 = "3	10188320	COSM18152	G	A	.	.	GENE=VHL;STRAND=+;CDS=c.463G>A;AA=p.V155M;CNT=7";

			var sa = new SupplementaryAnnotation(10188320);
			foreach (var cosmicItem in cosmicReader.ExtractCosmicItems(vcfLine1))
			{
				cosmicItem.SetSupplementaryAnnotations(sa);
			}

			Assert.Equal("9A",sa.CosmicItems[0].SaAltAllele);

			foreach (var cosmicItem in cosmicReader.ExtractCosmicItems(vcfLine2))
			{
				cosmicItem.SetSupplementaryAnnotations(sa);
			}

			Assert.Equal("A", sa.CosmicItems[1].SaAltAllele);

		}

	}
}
