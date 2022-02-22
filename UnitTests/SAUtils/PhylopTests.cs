using System;
using System.IO;
using Genome;
using IO;
using SAUtils.InputFileParsers;
using SAUtils.PhyloP;
using UnitTests.TestUtilities;
using VariantAnnotation.PhyloP;
using VariantAnnotation.SA;
using Versioning;
using Xunit;

namespace UnitTests.SAUtils
{
    public sealed class PhylopTests
    {
        [Fact]
        public void LoopbackTest()
        {
            var wigFixFile = Resources.TopPath("mini.WigFix");
            var version = new DataSourceVersion("phylop", "unit test", "0", DateTime.Now.Ticks);
            
            using(var reader      = new PhylopParser(FileUtilities.GetReadStream(wigFixFile),GenomeAssembly.GRCh37, ChromosomeUtilities.RefNameToChromosome))
            using (var npdStream  = new MemoryStream())
            using(var indexStream = new MemoryStream())
            using (var npdWriter  = new NpdWriter(npdStream, indexStream, version, GenomeAssembly.GRCh37, SaCommon.PhylopTag, SaCommon.SchemaVersion))
            {
                npdWriter.Write(reader.GetItems());

                npdStream.Position = 0;
                indexStream.Position = 0;

                using (var phylopReader = new NpdReader(npdStream, indexStream))
                {
                    Assert.Equal(0.1, phylopReader.GetAnnotation(ChromosomeUtilities.Chr1, 100));//first position of first block
                    Assert.Equal(0.1, phylopReader.GetAnnotation(ChromosomeUtilities.Chr1, 101));// second position
                    Assert.Equal(0.1, phylopReader.GetAnnotation(ChromosomeUtilities.Chr1, 120));// some internal position
                    Assert.Equal(0.1, phylopReader.GetAnnotation(ChromosomeUtilities.Chr1, 130));//last position of first block

                    //moving on to the next block: should cause reloading from file
                    Assert.Equal(0.1, phylopReader.GetAnnotation(ChromosomeUtilities.Chr1, 175));//first position of second block
                    Assert.Equal(-2.1, phylopReader.GetAnnotation(ChromosomeUtilities.Chr1, 182));// some negative value

                    //chrom 2
                    Assert.Null(phylopReader.GetAnnotation(ChromosomeUtilities.Chr2, 400));//values past the last phylop positions should return null
                }
            }
            
        }

    }
}