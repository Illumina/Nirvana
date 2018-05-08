using System;
using System.IO;
using Genome;
using IO;
using UnitTests.TestUtilities;
using VariantAnnotation.PhyloP;
using VariantAnnotation.Providers;
using Xunit;

namespace UnitTests.VariantAnnotation.PhyloP
{
    public sealed class PhylopReaderTests
    {
        private readonly PhylopReader _phylopReader;

        public PhylopReaderTests()
        {
            // TODO: Fix fragile constructor
            _phylopReader = new PhylopReader(ResourceUtilities.GetReadStream(Resources.TopPath("chr1_10918_150000.npd")));
        }

        [Fact]
        public void LoopbackTest()
        {
            var wigFixFile = new FileInfo(Resources.TopPath("mini.WigFix"));
            var version = new DataSourceVersion("phylop", "0", DateTime.Now.Ticks, "unit test");

            var phylopWriter = new PhylopWriter(wigFixFile.FullName, version, GenomeAssembly.Unknown, Path.GetTempPath(), 50);

            using (phylopWriter)
            {
                phylopWriter.ExtractPhylopScores();
            }

            var phylopReader = new PhylopReader(FileUtilities.GetReadStream(Path.GetTempPath() + Path.DirectorySeparatorChar + "chr1.npd"));

            using (phylopReader)
            {
                Assert.Equal(0.064, phylopReader.GetScore(100));//first position of first block
                Assert.Equal(0.058, phylopReader.GetScore(101));// second position
                Assert.Equal(0.064, phylopReader.GetScore(120));// some internal position
                Assert.Equal(0.058, phylopReader.GetScore(130));//last position of first block

                //moving on to the next block: should cause reloading from file
                Assert.Equal(0.064, phylopReader.GetScore(175));//first position of first block
                Assert.Equal(-2.088, phylopReader.GetScore(182));// some negative value
            }

            File.Delete(Path.GetTempPath() + Path.DirectorySeparatorChar + "chr1.npd");
        }

        [Fact]
        public void GetScore_BeforeFirstPosition()
        {
            var obtainedValue = _phylopReader.GetScore(10910);
            Assert.Null(obtainedValue);
        }

        [Fact]
        public void GetScore_FirstPosition()
        {
            const double desiredValue = 0.064;
            var obtainedValue = _phylopReader.GetScore(10918);
            Assert.Equal(desiredValue, obtainedValue);
        }

        [Fact]
        public void GetScore_LastPositionOfFirstInterval()
        {
            const double desiredValue = 0.179;
            var obtainedValue = _phylopReader.GetScore(34041);
            Assert.Equal(desiredValue, obtainedValue);
        }

        [Fact]
        public void GetScore_BetweenFirstAndSecondInterval()
        {
            var obtainedValue = _phylopReader.GetScore(34044);
            Assert.Null(obtainedValue);
        }

        [Fact]
        public void GetScore_InsideInterval()
        {
            const double desiredValue = 0.058;
            var obtainedValue = _phylopReader.GetScore(84285); // this is in the 5th interval
            Assert.Equal(desiredValue, obtainedValue);
        }

        [Fact]
        public void GetScore_LastPosition()
        {
            var obtainedValue = _phylopReader.GetScore(168044);
            Assert.Equal(0.301, obtainedValue);
        }

        [Fact]
        public void GetScore_PastLastInterval()
        {
            var obtainedValue = _phylopReader.GetScore(168045); //this should be past the last position of our tiny npd file
            Assert.Null(obtainedValue);
        }
    }
}
