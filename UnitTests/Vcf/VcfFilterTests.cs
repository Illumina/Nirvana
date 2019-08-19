using System.IO;
using System.Text;
using Genome;
using UnitTests.TestUtilities;
using Vcf;
using Xunit;

namespace UnitTests.Vcf
{
    public sealed class VcfFilterTests
    {

        [Fact]
        public void FastForward_ChangeReaderStateCorrectly()
        {
            var annotationInterval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 100, 200);

            var vcfFileter = new VcfFilter(annotationInterval);

            var firstLineInRange =
                "chr1\t100\t.\tC\tT\t165.00\tPASS\tSNVSB=-12.5;SNVHPOL=2\tGT:GQ:GQX:DP:DPF:AD\t0/1:119:35:25:0:8,17";

            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                {
                    writer.WriteLine("#Header line 1");
                    writer.WriteLine("#Header line 2");
                    writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO\tFORMAT\tMother");
                    writer.WriteLine("chr2\t150\t.\tG\tA\t5.00\tLowGQXHetSNP\tSNVSB=0.0;SNVHPOL=2\tGT:GQ:GQX:DP:DPF:AD\t0/1:3:1:1:0:0,1");
                    writer.WriteLine("chr1\t90\t.\tT\tC\t1.00\tLowGQXHetSNP\tSNVSB=0.0;SNVHPOL=2\tGT:GQ:GQX:DP:DPF:AD\t0/1:23:9:3:0:2,1");
                    writer.WriteLine("1\t95\t.\tA\tT\t2.00\tLowGQXHetSNP\tSNVSB=0.0;SNVHPOL=2\tGT:GQ:GQX:DP:DPF:AD\t0/1:23:9:3:0:2,1");
                    writer.WriteLine(firstLineInRange);
                    writer.WriteLine("chr1\t102\t.\tC\tA\t3.00\tLowGQXHetSNP\tSNVSB=0.0;SNVHPOL=5\tGT:GQ:GQX:DP:DPF:AD\t0/1:29:2:2:0:1,1");

                }

                ms.Position = 0;

                using (var reader = new StreamReader(ms))
                {
                    vcfFileter.FastForward(reader);
                    Assert.Equal(firstLineInRange, vcfFileter.BufferedLine);
                }
            }
        }

        [Fact]
        public void GetNextLine_NoBufferedLine_ReadNextLine()
        {
            var vcfFilter = new VcfFilter(null);
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("first line\nsecond line\n")))
            using (var reader = new StreamReader(ms))
            {
                string nextLine = vcfFilter.GetNextLine(reader);
                Assert.Equal("first line", nextLine);
            }
        }

        [Fact]
        public void GetNextLine_ReturnBufferedLine()
        {
            const string bufferedLine = "I am buffered";
            var vcfFilter = new VcfFilter(null) {BufferedLine = bufferedLine};

            string nextLine = vcfFilter.GetNextLine(null);
            Assert.Equal(bufferedLine, nextLine);

        }

        [Fact]
        public void PassedTheEnd_AsExpected()
        {
            var annotationInterval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 100, 200);
            var vcfFilter = new VcfFilter(annotationInterval);

            Assert.False(vcfFilter.PassedTheEnd(ChromosomeUtilities.Chr1, 150));
            Assert.False(vcfFilter.PassedTheEnd(ChromosomeUtilities.Chr1, 200));
            Assert.True(vcfFilter.PassedTheEnd(ChromosomeUtilities.Chr1, 201));
            Assert.True(vcfFilter.PassedTheEnd(ChromosomeUtilities.Chr2, 150));
        }
    }
}
