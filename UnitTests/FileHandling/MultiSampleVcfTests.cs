using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.FileHandling
{
    [Collection("ChromosomeRenamer")]
    public sealed class MultiSampleVcfTests
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public MultiSampleVcfTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        [Fact]
		public void MultiSampleVcf()
		{
			using (var reader = new LiteVcfReader(ResourceUtilities.GetReadStream(Resources.TopPath("OneKMultiSample.vcf"))))
			{
				// HG00096	HG00097	HG00099	HG00100	HG00101	HG00102......
				Assert.Equal("HG00096", reader.SampleNames[0]);
				Assert.Equal("HG00097", reader.SampleNames[1]);
				Assert.Equal("HG00099", reader.SampleNames[2]);
				Assert.Equal("HG00100", reader.SampleNames[3]);
				Assert.Equal("HG00101", reader.SampleNames[4]);
			}
		}

		[Fact]
		public void JsonSamplesOutput()
		{
			using (var reader = new LiteVcfReader(ResourceUtilities.GetReadStream(Resources.InputFiles("Nirvana_unified_json_format.vcf"))))
			{
				// LP2000021	LP2000022	LP2000023
				Assert.Equal("LP2000021", reader.SampleNames[0]);
				Assert.Equal("LP2000022", reader.SampleNames[1]);
				Assert.Equal("LP2000023", reader.SampleNames[2]);

			    var variant = VcfUtilities.GetNextVariant(reader, _renamer);

				// chr9	138685463	.	A	C	.	PASS	BaseQRankSum=-1.61165;GQ=120;DP=43;ReadPosRankSum=0;MQ=60;SNVHPOL=3;SNVSB=-65.7;MQRankSum=0	GT:GQX:GQ:DP:DPF:AD	0/0:90:0:31:0:.	0/0:75:0:26:0:.	0/1:161:194:36:0:20,16

				// the unified json will call this function to get all the samples and can print them out using GetEntry as shown below
				var sampleVariants = variant.ExtractSampleInfo();

				Assert.Equal("0/0", sampleVariants[0].Genotype);
				Assert.Equal("0/0", sampleVariants[1].Genotype);
				Assert.Equal("0/1", sampleVariants[2].Genotype);


				const string expectedEntry1 = "{\"totalDepth\":31,\"genotypeQuality\":90,\"genotype\":\"0/0\"}";
				const string expectedEntry2 = "{\"totalDepth\":26,\"genotypeQuality\":75,\"genotype\":\"0/0\"}";
				const string expectedEntry3 = "{\"variantFreq\":0.4444,\"totalDepth\":36,\"genotypeQuality\":161,\"alleleDepths\":[20,16],\"genotype\":\"0/1\"}";

				Assert.Equal(expectedEntry1, sampleVariants[0].ToString());
				Assert.Equal(expectedEntry2, sampleVariants[1].ToString());
				Assert.Equal(expectedEntry3, sampleVariants[2].ToString());
			}
		}

        [Fact]
        public void MultipleAllelesMultipleSamples()
        {
            using (var reader = new LiteVcfReader(ResourceUtilities.GetReadStream(Resources.InputFiles("Nirvana_unified_json_format.vcf"))))
            {
                // LP2000021	LP2000022	LP2000023
                Assert.Equal("LP2000021", reader.SampleNames[0]);
                Assert.Equal("LP2000022", reader.SampleNames[1]);
                Assert.Equal("LP2000023", reader.SampleNames[2]);

                // skip the first variant
                VcfUtilities.GetNextVariant(reader, _renamer);
                var variant = VcfUtilities.GetNextVariant(reader, _renamer);

                // GT:GQ:GQX:DPI:AD	
                // 1/2:46:1:32:0,22,3	
                // 1/1:147:4:27:0,12,5	
                // 0/0:365:9:29:0,8,10

                // the unified json will call this function to get all the samples and can print them out using GetEntry as shown below
                var sampleVariants = variant.ExtractSampleInfo();

                Assert.Equal("1/2", sampleVariants[0].Genotype);
                Assert.Equal("1/1", sampleVariants[1].Genotype);
                Assert.Equal("0/0", sampleVariants[2].Genotype);


				// 1/2 - A
				var expectedEntry =
				"{\"variantFreq\":1,\"totalDepth\":32,\"genotypeQuality\":1,\"alleleDepths\":[0,22,3],\"genotype\":\"1/2\"}";
                var observedEntry    = sampleVariants[0].ToString();
                Assert.Equal(expectedEntry, observedEntry);

                // 1/1 - T
				expectedEntry = "{\"variantFreq\":1,\"totalDepth\":27,\"genotypeQuality\":4,\"alleleDepths\":[0,12,5],\"genotype\":\"1/1\"}";
                observedEntry = sampleVariants[1].ToString();
                Assert.Equal(expectedEntry, observedEntry);

                // 0/0 - A
				expectedEntry = "{\"variantFreq\":1,\"totalDepth\":29,\"genotypeQuality\":9,\"alleleDepths\":[0,8,10],\"genotype\":\"0/0\"}";
                observedEntry = sampleVariants[2].ToString();
                Assert.Equal(expectedEntry, observedEntry);
            }
        }

		[Fact]
		public void NoSamples()
		{
			using (var reader = new LiteVcfReader(ResourceUtilities.GetReadStream(Resources.InputFiles("NoSamples.vcf"))))
			{
				Assert.Null(reader.SampleNames);
			}
		}

		[Fact]
		public void EmptySamplesTest()
		{
			using (var reader = new LiteVcfReader(ResourceUtilities.GetReadStream(Resources.InputFiles("Nirvana_unified_json_format.vcf"))))
			{
                // getting the 4th variant
                VcfUtilities.GetNextVariant(reader, _renamer);
                VcfUtilities.GetNextVariant(reader, _renamer);
                VcfUtilities.GetNextVariant(reader, _renamer);
                var variant = VcfUtilities.GetNextVariant(reader, _renamer);

                // GT:GQ:GQX:DPI:AD	
                // 0/1:124:19:5:11,8:PASS:.	
                // .	
                // 1/2:55:59:.:0,21:LowGQX:20

                // the unified json will call this function to get all the samples and can print them out using GetEntry as shown below
                var sampleVariants = variant.ExtractSampleInfo();

				var expectedEntry =
				"{\"variantFreq\":0.4211,\"genotypeQuality\":124,\"alleleDepths\":[11,8],\"genotype\":\"0/1\"}";
				var observedEntry = sampleVariants[0].ToString();
				Assert.Equal(expectedEntry, observedEntry);

				expectedEntry = "{\"isEmpty\":true}";
				observedEntry = sampleVariants[1].ToString();
				Assert.Equal(expectedEntry, observedEntry);

				expectedEntry =
					"{\"variantFreq\":1,\"totalDepth\":20,\"genotypeQuality\":55,\"alleleDepths\":[0,21],\"genotype\":\"1/2\",\"failedFilter\":true}";
                observedEntry = sampleVariants[2].ToString();
				Assert.Equal(expectedEntry, observedEntry);
			}
		}

		[Fact]
		public void SamplesWithFilterAndGq()
		{
			using (var reader = new LiteVcfReader(ResourceUtilities.GetReadStream(Resources.InputFiles("Nirvana_unified_json_format.vcf"))))
			{
                // skip the first two variants
                VcfUtilities.GetNextVariant(reader, _renamer);
                VcfUtilities.GetNextVariant(reader, _renamer);
                var variant = VcfUtilities.GetNextVariant(reader, _renamer);

                // GT:GQ:DP:DPF:AD:FT:DPI      
                // 0/1:124:19:5:11,8:PASS:.        
                // 2/2:58:55:.:0,23:LowGQX:21	
                // 1/2:55:59:.:0,21:LowGQX:20

                // the unified json will call this function to get all the samples and can print them out using GetEntry as shown below
                var sampleVariants = variant.ExtractSampleInfo();

				Assert.Equal("0/1", sampleVariants[0].Genotype);
				Assert.Equal("2/2", sampleVariants[1].Genotype);
				Assert.Equal("1/2", sampleVariants[2].Genotype);



				var expectedEntry =
				"{\"variantFreq\":0.4211,\"genotypeQuality\":124,\"alleleDepths\":[11,8],\"genotype\":\"0/1\"}";
				var observedEntry = sampleVariants[0].ToString();
				Assert.Equal(expectedEntry, observedEntry);

				// 1/1 - T
				expectedEntry =
				"{\"variantFreq\":1,\"totalDepth\":21,\"genotypeQuality\":58,\"alleleDepths\":[0,23],\"genotype\":\"2/2\",\"failedFilter\":true}";
				observedEntry = sampleVariants[1].ToString();
				Assert.Equal(expectedEntry, observedEntry);

				// 0/0 - A
				expectedEntry =
				"{\"variantFreq\":1,\"totalDepth\":20,\"genotypeQuality\":55,\"alleleDepths\":[0,21],\"genotype\":\"1/2\",\"failedFilter\":true}";
				observedEntry = sampleVariants[2].ToString();
				Assert.Equal(expectedEntry, observedEntry);
			}
		}

	}
}
