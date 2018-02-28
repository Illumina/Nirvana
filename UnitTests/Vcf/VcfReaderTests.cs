using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Phantom.Workers;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Vcf;
using Vcf.Info;
using Vcf.Sample;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf
{
    public sealed class VcfReaderTests
    {
        private MemoryStream _ms;
        private StreamWriter _streamWriter;

        private void AddLines(string[] lines)
        {
            _ms = new MemoryStream();
            _streamWriter = new StreamWriter(_ms);
            foreach (var headline in lines)
            {
                _streamWriter.WriteLine(headline);
            }
            _streamWriter.Flush();

            _ms.Position = 0;
        }

        [Fact]
        public void Invalid_format_exception_thrown_when_header_no_genotype_line()
        {
            var headers = new[] { "##fileformat=VCFv4.1", "##FILTER=<ID=PASS,Description=\"All filters passed\">", "##fileDate=20160920" };
            AddLines(headers);
            Assert.Throws<FormatException>(() => new VcfReader(_ms, null, null, false, null));
        }

        [Fact]
        public void HeaderLines_are_parsed()
        {
            var headers = new[] { "##fileformat=VCFv4.1", "##FILTER=<ID=PASS,Description=\"All filters passed\">", "##fileDate=20160920", "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT	NHL-16" };
            AddLines(headers);
            IEnumerable<string> observedHeaders;
            using (var vcfReader = new VcfReader(_ms, null, null, false, new NullRecomposer()))
            {
                observedHeaders = vcfReader.GetHeaderLines();
            }

            Assert.Equal(headers, observedHeaders.ToArray());
        }

        [Fact]
        public void Duplicated_headlines_are_removed()
        {
            var headers = new[] { "##fileformat=VCFv4.1", "##FILTER=<ID=PASS,Description=\"All filters passed\">", "##fileDate=20160920", "##dataSource=ClinVar,version:unknown,release date:2016-09-01", "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT	NHL-16	NHL-17" };
            AddLines(headers);
            IEnumerable<string> observedHeaders;
            using (var vcfReader = new VcfReader(_ms, null, null, false, new NullRecomposer()))
            {
                observedHeaders = vcfReader.GetHeaderLines();
            }

            Assert.Equal(4, observedHeaders.Count());
        }

	    [Fact]
	    public void GetSampleCopyNumber_from_sample()
	    {
		    const string vcfLine =
			    @"1	723707	.	N	<CNV>	41	PASS	SVTYPE=CNV;END=2581225	RC:BC:CN:MCC	.	129:3123:3:2";

			var samples = new SampleFieldExtractor(vcfLine.Split('\t')).ExtractSamples();

		    var sampleCopyNumber = Position.GetSampleCopyNumbers(samples);

			Assert.Equal(3, sampleCopyNumber);
	    }

	    [Fact]
	    public void CopyNumber_from_info_overrides_sample()
	    {
		    const string vcfLine =
			    @"1	723707	.	N	<CNV>	41	PASS	SVTYPE=CNV;END=2581225;CN=4	RC:BC:CN:MCC	.	129:3123:3:2";

		    var vcfColumns = vcfLine.Split('\t');

			var infoData = VcfInfoParser.Parse(vcfColumns[VcfCommon.InfoIndex]);
			var samples = new SampleFieldExtractor(vcfColumns).ExtractSamples();

		    var sampleCopyNumber = Position.GetSampleCopyNumbers(samples);

		    var chromosome1 = new Chromosome("chr1", "1", 0);
		    var variantFactory = new VariantFactory(new Dictionary<string, IChromosome> { { "1", chromosome1 } }, null, false);

		    var variants = variantFactory.CreateVariants(chromosome1, null, 723707, 2581225, "N", new[] { "<CNV>" }, infoData, sampleCopyNumber, new[]{false}, false);
		    Assert.NotNull(variants);
		    Assert.Null(variants[0].BreakEnds);

		    Assert.Equal("1:723708:2581225:4", variants[0].VariantId);
		    Assert.Equal(VariantType.copy_number_gain, variants[0].Type);

	    }

		[Fact]
	    public void UpdateCnvInfo_regular_chrY()
	    {
		    const string vcfLine =
			    @"Y	723707	.	N	<CNV>	41	PASS	SVTYPE=CNV;END=2581225	RC:BC:CN:MCC	.	129:3123:3:2";

		    var vcfColumns = vcfLine.Split('\t');

		    var infoData = VcfInfoParser.Parse(vcfColumns[VcfCommon.InfoIndex]);
		    var samples = new SampleFieldExtractor(vcfColumns).ExtractSamples();

		    var sampleCopyNumber = Position.GetSampleCopyNumbers(samples);
		    var chromosome1 = new Chromosome("chr1", "1", 0);
		    var variantFactory = new VariantFactory(new Dictionary<string, IChromosome> { { "1", chromosome1 } }, null, false);

			var variants = variantFactory.CreateVariants(chromosome1, null, 723707, 2581225, "N", new[] { "<CNV>" }, infoData, sampleCopyNumber, new[]{ false}, false);

		    Assert.NotNull(variants);
		    Assert.Null(variants[0].BreakEnds);

		    Assert.Equal("1:723708:2581225:3", variants[0].VariantId);
		    Assert.Equal(VariantType.copy_number_gain, variants[0].Type);
		}

		[Fact]
        public void Sample_names_are_reported()
        {
            var headers = new[] { "##fileformat=VCFv4.1", "##FILTER=<ID=PASS,Description=\"All filters passed\">", "##fileDate=20160920", "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT	NHL-16	NHL-17" };
            AddLines(headers);
            string[] samples;
            using (var vcfReader = new VcfReader(_ms, null, null, false, new NullRecomposer()))
            {
                samples = vcfReader.GetSampleNames();
            }

            Assert.Equal(new[] { "NHL-16", "NHL-17" }, samples);
        }

        [Fact]
        public void GetNextPosition()
        {
            const string vcfLine = "chr1	13133	.	T	C	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            var lines = new[]
            {
                "##fileformat=VCFv4.1", "##FILTER=<ID=PASS,Description=\"All filters passed\">", "##fileDate=20160920",
                "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT	NHL-16", vcfLine
            };

            AddLines(lines);

            var chromosome = new Chromosome("chr1", "1", 0);

            var refMinorProvider = new Mock<IRefMinorProvider>();
            refMinorProvider.Setup(x => x.IsReferenceMinor(chromosome, 13133)).Returns(false);

            var refNameToChromosome = new Dictionary<string, IChromosome> { ["chr1"] = chromosome };

            IPosition observedResult;
            using (var vcfReader = new VcfReader(_ms, refNameToChromosome, refMinorProvider.Object, false, new NullRecomposer()))
            {
                observedResult = vcfReader.GetNextPosition();
            }

            var expectedResult = new Position(chromosome, 13133, 13133, "T", new[] { "C" }, 36, new[] { "PASS" }, null,
                null, null, vcfLine.Split('\t'), new[] { false }, false);

            Assert.NotNull(observedResult);
            Assert.Equal(expectedResult.End,        observedResult.End);
            Assert.Equal(expectedResult.AltAlleles, observedResult.AltAlleles);
            Assert.Equal(expectedResult.Filters,    observedResult.Filters);
            Assert.Equal(expectedResult.Quality,    observedResult.Quality);
            Assert.Equal(expectedResult.VcfFields,  observedResult.VcfFields);
        }
    }
}