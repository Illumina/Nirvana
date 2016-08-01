using UnitTests.Fixtures;
using VariantAnnotation.FileHandling.Phylop;
using Xunit;

namespace UnitTests.FileHandling
{
    public sealed class PhylopGetScoreTests : IClassFixture<PhylopDatabaseFixture>
    {
        #region memebers

        private readonly PhylopReader _npdReader;


        #endregion

        //constructor
        public PhylopGetScoreTests(PhylopDatabaseFixture context)
        {
            _npdReader = context.NpdDatabase;
        }

        [Fact]
        public void BeforeFirstPosition()
        {
            string obtainedValue = _npdReader.GetScore(10910);
            Assert.Null(obtainedValue);

        }

        [Fact]
        public void FirstPosition()
        {
            const string desiredValue = "0.064";

            string obtainedValue = _npdReader.GetScore(10918);

            Assert.Equal(desiredValue, obtainedValue);
        }

        [Fact]
        public void LastPositionOfFirstInterval()
        {
            const string desiredValue = "0.179";

            string obtainedValue = _npdReader.GetScore(34041);

            Assert.Equal(desiredValue, obtainedValue);
        }

        [Fact]
        public void BetweenFirstAndSecondInterval()
        {
            string obtainedValue = _npdReader.GetScore(34044);

            Assert.Null(obtainedValue);

        }

        [Fact]
        public void InsideInterval()
        {
            const string desiredValue = "0.058";

            string obtainedValue = _npdReader.GetScore(84285); // this is in the 5th interval

            Assert.Equal(desiredValue, obtainedValue);
        }

        [Fact]
        public void LastPosition()
        {
            string obtainedValue = _npdReader.GetScore(168044);
            Assert.Equal("0.301", obtainedValue);
        }

        [Fact]
        public void PastLastInterval()
        {
            string obtainedValue = _npdReader.GetScore(168045); //this should be past the last position of our tiny npd file
            Assert.Null(obtainedValue);

        }


    }
}
