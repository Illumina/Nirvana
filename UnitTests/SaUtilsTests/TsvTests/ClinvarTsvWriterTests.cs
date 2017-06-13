using System.Collections.Generic;
using Moq;
using SAUtils.DataStructures;
using SAUtils.TsvWriters;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.SaUtilsTests.TsvTests
{
	public class ClinvarTsvWriterTests
	{
		[Fact(Skip="Bad output options")]
		public void DuplicateRcvItemsShouldBeRmoveWhenWritingTsv()
		{
			var datasourceVersion = new DataSourceVersion("clinvar","2017",2017);

			var mockTsvWriter = new Mock<SaTsvWriter>("",datasourceVersion,"",1,"","",false,false);
			var jsonStringsCount = 0;
			mockTsvWriter.Setup(
				x =>
					x.AddEntry(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<List<string>>()))
				.Callback((string s1, int i1, string s2, string s3, string s4, List<string> l1) => jsonStringsCount = l1.Count);
			using (var clinvarTsvWriter = new ClinvarTsvWriter(mockTsvWriter.Object))
			{
				var item1 = new ClinVarItem("chr1", 1234, new List<string> { "germLine" }, "T", "RCV1234", "", null, null, null, null,
				"A", "pathogenic");
				var item2 = new ClinVarItem("chr1", 1234, new List<string> { "germLine" }, "T", "RCV1234", "", null, null, null, null,
					"A", "pathogenic");
				var itemList = new List<ClinVarItem> { item1, item2 };
				clinvarTsvWriter.WritePosition(itemList);
			}
			
			Assert.Equal(1, jsonStringsCount);
		}
	}
}