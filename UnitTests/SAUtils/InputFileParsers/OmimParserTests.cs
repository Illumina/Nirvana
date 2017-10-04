//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
////using SAUtils.InputFileParsers.Omim;
//using UnitTests.TestUtilities;
//using Xunit;

//namespace UnitTests.SaUtilsTests.InputFileParsers
//{
//    public sealed class OmimParserTests
//    {
//        [Fact]
//        void TestOmimParser()
//        {
//            var expectedOmimEntries = new List<string>
//            {
//                "{\"mimNumber\":601405,\"description\":\"Chymotrypsin\",\"phenotypes\":[{\"mimNumber\":167800,\"phenotype\":\"Pancreatitis, chronic, susceptibility to\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Autosomal dominant\"],\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"}]}",
//                "{\"mimNumber\":606928,\"description\":\"Bone mineral density QTL 3\",\"phenotypes\":[{\"mimNumber\":606928,\"phenotype\":\"Bone mineral density QTL 3\",\"mapping\":\"disease phenotype itself was mapped\",\"comments\":\"nondiseases\"}]}",
//                "{\"mimNumber\":103730,\"description\":\"Alcohol dehydrogenase IC (class I), gamma polypeptide\",\"phenotypes\":[{\"mimNumber\":103780,\"phenotype\":\"Alcohol dependence, protection against\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Multifactorial\"],\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"},{\"mimNumber\":168600,\"phenotype\":\"Parkinson disease, susceptibility to\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Isolated cases\",\"Multifactorial\"],\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"}]}",
//                "{\"mimNumber\":162230,\"description\":\"Neurofilament, heavy polypeptide\",\"phenotypes\":[{\"mimNumber\":105400,\"phenotype\":\"Amyotrophic lateral sclerosis, susceptibility to\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Autosomal recessive\",\"Autosomal dominant\"],\"comments\":\"unconfirmed or possibly spurious mapping\"},{\"mimNumber\":616924,\"phenotype\":\"Charcot-Marie-Tooth disease, axonal, type 2CC\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Autosomal dominant\"]}]}",
//                "{\"mimNumber\":615410,\"description\":\"Melanocortin 2 receptor accessory protein 2\",\"phenotypes\":[{\"mimNumber\":615457,\"phenotype\":\"Obesity, susceptibility to, BMIQ18\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Autosomal dominant\"],\"comments\":\"unconfirmed or possibly spurious mapping\"}]}",
//                "{\"mimNumber\":603517,\"description\":\"B-cell leukemia/lymphoma 10\",\"phenotypes\":[{\"mimNumber\":616098,\"phenotype\":\"Immunodeficiency 37\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Autosomal recessive\"],\"comments\":\"unconfirmed or possibly spurious mapping\"},{\"mimNumber\":137245,\"phenotype\":\"Lymphoma, MALT, somatic\",\"mapping\":\"molecular basis of the disorder is known\"},{\"mimNumber\":605027,\"phenotype\":\"Lymphoma, follicular, somatic\",\"mapping\":\"molecular basis of the disorder is known\",\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"},{\"mimNumber\":273300,\"phenotype\":\"Male germ cell tumor, somatic\",\"mapping\":\"molecular basis of the disorder is known\",\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"},{\"mimNumber\":156240,\"phenotype\":\"Mesothelioma, somatic\",\"mapping\":\"molecular basis of the disorder is known\",\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"},{\"phenotype\":\"Sezary syndrome, somatic\",\"mapping\":\"molecular basis of the disorder is known\",\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"}]}"
//            };
//            var omimFile = Resources.TopPath("testOmim.txt");
//            var omimReader = new OmimReader(new FileInfo(omimFile));
//            var observedOmimEntries = omimReader.ToList().Select(x => x.ToString()).ToList();

//            Assert.Equal(6, observedOmimEntries.Count);
//            Assert.True(expectedOmimEntries.SequenceEqual(observedOmimEntries));
//        }
//    }
//}