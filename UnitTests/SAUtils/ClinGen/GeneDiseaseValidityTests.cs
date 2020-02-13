using System.Collections.Generic;
using System.IO;
using SAUtils.ClinGen;
using Xunit;

namespace UnitTests.SAUtils.ClinGen
{
    public class GeneDiseaseValidityTests
    {
        private Stream GetGeneValidityStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("CLINGEN GENE VALIDITY CURATIONS\t\t\t\t");
            writer.WriteLine("FILE CREATED: 2019-12-02\t\t\t\t");
            writer.WriteLine("WEBPAGE: https://search.clinicalgenome.org/kb/gene-validity \t\t\t\t");
            writer.WriteLine("+++++++++++\t++++++++++++++\t+++++++++++++\t++++++++++++++++++\t+++++++++\t++++++++++++++\t+++++++++++++\t+++++++++++++++++++");
            writer.WriteLine("GENE SYMBOL\tGENE ID (HGNC)\tDISEASE LABEL\tDISEASE ID (MONDO)\tSOP\tCLASSIFICATION\tONLINE REPORT\tCLASSIFICATION DATE");
            writer.WriteLine("+++++++++++\t++++++++++++++\t+++++++++++++\t++++++++++++++++++\t+++++++++\t++++++++++++++\t+++++++++++++\t+++++++++++++++++++");
            writer.WriteLine("A2ML1\tHGNC:23336\tNoonan syndrome with multiple lentigines\tMONDO_0007893\tSOP5\tNo Reported Evidence\thttps://search.clinicalgenome.org/kb/gene-validity/59b87033-dd91-4f1e-aec1-c9b1f5124b16--2018-06-07T14:37:47\t2018-06-07T14:37:47.175Z");
            writer.WriteLine("A2ML1\tHGNC:23336\tcardiofaciocutaneous syndrome\tMONDO_0015280\tSOP5\tNo Reported Evidence\thttps://search.clinicalgenome.org/kb/gene-validity/fc3c41d8-8497-489b-a350-c9e30016bc6a--2018-06-07T14:31:03\t2018-06-07T14:31:03.696Z");
            writer.WriteLine("A2ML1\tHGNC:23336\tCostello syndrome\tMONDO_0009026\tSOP5\tNo Reported Evidence\thttps://search.clinicalgenome.org/kb/gene-validity/ea72ba8d-cf62-44bc-86be-da64e3848eba--2018-06-07T14:34:05\t2018-06-07T14:34:05.324Z");
            writer.WriteLine("AARS\tHGNC:20\tundetermined early-onset epileptic encephalopathy\tMONDO_0018614\tSOP6\tLimited\thttps://search.clinicalgenome.org/kb/gene-validity/ac62fe65-ee56-4146-9fe4-00dc1db2d958--2018-11-20T17:00:00\t2018-11-20T17:00:00.000Z");
            writer.WriteLine("AASS\tHGNC:17366\thyperlysinemia (disease)\tMONDO_0009388\tSOP6\tModerate\thttps://search.clinicalgenome.org/kb/gene-validity/92e04f9e-f03e-4295-baac-e9fb6b48a258--2019-11-08T17:00:00\t2019-11-08T17:00:00.000Z");
            writer.WriteLine("ABCC9\tHGNC:60\thypertrichotic osteochondrodysplasia Cantu type\tMONDO_0009406\tSOP4\tDefinitive\thttps://search.clinicalgenome.org/kb/gene-validity/10028\t2017-09-27T00:00:00");
            //duplicate item
            writer.WriteLine("ABCC9\tHGNC:60\thypertrichotic osteochondrodysplasia Cantu type\tMONDO_0009406\tSOP4\tDefinitive\thttps://search.clinicalgenome.org/kb/gene-validity/10028\t2017-10-27T00:00:00");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }
        private Dictionary<int, string> GetIdToSymbols()
        {
            return new Dictionary<int, string>
            {
                { 23336,"A2ML1" },
                { 20, "AARS"},
                { 60, "ABCC9" }
            };
        }

        [Fact]
        public void ParserTest()
        {
            var parser = new GeneDiseaseValidityParser(GetGeneValidityStream(), GetIdToSymbols());

            var items = parser.GetItems();
            Assert.Equal(3, items.Count);

            var firstGene = items["A2ML1"];
            Assert.Equal(3, firstGene.Count);

            Assert.Equal("{\"diseaseId\":\"MONDO_0007893\",\"disease\":\"Noonan syndrome with multiple lentigines\",\"classification\":\"no reported evidence\",\"classificationDate\":\"2018-06-07\"}", firstGene[0].GetJsonString());

            var thirdGene = items["ABCC9"];
            Assert.Single(thirdGene);
            Assert.Equal("{\"diseaseId\":\"MONDO_0009406\",\"disease\":\"hypertrichotic osteochondrodysplasia Cantu type\",\"classification\":\"definitive\",\"classificationDate\":\"2017-10-27\"}", thirdGene[0].GetJsonString());
        }
        
    }
}