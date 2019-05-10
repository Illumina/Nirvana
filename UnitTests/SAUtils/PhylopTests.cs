using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using SAUtils.InputFileParsers;
using SAUtils.PhyloP;
using UnitTests.TestUtilities;
using VariantAnnotation.PhyloP;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils
{
    public sealed class PhylopTests
    {
        private static readonly IChromosome Chrom1 = new Chromosome("chr1", "1", 1);
        private static readonly IChromosome Chrom2 = new Chromosome("chr2", "2", 2);

        private readonly Dictionary<string, IChromosome> _chromDict = new Dictionary<string, IChromosome>
        {
            { "chr1", Chrom1},
            { "chr2", Chrom2}
        };
        [Fact]
        public void LoopbackTest()
        {
            var wigFixFile = Resources.TopPath("mini.WigFix");
            var version = new DataSourceVersion("phylop", "0", DateTime.Now.Ticks, "unit test");
            
            using(var reader      = new PhylopParser(FileUtilities.GetReadStream(wigFixFile),GenomeAssembly.GRCh37, _chromDict))
            using (var npdStream  = new MemoryStream())
            using(var indexStream = new MemoryStream())
            using (var npdWriter  = new NpdWriter(npdStream, indexStream, version, GenomeAssembly.GRCh37, SaCommon.PhylopTag, SaCommon.SchemaVersion))
            {
                npdWriter.Write(reader.GetItems());

                npdStream.Position = 0;
                indexStream.Position = 0;

                using (var phylopReader = new NpdReader(npdStream, indexStream))
                {
                    Assert.Equal(0.1, phylopReader.GetAnnotation(Chrom1, 100));//first position of first block
                    Assert.Equal(0.1, phylopReader.GetAnnotation(Chrom1, 101));// second position
                    Assert.Equal(0.1, phylopReader.GetAnnotation(Chrom1, 120));// some internal position
                    Assert.Equal(0.1, phylopReader.GetAnnotation(Chrom1, 130));//last position of first block

                    //moving on to the next block: should cause reloading from file
                    Assert.Equal(0.1, phylopReader.GetAnnotation(Chrom1, 175));//first position of second block
                    Assert.Equal(-2.1, phylopReader.GetAnnotation(Chrom1, 182));// some negative value

                    //chrom 2
                    Assert.Null(phylopReader.GetAnnotation(Chrom2, 400));//values past the last phylop positions should return null
                }
            }
            
        }

    }
}