using VariantAnnotation.Interface.AnnotatedPositions;
using Moq;
using VariantAnnotation.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
	public sealed class ProteinChangeTests
	{
		[Fact]
		public void Substitution()
		{
			var variantEffect = new Mock<IVariantEffect>();
			variantEffect.Setup(x => x.IsFrameshiftVariant()).Returns(false);

			variantEffect.Setup(x => x.IsStopRetained()).Returns(false);
			variantEffect.Setup(x => x.IsStartLost()).Returns(false);
			variantEffect.Setup(x => x.IsStopLost()).Returns(false);

			var proteinChange = HgvsProteinNomenclature.GetProteinChange(5, "A", "B", "MACTAWR", variantEffect.Object);

			Assert.Equal(ProteinChange.Substitution, proteinChange);
		}

		[Fact]
		public void Single_base_deletion()
		{
			var variantEffect = new Mock<IVariantEffect>();
			variantEffect.Setup(x => x.IsFrameshiftVariant()).Returns(false);

			variantEffect.Setup(x => x.IsStopRetained()).Returns(false);
			variantEffect.Setup(x => x.IsStartLost()).Returns(false);
			variantEffect.Setup(x => x.IsStopLost()).Returns(false);

			var proteinChange = HgvsProteinNomenclature.GetProteinChange(5, "A", "", "MACTAWR", variantEffect.Object);

			Assert.Equal(ProteinChange.Deletion, proteinChange);
		}

		[Fact]
		public void Frameshift()
		{
			var variantEffect = new Mock<IVariantEffect>();
			variantEffect.Setup(x => x.IsFrameshiftVariant()).Returns(true);

			variantEffect.Setup(x => x.IsStopRetained()).Returns(false);
			variantEffect.Setup(x => x.IsStartLost()).Returns(false);
			variantEffect.Setup(x => x.IsStopLost()).Returns(false);

			var proteinChange = HgvsProteinNomenclature.GetProteinChange(5, "A", "C", "MACTAWR", variantEffect.Object);

			Assert.Equal(ProteinChange.Frameshift, proteinChange);
		}

		[Fact]
		public void Extension()
		{
			var variantEffect = new Mock<IVariantEffect>();
			variantEffect.Setup(x => x.IsFrameshiftVariant()).Returns(false);

			variantEffect.Setup(x => x.IsStopRetained()).Returns(false);
			variantEffect.Setup(x => x.IsStartLost()).Returns(false);
			variantEffect.Setup(x => x.IsStopLost()).Returns(true);

			var proteinChange = HgvsProteinNomenclature.GetProteinChange(5, "*", "C", "MACTAWR", variantEffect.Object);

			Assert.Equal(ProteinChange.Extension, proteinChange);
		}

		[Fact]
		public void Duplication()
		{
			var variantEffect = new Mock<IVariantEffect>();
			variantEffect.Setup(x => x.IsFrameshiftVariant()).Returns(false);

			variantEffect.Setup(x => x.IsStopRetained()).Returns(false);
			variantEffect.Setup(x => x.IsStartLost()).Returns(false);
			variantEffect.Setup(x => x.IsStopLost()).Returns(false);

			var proteinChange = HgvsProteinNomenclature.GetProteinChange(6, "", "A", "MACTAWR", variantEffect.Object);

			Assert.Equal(ProteinChange.Duplication, proteinChange);
		}

		[Fact]
		public void Insertion()
		{
			var variantEffect = new Mock<IVariantEffect>();
			variantEffect.Setup(x => x.IsFrameshiftVariant()).Returns(false);

			variantEffect.Setup(x => x.IsStopRetained()).Returns(false);
			variantEffect.Setup(x => x.IsStartLost()).Returns(false);
			variantEffect.Setup(x => x.IsStopLost()).Returns(false);

			var proteinChange = HgvsProteinNomenclature.GetProteinChange(4, "", "A", "MACTAWR", variantEffect.Object);

			Assert.Equal(ProteinChange.Insertion, proteinChange);
		}
	}
}