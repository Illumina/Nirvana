using UnitTests.Utilities;
using VariantAnnotation.FileHandling.Phylop;
using Xunit;

namespace UnitTests.FileHandling
{
    public sealed class PhylopGetScoreTests
    {
        #region members

        private readonly PhylopReader _npdReader;

        #endregion

        //constructor
        public PhylopGetScoreTests()
        {
            _npdReader = new PhylopReader(ResourceUtilities.GetReadStream(Resources.TopPath("chr1_10918_150000.npd")));
        }

        [Fact]
        public void BeforeFirstPosition()
        {
            var obtainedValue = _npdReader.GetScore(10910);
            Assert.Null(obtainedValue);
        }

        [Fact]
        public void FirstPosition()
        {
            const string desiredValue = "0.064";

            var obtainedValue = _npdReader.GetScore(10918);

            Assert.Equal(desiredValue, obtainedValue);
        }

        [Fact]
        public void LastPositionOfFirstInterval()
        {
            const string desiredValue = "0.179";

            var obtainedValue = _npdReader.GetScore(34041);

            Assert.Equal(desiredValue, obtainedValue);
        }

        [Fact]
        public void BetweenFirstAndSecondInterval()
        {
            var obtainedValue = _npdReader.GetScore(34044);

            Assert.Null(obtainedValue);

        }

        [Fact]
        public void InsideInterval()
        {
            const string desiredValue = "0.058";

            var obtainedValue = _npdReader.GetScore(84285); // this is in the 5th interval

            Assert.Equal(desiredValue, obtainedValue);
        }

        [Fact]
        public void LastPosition()
        {
            var obtainedValue = _npdReader.GetScore(168044);
            Assert.Equal("0.301", obtainedValue);
        }

        [Fact]
        public void PastLastInterval()
        {
            var obtainedValue = _npdReader.GetScore(168045); //this should be past the last position of our tiny npd file
            Assert.Null(obtainedValue);
        }
    }
}
