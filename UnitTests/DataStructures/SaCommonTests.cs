using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ErrorHandling.Exceptions;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.DataStructures
{
	public sealed class SaCommonTests
	{
		//[Fact]
		//public void  CheckDirectoryIntegrityTests()
		//{
		//	var dataSourceVersions = new List<DataSourceVersion>();
		//	SupplementaryAnnotationDirectory saDirectory;

		//	var saPath = Path.Combine("..", "..", "UnitTests", "Resources", "MiniSuppAnnot");
		//	SupplementaryAnnotationCommon.CheckDirectoryIntegrity(saPath, dataSourceVersions, out saDirectory);


		//	Assert.NotNull(saDirectory);

		//	Assert.Equal(8,dataSourceVersions.Count);

		//	Assert.Equal(SupplementaryAnnotationCommon.DataVersion,saDirectory.DataVersion);
		//}

		[Fact]
		void CheckDirectoryNoSA()
		{
			var dataSourceVersions = new List<DataSourceVersion>();

		    Exception obsException = null;

			try
			{
			    SupplementaryAnnotationDirectory saDirectory;
			    SupplementaryAnnotationCommon.CheckDirectoryIntegrity(Resources.TopPath("CustomIntervals"), dataSourceVersions, out saDirectory);
			}
			catch (Exception ex)
			{
				obsException = ex;
			}

			Assert.NotNull(obsException);
			Assert.True(obsException.GetType().IsAssignableFrom(typeof(UserErrorException)));
		}
	}
}