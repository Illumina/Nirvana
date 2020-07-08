using System.IO;
using System.Linq;
using System.Text;
using Moq;
using SAUtils.gnomAD;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Providers;
using Xunit;

namespace UnitTests.SAUtils.gnomAD
{
    public class LcrParserTests
    {
        private Stream GetGRCh37Stream()
        {
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.Default, 512*1024, true))
            {
                writer.WriteLine("1:1-10000");
                writer.WriteLine("1:40637-40658");
                writer.WriteLine("1:77172-77195");
            }

            stream.Position = 0;
            return stream;
        }
        
        private ISequenceProvider GetGRCh37()
        {
            var seqProvider = new Mock<ISequenceProvider>();

            seqProvider.Setup(x => x.Sequence.Substring(0, It.IsAny<int>())).
                Returns(new string('n',500)+new string ('N',500));
            seqProvider.Setup(x => x.Sequence.Substring(40637-1, It.IsAny<int>())).
                Returns(new string('A',50) +new string ('C',50));
            seqProvider.Setup(x => x.Sequence.Substring(77172 -1, It.IsAny<int>())).
                Returns(new string('T',50) +new string ('G',50));

            seqProvider.SetupGet(x => x.RefNameToChromosome).Returns(
                ChromosomeUtilities.RefNameToChromosome);
            return seqProvider.Object;
        }
        
        private Stream GetGRCh38Stream()
        {
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.Default, 512 *1024, true))
            {
                writer.WriteLine("chr1\t9999\t10468");
                writer.WriteLine("chr1\t30853\t30959");
                writer.WriteLine("chr1\t47317\t47328");
            }

            stream.Position = 0;
            return stream;
        }

        private ISequenceProvider GetGRCh38()
        {
            var seqProvider = new Mock<ISequenceProvider>();

            seqProvider.Setup(x => x.Sequence.Substring(9999 -1, It.IsAny<int>())).
                Returns(new string('G',50) +new string ('C',50));
            seqProvider.Setup(x => x.Sequence.Substring(30853 -1, It.IsAny<int>())).
                Returns(new string('A',50) +new string ('C',50));
            seqProvider.Setup(x => x.Sequence.Substring(47317 -1, It.IsAny<int>())).
                Returns(new string('T',50) +new string ('G',50));

            seqProvider.SetupGet(x => x.RefNameToChromosome).Returns(
                ChromosomeUtilities.RefNameToChromosome);
            
            return seqProvider.Object;
        }

        [Fact]
        public void GetGRCh37Lcrs()
        {
            var parser = new LcrRegionParser(new StreamReader(GetGRCh37Stream()), GetGRCh37());

            var items = parser.GetItems().ToList();
            
            Assert.Equal(2, items.Count);
            
        }
        
        [Fact]
        public void GetGRCh38Lcrs()
        {
            var parser = new LcrRegionParser(new StreamReader(GetGRCh38Stream()), GetGRCh38());

            var items = parser.GetItems().ToList();
            
            Assert.Equal(3, items.Count);
            
        }
    }
}