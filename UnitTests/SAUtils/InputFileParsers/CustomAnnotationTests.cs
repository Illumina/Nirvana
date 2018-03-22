using System.Collections.Generic;
using System.IO;
using SAUtils.InputFileParsers.CustomAnnotation;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Sequence;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    [Collection("ChromosomeRenamer")]
    public sealed class CustomAnnotationTests
    {
        private readonly IDictionary<string,IChromosome> _refChromDict;

        /// <summary>
        /// constructor
        /// </summary>
        public CustomAnnotationTests()
        {
            _refChromDict = new Dictionary<string, IChromosome>();
        }

        [Fact]
        public void BasicReaderTest()
        {
            var customFile = new FileInfo(Resources.TopPath("customCosmic.vcf"));

            var customReader = new CustomAnnotationReader(customFile, _refChromDict);

            // all items from this file should be of type cosmic.
            foreach (var customItem in customReader.GetCustomItems())
            {
                Assert.Equal("cosmic", customItem.AnnotationType);
            }
        }

        [Fact]
        public void StringFieldsTest()
        {
            var customFile = new FileInfo(Resources.TopPath("customCosmic.vcf"));

            var customReader = new CustomAnnotationReader(customFile, _refChromDict);

            // all items from this file should be of type cosmic.
            var i = 0;
            foreach (var customItem in customReader.GetCustomItems())
            {
                switch (i)
                {
                    case 0://checking the first item
                        Assert.Equal("COSM3677745", customItem.Id);
                        Assert.Equal("OR4F5", customItem.StringFields["gene"]);
                        Assert.Equal("+", customItem.StringFields["strand"]);
                        Assert.Equal("c.134A>C", customItem.StringFields["cds"]);
                        Assert.Equal("p.D45A", customItem.StringFields["aminoAcid"]);
                        Assert.Equal("1", customItem.StringFields["count"]);
                        break;
                    case 1:
                        Assert.Equal("COSM911918", customItem.Id);
                        Assert.Equal("OR4F5", customItem.StringFields["gene"]);
                        Assert.Equal("+", customItem.StringFields["strand"]);
                        Assert.Equal("c.255C>A", customItem.StringFields["cds"]);
                        Assert.Equal("p.I85I", customItem.StringFields["aminoAcid"]);
                        Assert.Equal("1", customItem.StringFields["count"]);
                        Assert.Equal("inExomeTarget", customItem.BooleanFields[0]);
                        break;
                }

                i++;
            }
        }
    }
}
