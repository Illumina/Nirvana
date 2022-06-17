using System.IO;
using System.Linq;
using SAUtils.InputFileParsers.Decipher;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class DecipherTests
    {
        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            // file has been modified to 7 columns
            writer.WriteLine("#population_cnv_id\tchr\tstart\tend\tdeletion_observations\tdeletion_frequency\tdeletion_standard_error\tduplication_observations\tduplication_frequency\tduplication_standard_error\tobservations\tfrequency\tstandard_error\ttype\tsample_size\tstudy");
            writer.WriteLine("1\t1\t10529\t177368\t0\t0\t1\t3\t0.075\t0.555277708\t3\t0.075\t0.555277708\t1\t40\t42M calls");
            writer.WriteLine("2\t1\t13516\t91073\t0\t0\t1\t27\t0.675\t0.109713431\t27\t0.675\t0.109713431\t1\t40\t42M call");
            writer.WriteLine("3\t1\t18888\t35451\t0\t0\t1\t2\t0.002366864\t0.706269473\t2\t0.002366864\t0.706269473\t1\t845\tDDD");
            writer.WriteLine("4\t1\t23946\t88271\t27\t0.031952663\t0.189350482\t21\t0.024852071\t0.215489247\t48\t0.056804734\t0.140178106\t0\t845\tDDD");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetItemsTest()
        {
            var decipherReader = new DecipherParser(new StreamReader(GetStream()), ChromosomeUtilities.RefNameToChromosome);
            var items = decipherReader.GetItems().ToList();

            Assert.Equal(4, items.Count);
            Assert.Equal("\"chromosome\":\"1\",\"begin\":10529,\"end\":177368,\"numDeletions\":0,\"deletionFrequency\":0,\"numDuplications\":3,\"duplicationFrequency\":0.075,\"sampleSize\":40", items[0].GetJsonString());
            Assert.Equal("\"chromosome\":\"1\",\"begin\":13516,\"end\":91073,\"numDeletions\":0,\"deletionFrequency\":0,\"numDuplications\":27,\"duplicationFrequency\":0.675,\"sampleSize\":40", items[1].GetJsonString());
            Assert.Equal("\"chromosome\":\"1\",\"begin\":18888,\"end\":35451,\"numDeletions\":0,\"deletionFrequency\":0,\"numDuplications\":2,\"duplicationFrequency\":0.002367,\"sampleSize\":845", items[2].GetJsonString());
            Assert.Equal("\"chromosome\":\"1\",\"begin\":23946,\"end\":88271,\"numDeletions\":27,\"deletionFrequency\":0.031953,\"numDuplications\":21,\"duplicationFrequency\":0.024852,\"sampleSize\":845", items[3].GetJsonString());
        }
    }
}