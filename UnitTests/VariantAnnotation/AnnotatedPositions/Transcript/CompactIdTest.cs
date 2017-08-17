using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public class CompactIdTest
    {
        [Fact]

        public void Covert_Test()
        {
            var compactId_checkEmpty = new CompactId();
            Assert.True(compactId_checkEmpty.IsEmpty);


            // empty            
            var compactId_empty = CompactId.Convert("");
            Assert.Equal(IdType.Unknown, compactId_empty.Id);
            Assert.Equal(0, compactId_empty.Info);
            Assert.Equal("", compactId_empty.ToString());

            // ENSR

            var compactId_ENSR = CompactId.Convert("ENSR00001576074");
            Assert.Equal(IdType.EnsemblRegulatory, compactId_ENSR.Id);
            Assert.Equal(25217195, compactId_ENSR.Info);
            Assert.Equal("ENSR00001576074", compactId_ENSR.ToString());

            // ENSESTG
            var compactId_ENSESTG = CompactId.Convert("ENSESTG030567.1");
            Assert.Equal(IdType.EnsemblEstGene, compactId_ENSESTG.Id);
            Assert.Equal(489078, compactId_ENSESTG.Info);
            Assert.Equal("ENSESTG030567",compactId_ENSESTG.ToString());

            // CCDS
            var compactId_CCDS = CompactId.Convert("CCDS30555.1");
            Assert.Equal(IdType.Ccds, compactId_CCDS.Id);
            Assert.Equal(488885, compactId_CCDS.Info);
            Assert.Equal("CCDS30555", compactId_CCDS.ToString());

            // NR
            var compactId_NR = CompactId.Convert("NR_074509.1");
            Assert.Equal(IdType.RefSeqNonCodingRNA, compactId_NR.Id);
            Assert.Equal(1192150, compactId_NR.Info);
            Assert.Equal("NR_074509", compactId_NR.ToString());

            // NM
            var compactId_NM = CompactId.Convert("NM_001029885.1");
            Assert.Equal(IdType.RefSeqMessengerRNA, compactId_NM.Id);
            Assert.Equal(16478169, compactId_NM.Info);
            Assert.Equal("NM_001029885", compactId_NM.ToString());

            // NP
            var compactId_NP = CompactId.Convert("NP_001025056.1");
            Assert.Equal(IdType.RefSeqProtein, compactId_NP.Id);
            Assert.Equal(16400905, compactId_NP.Info);
            Assert.Equal("NP_001025056", compactId_NP.ToString());

            // XR
            var compactId_XR = CompactId.Convert("XR_246629.1");
            Assert.Equal(IdType.RefSeqPredictedNonCodingRNA, compactId_XR.Id);
            Assert.Equal(3946070, compactId_XR.Info);
            Assert.Equal("XR_246629", compactId_XR.ToString());

            // XM
            var compactId_XM = CompactId.Convert("XM_005244723.1");
            Assert.Equal(IdType.RefSeqPredictedMessengerRNA, compactId_XM.Id);
            Assert.Equal(83915577, compactId_XM.Info);
            Assert.Equal("XM_005244723", compactId_XM.ToString());

            // XP
            var compactId_XP = CompactId.Convert("XP_005244780.1");
            Assert.Equal(IdType.RefSeqPredictedProtein, compactId_XP.Id);
            Assert.Equal(83916489, compactId_XP.Info);
            Assert.Equal("XP_005244780", compactId_XP.ToString());

            // UNKNOWN
            var compactId_Empty = CompactId.Convert("ABC_005244780.1");
            Assert.Equal(IdType.Unknown, compactId_Empty.Id);
            Assert.Equal(0, compactId_Empty.Info);

            // Only numbers
            var compactId_onlyNum = CompactId.Convert("1234567");
            Assert.Equal(IdType.OnlyNumbers, compactId_onlyNum.Id);
            Assert.Equal(19753079, compactId_onlyNum.Info);
            Assert.Equal("1234567",compactId_onlyNum.ToString());
        }


    }
}