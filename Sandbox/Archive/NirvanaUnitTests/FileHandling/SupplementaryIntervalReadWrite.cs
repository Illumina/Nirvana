
using System.IO;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;
using InputFileParsers.OneKGen;
using Xunit;

namespace NirvanaUnitTests.FileHandling
{
	[Collection("Chromosome 1 collection")]
	public sealed class SupplementaryIntervalReadWrite
	{
		[Fact(Skip="use new format for reading 1000 genome intervals")]
		public void WriteReadSuppInterval()
		{
			const string vcfLine = "1	713044	esv3584977;esv3584978	C	<CN0>,<CN2>	100	PASS	AC=3,206;AF=0.000599042,0.0411342;AN=5008;CS=DUP_gs;END=755966;NS=2504;SVTYPE=CNV;DP=20698;EAS_AF=0.001,0.0615;AMR_AF=0.0014,0.0259;AFR_AF=0,0.0303;EUR_AF=0.001,0.0417;SAS_AF=0,0.045;VT=SV;EX_TARGET";

			string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var randomFileStream = new FileStream(randomPath, FileMode.Create);
            var writer= new ExtendedBinaryWriter(new BinaryWriter(randomFileStream));

			var oneKGenReader = new OneKGenReader();
			oneKGenReader.ExtractOneKGenItem(vcfLine);

			foreach (var oneKGenItem in oneKGenReader.ExtractOneKGenItem(vcfLine))
			{
				var suppInterval = oneKGenItem.GetSupplementaryInterval();
				suppInterval?.Write(writer);
			}
			
			randomFileStream.Close();

			// now we will read the intervals
			var readFileStream = new FileStream(randomPath, FileMode.Open);
			var reader = new ExtendedBinaryReader(new BinaryReader(readFileStream));

			var firstItem = SupplementaryInterval.Read(reader);

			Assert.Equal(713045, firstItem.Start);
			Assert.Equal(755966, firstItem.End);
			Assert.Equal("<CN0>", firstItem.AlternateAllele);
			Assert.Equal("CNV", firstItem.VariantType.ToString());
			Assert.Equal("1000 Genomes Project", firstItem.Source);
			Assert.Equal(0.000599042, firstItem.PopulationFrequencies["OneKgAll"]);
			Assert.Equal(0.001, firstItem.PopulationFrequencies["OneKgEas"]);
			Assert.Equal(0.0014, firstItem.PopulationFrequencies["OneKgAmr"]);
			Assert.Equal(0, firstItem.PopulationFrequencies["OneKgAfr"]);
			Assert.Equal(0.001, firstItem.PopulationFrequencies["OneKgEur"]);
			Assert.Equal(0, firstItem.PopulationFrequencies["OneKgSas"]);

			var secondItem = SupplementaryInterval.Read(reader);

			Assert.Equal(713045, secondItem.Start);
			Assert.Equal(755966, secondItem.End);
			Assert.Equal("<CN2>", secondItem.AlternateAllele);
			Assert.Equal("CNV", secondItem.VariantType.ToString());
			Assert.Equal("1000 Genomes Project", secondItem.Source);
			Assert.Equal(0.0411342, secondItem.PopulationFrequencies["OneKgAll"]);
			Assert.Equal(0.0615, secondItem.PopulationFrequencies["OneKgEas"]);
			Assert.Equal(0.0259, secondItem.PopulationFrequencies["OneKgAmr"]);
			Assert.Equal(0.0303, secondItem.PopulationFrequencies["OneKgAfr"]);
			Assert.Equal(0.0417, secondItem.PopulationFrequencies["OneKgEur"]);
			Assert.Equal(0.045, secondItem.PopulationFrequencies["OneKgSas"]);

			readFileStream.Close();
			File.Delete(randomPath);
		}
	}
}
