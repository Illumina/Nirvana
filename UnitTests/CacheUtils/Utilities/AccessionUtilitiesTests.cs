using System.IO;
using CacheUtils.Utilities;
using Xunit;

namespace UnitTests.CacheUtils.Utilities
{
    public sealed class AccessionUtilitiesTests
    {
        [Fact]
        public void GetMaxVersion_Dupl()
        {
            const string expectedId = "NM_004522.2_dupl6";
            const byte expectedVersion = 1;
            var observedResult = AccessionUtilities.GetMaxVersion("NM_004522.2_dupl6", 1);
            Assert.Equal(expectedId, observedResult.Id);
            Assert.Equal(expectedVersion, observedResult.Version);
        }

        [Fact]
        public void GetMaxVersion_IdVersionMax()
        {
            const string expectedId = "NM_004522";
            const byte expectedVersion = 2;
            var observedResult = AccessionUtilities.GetMaxVersion("NM_004522.2", 1);
            Assert.Equal(expectedId, observedResult.Id);
            Assert.Equal(expectedVersion, observedResult.Version);
        }

        [Fact]
        public void GetMaxVersion_SuppliedVersionMax()
        {
            const string expectedId = "NM_004522";
            const byte expectedVersion = 3;
            var observedResult = AccessionUtilities.GetMaxVersion("NM_004522.2", 3);
            Assert.Equal(expectedId, observedResult.Id);
            Assert.Equal(expectedVersion, observedResult.Version);
        }

        [Fact]
        public void GetAccessionNumber_ReturnNumber_RefSeq()
        {
            const int expectedResult = 4522;
            var observedResult = AccessionUtilities.GetAccessionNumber("NM_004522");
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void GetAccessionNumber_ReturnNumber_Ensembl()
        {
            const int expectedResult = 515242;
            var observedResult = AccessionUtilities.GetAccessionNumber("ENST00000515242");
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void GetAccessionNumber_ReturnMinusOne()
        {
            const int expectedResult = -1;
            var observedResult = AccessionUtilities.GetAccessionNumber(null);
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void GetAccessionNumber_ThrowException_IfUnderlineMissingRefSeq()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var observedResult = AccessionUtilities.GetAccessionNumber("NM004522");
            });
        }
    }
}
