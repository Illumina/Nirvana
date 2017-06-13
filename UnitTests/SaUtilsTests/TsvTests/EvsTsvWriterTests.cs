using System.Collections.Generic;
using Moq;
using SAUtils.DataStructures;
using SAUtils.TsvWriters;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.SaUtilsTests.TsvTests
{
	public class EvsTsvWriterTests
	{
		[Fact(Skip = "Bad output options")]
        public void OnlyDuplicatedAllelesAreRemoved()
        {
            var datasourceVersion = new DataSourceVersion("evs", "2017", 2017);
            var mockTsvWriter     = new Mock<SaTsvWriter>("", datasourceVersion, "", 1, "", "", false, false);
            

            var evs1 = new EvsItem("chr1", 100, ".", "A", "T", null, null, null, null, null);
            var evs2 = new EvsItem("chr1", 100, ".", "A", "1", null, null, null, null, null);
            var evs3 = new EvsItem("chr1", 100, ".", "A", "1", null, null, null, null, null);

            var evsItems = new List<EvsItem> { evs1, evs3, evs2 };
			using (var evsTsvWriter = new EvsTsvWriter(mockTsvWriter.Object))
			{
				evsTsvWriter.WritePosition(evsItems);
			}
			
            mockTsvWriter.Verify(
                x =>
                    x.AddEntry(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>(), It.IsAny<List<string>>()), Times.Once);
        }
    }
}