using System.Linq;
using IO;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.ClinVar;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class ClinvarVariationParserTests
    {
        [Fact]
        public void InterpretedRecordsTest()
        {
            using (var reader = new ClinVarVariationReader(FileUtilities.GetReadStream(Resources.VcvXmlFiles("TwoRecords.xml"))))
            {
                var items = reader.GetItems().ToArray();
                Assert.Equal(2, items.Length);
                Assert.Equal(79, items[0].VariantId);
                Assert.Equal(ClinVarCommon.ReviewStatus.no_criteria, items[0].ReviewStatus);
                Assert.Equal(new []{"pathogenic"}, items[0].Significances);
                
                Assert.Equal(86, items[1].VariantId);
            }
        }
        
        [Fact]
        public void IncludedRecordTest()
        {
            using (var reader = new ClinVarVariationReader(FileUtilities.GetReadStream(Resources.VcvXmlFiles("VCV000431749.xml"))))
            {
                var items = reader.GetItems().ToArray();
                Assert.Equal(ClinVarCommon.ReviewStatus.no_interpretation_single, items[0].ReviewStatus);
                Assert.Equal(new []{"no interpretation for the single variant"}, items[0].Significances);

            }
        }
    }
}