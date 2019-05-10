using System.IO;
using System.Linq;
using SAUtils.ExacScores;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class ExacGeneScoreTests
    {
        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("transcript\tgene\tchr\tn_exons\tcds_start\tcds_end\tbp\tmu_syn\tmu_mis\tmu_lof\tn_syn\tn_mis\tn_lof\texp_syn\texp_mis\texp_lof\tsyn_z\tmis_z\tlof_z\tpLI\tpRec\tpNull");
            writer.WriteLine("ENST00000379370.2\tAGRN\t1\t36\t955552\t990361\t6138\t5.46202903942e-05\t0.000102206705685\t4.84827940844e-06\t517\t829\t13\t445.215538582\t837.314601016\t54.4169577045\t-2.10908922369982\t0.140544378072657\t5.56161315048255\t0.17335234048116\t0.826647657926747\t1.59209302093807e-09");
            writer.WriteLine("ENST00000327044.6\tNOC2L\t1\t19\t880073\t894620\t2250\t1.41617661916e-05\t2.89834335593e-05\t2.06532877148e-06\t227\t394\t29\t154.668965626\t311.833215516\t28.8835117936\t-3.60555628871566\t-2.27589188516337\t-0.0214707226193315\t1.33038194561114e-19\t0.0025628417280481\t0.997437158271952");
            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetItems()
        {
            using (var reader = new ExacScoresParser(new StreamReader(GetStream())))
            {
                var items = reader.GetItems().ToList();

                Assert.Equal(2, items.Count);
                Assert.Equal("{\"pLi\":1.73e-1,\"pRec\":8.27e-1,\"pNull\":1.59e-9}", items[0].Value[0].GetJsonString());
            }
        }
    }
}