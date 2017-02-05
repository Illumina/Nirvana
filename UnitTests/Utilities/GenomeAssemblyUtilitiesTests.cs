using System;
using VariantAnnotation.DataStructures;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.Utilities
{
	public class GenomeAssemblyUtlilTest
	{
		[Fact]
		public void GenomeAssemblyConvertTest()
		{
			Assert.Equal(GenomeAssembly.GRCh37, GenomeAssemblyUtilities.Convert("grCh37"));
			Assert.Equal(GenomeAssembly.GRCh38, GenomeAssemblyUtilities.Convert("GrCh38"));
			Assert.Equal(GenomeAssembly.hg19, GenomeAssemblyUtilities.Convert("HG19"));
			var error = false;
			try
			{
				GenomeAssemblyUtilities.Convert("asldkjf");
			}
			catch (Exception)
			{
				error = true;
			}

			Assert.True(error);
		}
	}
}
