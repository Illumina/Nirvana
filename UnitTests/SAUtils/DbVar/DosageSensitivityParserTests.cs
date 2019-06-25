using System.IO;
using System.Linq;
using SAUtils.dbVar;
using Xunit;

namespace UnitTests.SAUtils.DbVar
{
    public class DosageSensitivityParserTests
    {
        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("#ClinGen Gene Curation Results");
            writer.WriteLine("#07 May,2019");
            writer.WriteLine("#Genomic Locations are reported on GRCh37 (hg19): GCF_000001405.13");
            writer.WriteLine("#Gene Symbol\tGene ID\tcytoBand\tGenomic Location\tHaploinsufficiency Score\tHaploinsufficiency Description\tHaploinsufficiency PMID1\tHaploinsufficiency PMID2\tHaploinsufficiency PMID3\tTriplosensitivity Score\tTriplosensitivity Description\tTriplosensitivity PMID1\tTriplosensitivity PMID2\tTriplosensitivity PMID3\tDate Last Evaluated\tLoss phenotype OMIM ID\tTriplosensitive phenotype OMIM ID");
            
            writer.WriteLine("A4GALT\t53947\t22q13.2\tchr22:43088121-43117307\t30\tGene associated with autosomal recessive phenotype\t\t\t\t0\tNo evidence available\t\t\t\t2014-12-11\t111400\t");
            writer.WriteLine("AAGAB\t79719\t15q23\tchr15:67493013-67547536\t3\tSufficient evidence for dosage pathogenicity\t23064416\t23000146\t\t0\tNo evidence available\t\t\t\t2013-02-28\t148600\t");
            writer.WriteLine("AARS\t16\t16q22.1\tchr16:70286297-70323412\t0\tNo evidence available\t\t\t\t0\tNo evidence available\t\t\t\t2018-01-11\t\t");
            writer.WriteLine("AARS2\t57505\t6p21.1\tchr6:44266463-44281063\t30\tGene associated with autosomal recessive phenotype\t\t\t\tNot yet evaluated\tNot yet evaluated\t\t\t\t2016-08-22\t\t");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void StandardParsing()
        {
            using (var dbVarReader = new DosageSensitivityParser(GetStream()))
            {
                var items = dbVarReader.GetItems();

                Assert.Equal(4, items.Count);
                Assert.Equal("{\"haploinsufficiency\":\"gene associated with autosomal recessive phenotype\",\"triplosensitivity\":\"no evidence to suggest that dosage sensitivity is associated with clinical phenotype\"}", items["A4GALT"][0].GetJsonString());
                Assert.Equal("{\"haploinsufficiency\":\"sufficient evidence suggesting dosage sensitivity is associated with clinical phenotype\",\"triplosensitivity\":\"no evidence to suggest that dosage sensitivity is associated with clinical phenotype\"}", items["AAGAB"][0].GetJsonString());
                Assert.Equal("{\"haploinsufficiency\":\"no evidence to suggest that dosage sensitivity is associated with clinical phenotype\",\"triplosensitivity\":\"no evidence to suggest that dosage sensitivity is associated with clinical phenotype\"}", items["AARS"][0].GetJsonString());
                Assert.Equal("{\"haploinsufficiency\":\"gene associated with autosomal recessive phenotype\"}", items["AARS2"][0].GetJsonString());
            }
        }
    }
}