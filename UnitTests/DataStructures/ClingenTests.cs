using System.Collections.Generic;
using System.IO;
using SAUtils.InputFileParsers.ClinGen;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using Xunit;
using System.Linq;

namespace UnitTests.DataStructures
{
	[Collection("Chromosome 1 collection")]
	public class ClingenTests
	{
		[Fact]
		public void EqualityAndHash()
		{
			var clinGenItem = new ClinGenItem("CGEN101","chr1", 100, 1000, VariantType.copy_number_gain, 100, 0,ClinicalInterpretation.likely_pathogenic, true, new HashSet<string>() {"phenotype1"}, new HashSet<string>() {"phenoId1"});

			var clingenHash = new HashSet<ClinGenItem>() { clinGenItem };

			Assert.Equal(1, clingenHash.Count);
			Assert.True(clingenHash.Contains(clinGenItem));
		}

		[Fact]
		public void ReadClingenItems()
		{
			var clingenReader = new ClinGenReader(new FileInfo(@"Resources\InputFiles\ClinGenMini.tsv"));

			var clinGenList = clingenReader.ToList();

			Assert.Equal("nsv530705", clinGenList[0].Id);
			Assert.Equal(564405, clinGenList[0].Start);
			Assert.Equal(8597804, clinGenList[0].End);
			Assert.Equal(new List<string>() { "Developmental delay AND/OR other significant developmental or morphological phenotypes" }, clinGenList[0].Phenotypes);
			Assert.Equal(new HashSet<string>(), clinGenList[0].PhenotypeIds);
			Assert.Equal(ClinicalInterpretation.pathogenic, clinGenList[0].ClinicalInterpretation);
			
		}

		[Fact]
		public void GetInterval()
		{
			var clingenReader = new ClinGenReader(new FileInfo(@"Resources\InputFiles\ClinGenMini.tsv"));

			var clinGenList = clingenReader.ToList();

			var si = clinGenList[0].GetSupplementaryInterval();

			Assert.Equal("nsv530705", si.StringValues["id"]);
			Assert.Equal(564405, si.Start);
			Assert.Equal(8597804, si.End);
			Assert.Equal(VariantType.copy_number_loss, si.VariantType);
		}
	}
}
