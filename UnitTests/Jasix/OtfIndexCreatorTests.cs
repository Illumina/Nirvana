using System.IO;
using Jasix;
using Jasix.DataStructures;
using Moq;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Positions;
using Xunit;

namespace UnitTests.Jasix
{
    public sealed class OtfIndexCreatorTests
    {
        [Fact]
        public void Add_one_chrom()
        {
            var position1 = new Mock<IPosition>();
            position1.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr1);
            position1.SetupGet(x => x.Start).Returns(100);
            position1.SetupGet(x => x.RefAllele).Returns("A");
            position1.SetupGet(x => x.AltAlleles).Returns(new []{"C"});

            var memStream = new MemoryStream();
            using (var indexCreator = new OnTheFlyIndexCreator(memStream))
            {
                indexCreator.BeginSection("positions", 100);
                indexCreator.Add(position1.Object, 2588);
                indexCreator.EndSection("positions",2699 );
            }

            var readStream = new MemoryStream(memStream.ToArray());
            readStream.Seek(0, SeekOrigin.Begin);
            var index = new JasixIndex(readStream);

            Assert.Equal(100, index.GetSectionBegin("positions"));
            Assert.Equal(2588, index.GetFirstVariantPosition("chr1", 100,102));
        }
    }
}