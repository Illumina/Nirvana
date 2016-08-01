using System;
using System.Reflection;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class ClearingTests
    { 
		[Fact]
		public void ClearVariantFeature()
		{
			Type t = typeof(VariantFeature);

			var propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			Assert.Equal(21, propInfos.Length);

            // populating the alternate alleles array
            const string vcfLine = "17	4634317	.	C	A,T	256	PASS	SNVSB=-27.1;SNVHPOL=7	GT	1/2";
		    var variant = VcfUtilities.GetVariantFeature(vcfLine);

			// add garbage values to a json varint object
			foreach (var propertyInfo in propInfos)
			{
				if (propertyInfo.PropertyType == typeof(string))
					propertyInfo.SetValue(variant, "bob");
				if (propertyInfo.PropertyType == typeof(int))
					propertyInfo.SetValue(variant, 100);
				if (propertyInfo.PropertyType == typeof(bool))
					propertyInfo.SetValue(variant, true);
			}
			
			variant.Clear();
			foreach (var propertyInfo in propInfos)
			{
				if (propertyInfo.PropertyType == typeof(string))
					Assert.Null(propertyInfo.GetValue(variant));
				if (propertyInfo.PropertyType == typeof(int))
					Assert.Equal(0, propertyInfo.GetValue(variant));
				if (propertyInfo.PropertyType == typeof(bool))
					Assert.Equal(false,propertyInfo.GetValue(variant));

				if (propertyInfo.PropertyType == typeof(string[]))
					Assert.Null(propertyInfo.GetValue(variant));
			}

			Assert.Empty(variant.AlternateAlleles);
		}		
	}
}
