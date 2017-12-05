using System.Collections.Generic;
using System.IO;
using SAUtils.InputFileParsers.CustomInterval;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Sequence;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class CustomIntervalParserTests
    {
        private readonly IDictionary<string, IChromosome> _refChromDict;
        /// <summary>
        /// constructor
        /// </summary>
        public CustomIntervalParserTests()
        {
            _refChromDict = new Dictionary<string, IChromosome>();
        }

        [Fact]
        public void CustomIntervalTypeReaderTest()
        {
            var customFile = new FileInfo(Resources.TopPath("icslInterval.bed"));

            var customReader = new CustomIntervalParser(customFile,_refChromDict);

            // all items from this file should be of type cosmic.
            foreach (var customInterval in customReader.GetCustomIntervals())
            {
                Assert.Equal("IcslIntervals", customInterval.Type);
            }
        }

        [Fact]
        public void InforFieldsTest()
        {
            var customFile = new FileInfo(Resources.TopPath("icslInterval.bed"));

            var customReader = new CustomIntervalParser(customFile, _refChromDict);

            // all items from this file should be of type cosmic.
            var i = 0;
            foreach (var customInterval in customReader.GetCustomIntervals())
            {
                switch (i)
                {
                    case 0://checking the first item
                        Assert.Equal("chr1", customInterval.Chromosome.UcscName);
                        Assert.Equal(69091, customInterval.Start);
                        Assert.Equal(70008, customInterval.End);
                        Assert.Equal("OR4F5", customInterval.StringValues["gene"]);
                        Assert.Equal("0.0", customInterval.NonStringValues["score"]);
                        Assert.Equal("3.60208899915", customInterval.NonStringValues["exacScore"]);
                        Assert.Equal("Some_evidence_of_constraint", customInterval.StringValues["assesment"]);
                        break;
                    case 1:
                        Assert.Equal("chr1", customInterval.Chromosome.UcscName);
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
