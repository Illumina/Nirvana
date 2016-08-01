using System.Collections.Generic;
using System.IO;
using System.Linq;
using Illumina.VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Illumina.VariantAnnotation.Interface;
using InputFileParsers.ClinGen;
using UnifyClinGenFile;
using Xunit;

namespace NirvanaUnitTests.FileHandling
{
	[Collection("Chromosome 1 collection")]
	public class ClinGenReaderTests
	{
		private static readonly FileInfo TestClinGenFile = new FileInfo(@"Resources\testClinGenUnifier.txt");


		private static IEnumerable<ClinGenItem> CreateTruthClinGenItemSequence()
		{
			yield return new ClinGenItem("nsv869079", "1", 757093, 2394455, VariantType.copy_number_variation, 1, 1,
				ClinicalInterpretation.pathogenic, true,
				new HashSet<string> { "Developmental delay AND/OR other significant developmental or morphological phenotypes" });
			yield return new ClinGenItem("nsv529358", "1", 779727, 2558913, VariantType.copy_number_loss, 0, 2,
						ClinicalInterpretation.pathogenic, true,
						new HashSet<string>
						{
							"Hypotelorism",
							"Microcephaly",
							"Short stature",
							"Developmental delay AND/OR other significant developmental or morphological phenotypes"
						},
						new HashSet<string>
						{
							"HP:0000252",
							"HP:0000601",
							"HP:0004322",
							"MedGen:C0349588",
							"MedGen:C1845868",
							"MedGen:CN000563"
						});
			yield return new ClinGenItem("nsv932267", "1", 65410208, 68057686, VariantType.copy_number_loss, 0, 1,
						ClinicalInterpretation.uncertain_significance, true,
						new HashSet<string>
						{
							"Intellectual disability",
							"Panhypopituitarism",
							"Short stature"
						},
						new HashSet<string>
						{
							"HP:0000871",
							"HP:0001249",
							"HP:0004322",
							"MedGen:C0349588",
							"MedGen:C1843367",
							"MedGen:CN000817"
						});
			yield return new ClinGenItem("nsv530955", "1", 145601946, 146944906, VariantType.copy_number_gain, 2, 0,
						ClinicalInterpretation.benign, false,
						new HashSet<string>
						{
							"Developmental delay AND/OR other significant developmental or morphological phenotypes",
							"Specific learning disability",
						},
						new HashSet<string>
						{
							"HP:0001328",
							"MedGen:CN001216"
						});
			yield return new ClinGenItem("nsv530955", "1", 146987841, 148234205, VariantType.copy_number_gain, 2, 0,
						ClinicalInterpretation.benign, false,
						new HashSet<string>
						{
							"Developmental delay AND/OR other significant developmental or morphological phenotypes",
							"Specific learning disability",
						},
						new HashSet<string>
						{
							"HP:0001328",
							"MedGen:CN001216"
						});

		}

	
			

		[Fact(Skip = "need to fix")]
		public void TestClinGenUnifier()
		{
			ClinGenUnifier clinGenUnifier = new ClinGenUnifier(TestClinGenFile);
			clinGenUnifier.Unify();

			string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			clinGenUnifier.Write(randomPath);

			var testUnifiedClinGenFile =new FileInfo(randomPath);
			var clinGenReader = new ClinGenReader(testUnifiedClinGenFile);
			var expectedItems = CreateTruthClinGenItemSequence();
			Assert.True(clinGenReader.SequenceEqual(expectedItems));

			File.Delete(randomPath);

		}

	}
}