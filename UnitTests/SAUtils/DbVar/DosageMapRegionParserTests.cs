using System.IO;
using System.Linq;
using SAUtils.ClinGen;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.DbVar
{
    public sealed class DosageMapRegionParserTests
    { 
        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("#ClinGen Region Curation Results");
            writer.WriteLine("#07 May,2019");
            writer.WriteLine("#Genomic Locations are reported on GRCh37 (hg19): GCF_000001405.13");
            writer.WriteLine("#https://www.ncbi.nlm.nih.gov/projects/dbvar/clingen");
            writer.WriteLine("#to create link: https://www.ncbi.nlm.nih.gov/projects/dbvar/clingen/clingen_region.cgi?id=key");
            writer.WriteLine("#ISCA ID\tISCA Region Name\tcytoBand\tGenomic Location\tHaploinsufficiency Score\tHaploinsufficiency Description\tHaploinsufficiency PMID1\tHaploinsufficiency PMID2\tHaploinsufficiency PMID3\tTriplosensitivity Score\tTriplosensitivity Description\tTriplosensitivity PMID1\tTriplosensitivity PMID2\tTriplosensitivity PMID3\tDate Last Evaluated\tLoss phenotype OMIM ID\tTriplosensitive phenotype OMIM ID");
            writer.WriteLine("ISCA-46299\tXp11.22 region (includes HUWE1)\tXp11.22\tchrX:53363456-53793054\t0\tNo evidence available\t\t\t\t3\tSufficient evidence for dosage pathogenicity\t22840365\t20655035\t26692240\t2018-11-19");
            writer.WriteLine("ISCA-46295\t15q13.3 recurrent region (D-CHRNA7 to BP5) (includes CHRNA7 and OTUD7A)\t15q13.3\tchr15:32019621-32445405\t3\tSufficient evidence for dosage pathogenicity\t19898479\t20236110\t22775350\t40\tDosage sensitivity unlikely\t26968334\t22420048\t\t2018-05-10");
            writer.WriteLine("ISCA-46291\t7q11.23 recurrent distal region (includes HIP1, YWHAG)\t7q11.23\tchr7:75158048-76063176\t2\tSome evidence for dosage pathogenicity\t21109226\t16971481\t\t1\tLittle evidence for dosage pathogenicity\t21109226\t27867344\t\t2018-12-31");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }
        
        [Fact]
        public void StandardParsing()
        {
            using (var dosageMapRegionParser = new DosageMapRegionParser(GetStream(), ChromosomeUtilities.RefNameToChromosome))
            {
                var items = dosageMapRegionParser.GetItems().OrderBy(x => x.Chromosome.Index).ToArray();

                Assert.Equal(3, items.Length);
                Assert.Equal("\"chromosome\":\"7\",\"begin\":75158048,\"end\":76063176,\"haploinsufficiency\":\"emerging evidence suggesting dosage sensitivity is associated with clinical phenotype\",\"triplosensitivity\":\"little evidence suggesting dosage sensitivity is associated with clinical phenotype\"", items[0].GetJsonString());
                Assert.Equal("\"chromosome\":\"15\",\"begin\":32019621,\"end\":32445405,\"haploinsufficiency\":\"sufficient evidence suggesting dosage sensitivity is associated with clinical phenotype\",\"triplosensitivity\":\"dosage sensitivity unlikely\"", items[1].GetJsonString());
                Assert.Equal("\"chromosome\":\"X\",\"begin\":53363456,\"end\":53793054,\"haploinsufficiency\":\"no evidence to suggest that dosage sensitivity is associated with clinical phenotype\",\"triplosensitivity\":\"sufficient evidence suggesting dosage sensitivity is associated with clinical phenotype\"", items[2].GetJsonString());
            }
        }
    }
}