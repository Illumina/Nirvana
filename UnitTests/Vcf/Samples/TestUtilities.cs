using Moq;
using VariantAnnotation.Interface.Positions;

namespace UnitTests.Vcf.Samples
{
    public static class TestUtilities
    {
        public static ISimplePosition GetSimplePositionUsingAlleleNum(int numAlleles)
        {
            var mock = new Mock<ISimplePosition>();
            mock.SetupGet(x => x.AltAlleles).Returns(new string[numAlleles]);
            mock.SetupGet(x => x.Start).Returns(-1);

            return mock.Object;
        }

    }
}