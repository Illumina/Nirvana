using VariantAnnotation.Interface.Positions;
using Vcf.Sample;
using Xunit;

namespace UnitTests.Vcf.Samples
{
    public sealed class SampleFieldExtractorTests
    {
        [Fact]
        public void NormalizeNulls()
        {
            const string periwinkle = "periwinkle";
            string[] cols = { periwinkle, "", ".", null };
            cols.NormalizeNulls();

            Assert.Equal(periwinkle, cols[0]);
            Assert.Null(cols[1]);
            Assert.Null(cols[2]);
            Assert.Null(cols[3]);
        }

        [Fact]
        public void ExtractSample_PEPE()
        {
            var formatIndices = new FormatIndices();
            formatIndices.Set("GT:GQ:AD:DP:VF:NL:SB:NC:US:AQ:LQ");
            var sample = SampleFieldExtractor.ExtractSample("0/1:5:338,1:339:0.00295:30:-7.3191:0.0314:0,0,0,1,0,0,17,1,129,21,148,22:3.366:0.000", formatIndices, 1, false);

            Assert.Equal("0/1",             sample.Genotype);
            Assert.Equal(5,                 sample.GenotypeQuality);
            Assert.Equal(new[] { 338, 1 },  sample.AlleleDepths);
            Assert.Equal(339,               sample.TotalDepth);
            Assert.Equal(new[] { 0.00295 }, sample.VariantFrequencies);
            Assert.Equal(3.366f,            sample.ArtifactAdjustedQualityScore);
            Assert.Equal(0.000f,            sample.LikelihoodRatioQualityScore);
        }

        [Fact]
        public void ExtractSample_ExpansionHunter()
        {
            var formatIndices = new FormatIndices();
            formatIndices.Set("GT:SO:REPCN:REPCI:ADSP:ADFL:ADIR:LC");
            var sample = SampleFieldExtractor.ExtractSample("1/1:SPANNING/SPANNING:15/15:15-15/15-15:22/22:23/23:0/0:38.270270", formatIndices, 1, false);

            Assert.Equal("1/1", sample.Genotype);
            Assert.Equal(new[] { 15, 15 }, sample.RepeatUnitCounts);
        }

        [Fact]
        public void ExtractSample_EmptySampleColumn_ReturnEmptySample()
        {
            var formatIndices = new FormatIndices();
            var sample = SampleFieldExtractor.ExtractSample(null, formatIndices, 1, false);
            Assert.True(sample.IsEmpty);
        }

        [Fact]
        public void ExtractSample_DotInSampleColumn_ReturnEmptySample()
        {
            var formatIndices = new FormatIndices();
            var sample = SampleFieldExtractor.ExtractSample(".", formatIndices, 1, false);
            Assert.True(sample.IsEmpty);
        }

        [Fact]
        public void ToSamples_SMN1_CNV()
        {
            // GT:AD:DST:RPL:LC
            // 0/1:30,20:-:35.8981:45.810811

            // GT:SM:CN:BC:QS:FT:DN
            // ./1:1.24763:3:4:5:cnvLength:.
            // ./.:1.17879:2:4:8:cnvLength:.
            // ./1:1.26335:3:4:6:cnvLength:Inherited

            var formatIndices = new FormatIndices();

            string[] cols = {
                "chr1",
                "125068769",
                "DRAGEN:GAIN:125068770-125075279",
                "N",
                "<DUP>",
                ".",
                "SampleFT",
                "SVTYPE=CNV;END=125075279;REFLEN=6510",
                "GT:AD:DST:RPL:LC:SM:CN:BC:QS:FT:DN",
                "0/1:30,20:-:35.8981:45.810811",
                "./1:.:.:.:.:1.24763:3:4:5:cnvLength:.",
                "./.:.:.:.:.:1.17879:2:4:8:cnvLength:.",
                "./1:.:.:.:.:1.26335:3:4:6:cnvLength:Inherited"
            };

            ISample[] samples = cols.ToSamples(formatIndices, 1, false);

            Assert.Equal(4, samples.Length);

            Assert.Equal("0/1", samples[0].Genotype);
            Assert.Equal(new[] { 30, 20 }, samples[0].AlleleDepths);
            Assert.Equal(new[] { "-" }, samples[0].DiseaseAffectedStatuses);

            Assert.Equal("./1", samples[1].Genotype);
            Assert.Equal(3, samples[1].CopyNumber);
            Assert.True(samples[1].FailedFilter);

            Assert.Equal("./.", samples[2].Genotype);
            Assert.Equal(2, samples[2].CopyNumber);
            Assert.True(samples[2].FailedFilter);

            Assert.Equal("./1", samples[3].Genotype);
            Assert.Equal(3, samples[3].CopyNumber);
            Assert.True(samples[3].FailedFilter);
        }

        [Fact]
        public void ToSamples_TooFewVcfColumns_ReturnNull()
        {
            var formatIndices = new FormatIndices();

            string[] cols = {
                "chr1",
                "125068769",
                "DRAGEN:GAIN:125068770-125075279",
                "N",
                "<DUP>",
                ".",
                "SampleFT",
                "SVTYPE=CNV;END=125075279;REFLEN=6510"
            };

            ISample[] samples = cols.ToSamples(formatIndices, 1, false);
            Assert.Null(samples);
        }
    }
}