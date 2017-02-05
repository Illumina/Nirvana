using System.IO;
using SAUtils.InputFileParsers.CustomInterval;
using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.FileHandling.SaFileParsers
{
    [Collection("ChromosomeRenamer")]
    public sealed class CustomIntervalParserTests
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public CustomIntervalParserTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        [Fact]
        public void CustomIntervalTypeReaderTest()
        {
            var customFile = new FileInfo(Resources.TopPath("icslInterval.bed"));

            var customReader = new CustomIntervalParser(customFile, _renamer);

            // all items from this file should be of type cosmic.
            foreach (var customInterval in customReader)
            {
                Assert.Equal("IcslIntervals", customInterval.Type);
            }
        }

        [Fact]
        public void InforFieldsTest()
        {
            var customFile = new FileInfo(Resources.TopPath("icslInterval.bed"));

            var customReader = new CustomIntervalParser(customFile, _renamer);

            // all items from this file should be of type cosmic.
            var i = 0;
            foreach (var customInterval in customReader)
            {
                switch (i)
                {
                    case 0://checking the first item
                        Assert.Equal("chr1", customInterval.ReferenceName);
                        Assert.Equal(69091, customInterval.Start);
                        Assert.Equal(70008, customInterval.End);
                        Assert.Equal("OR4F5", customInterval.StringValues["gene"]);
                        Assert.Equal("0.0", customInterval.NonStringValues["score"]);
                        Assert.Equal("3.60208899915", customInterval.NonStringValues["exacScore"]);
                        Assert.Equal("Some_evidence_of_constraint", customInterval.StringValues["assesment"]);
                        break;
                    case 1:
                        Assert.Equal("chr1", customInterval.ReferenceName);
                        Assert.Equal(861121, customInterval.Start);
                        Assert.Equal(879582, customInterval.End);
                        Assert.Equal("SAMD11", customInterval.StringValues["gene"]);
                        Assert.Equal("0.997960686608", customInterval.NonStringValues["score"]);
                        Assert.Equal("-3.7861959419", customInterval.NonStringValues["exacScore"]);
                        Assert.Equal("Minimal_evidence_of_constraint", customInterval.StringValues["assesment"]);
                        break;
                }

                i++;
            }
        }
    }
}
