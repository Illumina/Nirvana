using System.Collections.Generic;
using MitoHeteroplasmy;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Pools;
using Variants;
using Vcf;
using Vcf.Sample;
using Xunit;
using static UnitTests.Vcf.Samples.TestUtilities;

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
            var sample = SampleFieldExtractor.ExtractSample("0/1:5:338,1:339:0.00295:30:-7.3191:0.0314:0,0,0,1,0,0,17,1,129,21,148,22:3.366:0.000", formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);

            Assert.Equal("0/1", sample.Genotype);
            Assert.Equal(5, sample.GenotypeQuality);
            Assert.Equal(new[] { 338, 1 }, sample.AlleleDepths);
            Assert.Equal(339, sample.TotalDepth);
            Assert.Equal(new[] { 0.00295 }, sample.VariantFrequencies);
            Assert.Equal(3.366f, sample.ArtifactAdjustedQualityScore);
            Assert.Equal(0.000f, sample.LikelihoodRatioQualityScore);
        }

        [Fact]
        public void ExtractSample_DragenSomatic_AsExpected()
        {
            var formatIndices = new FormatIndices();
            formatIndices.Set("GT:SQ:AD:AF:F1R2:F2R1:DP:SB:MB:PS");
            var sample = SampleFieldExtractor.ExtractSample("0|1:3.96:33,8:0.195:13,6:20,2:41:17,16,4,4:13,20,4,4:534234", formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);

            Assert.Equal("0|1", sample.Genotype);
            Assert.Equal(3.96, sample.SomaticQuality);
            Assert.Equal(new[] { 33, 8 }, sample.AlleleDepths);
            Assert.Equal(41, sample.TotalDepth);
            Assert.Equal(new[] { 8 / 41.0 }, sample.VariantFrequencies);
        }

        [Fact]
        public void ExtractSample_DragenCNV_AsExpected()
        {
            var formatIndices = new FormatIndices();
            formatIndices.Set("GT:CN:MCN");
            var sample = SampleFieldExtractor.ExtractSample("0|1:3:1", formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);

            Assert.Equal("0|1", sample.Genotype);
            Assert.Equal(3, sample.CopyNumber);
            Assert.Equal(1, sample.MinorHaplotypeCopyNumber);
        }

        [Fact]
        public void ExtractSample_Custom_format()
        {
            var formatIndices = new FormatIndices(new HashSet<string>(){"CUST"});
            formatIndices.Set("GT:CN:MCN:CUST");
            var sample = SampleFieldExtractor.ExtractSample("0|1:3:1:4.5", formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);

            Assert.Equal("0|1", sample.Genotype);
            Assert.Equal(3,     sample.CopyNumber);
            Assert.Equal(1,     sample.MinorHaplotypeCopyNumber);
            Assert.NotNull(sample.CustomFields);
            Assert.Contains("\"CUST\":\"4.5\"", sample.CustomFields.ToString());
        }
        
        [Fact]
        public void ExtractSample_Custom_format_empty()
        {
            var formatIndices = new FormatIndices(new HashSet<string>(){"CUST"});
            formatIndices.Set("GT:CN:MCN");
            var sample = SampleFieldExtractor.ExtractSample("0|1:3:1", formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);

            Assert.Equal("0|1", sample.Genotype);
            Assert.Equal(3,     sample.CopyNumber);
            Assert.Equal(1,     sample.MinorHaplotypeCopyNumber);
            Assert.NotNull(sample.CustomFields);
            Assert.True(sample.CustomFields.IsEmpty());
        }

        [Theory]
        [InlineData("GT:CN:MCN:CNQ:MCNQ:CNF:MCNF:SD:MAF:BC:AS", "1/2:2:0:1000:1000:2.03102:0.000203:248.8:0.0001:1493:1137", 1493)]
        [InlineData("GT:CN:MCN:CNQ:MCNQ:CNF:MCNF:SD:MAF:BC:AS", "1/2:3:0:1000:1000:3.02612:0.000303:370.7:0.0001:8765:9070", 8765)]
        public void ExtractSample_DragenCNV_MCN_LOH_BC(string formatField, string sampleField, int binCount)
        {
            var formatIndices = new FormatIndices();
            formatIndices.Set(formatField);
            var sample = SampleFieldExtractor.ExtractSample(sampleField, formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);

            Assert.True(sample.IsLossOfHeterozygosity);
            Assert.Equal(binCount, sample.BinCount);
        }

        [Fact]
        public void ExtractSample_ExpansionHunter()
        {
            var formatIndices = new FormatIndices();
            formatIndices.Set("GT:SO:REPCN:REPCI:ADSP:ADFL:ADIR:LC");
            var sample = SampleFieldExtractor.ExtractSample("1/1:SPANNING/SPANNING:15/15:15-15/15-15:22/22:23/23:0/0:38.270270", formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);

            Assert.Equal("1/1", sample.Genotype);
            Assert.Equal(new[] { 15, 15 }, sample.RepeatUnitCounts);
        }

        [Fact]
        public void ExtractSample_EmptySampleColumn_ReturnEmptySample()
        {
            var formatIndices = new FormatIndices();
            var sample = SampleFieldExtractor.ExtractSample(null, formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);
            Assert.True(sample.IsEmpty);
        }

        [Fact]
        public void ExtractSample_DotInSampleColumn_ReturnEmptySample()
        {
            var formatIndices = new FormatIndices();
            var sample = SampleFieldExtractor.ExtractSample(".", formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);
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

            ISample[] samples = cols.ToSamples(formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);

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
        public void ToSamples_Custom()
        {
            var formatIndices = new FormatIndices(new HashSet<string>(){"CF"});

            string[] cols = {
                "chr1",
                "125068769",
                "DRAGEN:GAIN:125068770-125075279",
                "N",
                "<DUP>",
                ".",
                "SampleFT",
                "SVTYPE=CNV;END=125075279;REFLEN=6510",
                "GT:AD:DST:RPL:LC:SM:CN:BC:QS:CF:FT:DN",
                "0/1:30,20:-:35.8981:45.810811:.:.:.:.:4.5",
                "./1:.:.:.:.:1.24763:3:4:5:1.2:cnvLength:.",
                "./.:.:.:.:.:1.17879:2:4:8:2.3:cnvLength:.",
                "./1:.:.:.:.:1.26335:3:4:6:3.4:cnvLength:Inherited"
            };

            ISample[] samples = cols.ToSamples(formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);

            Assert.Equal(4, samples.Length);

            Assert.NotNull(samples[0].CustomFields);
            Assert.Contains("\"CF\":\"4.5\"", samples[0].CustomFields.ToString());
            
            Assert.NotNull(samples[1].CustomFields);
            Assert.Contains("\"CF\":\"1.2\"", samples[1].CustomFields.ToString());

            Assert.NotNull(samples[2].CustomFields);
            Assert.Contains("\"CF\":\"2.3\"", samples[2].CustomFields.ToString());

            Assert.NotNull(samples[3].CustomFields);
            Assert.Contains("\"CF\":\"3.4\"", samples[3].CustomFields.ToString());

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

            ISample[] samples = cols.ToSamples(formatIndices, GetSimplePositionUsingAlleleNum(1), null, null);
            Assert.Null(samples);
        }

        [Fact]
        public void ExtractSample_MitoHeteroplasmy_AsExpected()
        {
            var position = 1;
            var provider = new MitoHeteroplasmyProvider();
            provider.Add(position, "C", new[] { 0.123, 0.200, 0.301 }, new[] { 1, 2, 4 });
            provider.Add(position, "G", new[] { 0.101, 0.201 }, new[] { 1, 2 });

            
            var simplePosition = new SimplePosition(ChromosomeUtilities.ChrM, 1, "A", new[] { "C", "T"});
            IVariant[] variants =
            {
                VariantPool.Get(ChromosomeUtilities.ChrM, position, position, "A", "C", VariantType.SNV,
                    null, false, false, false, null, AnnotationBehavior.SmallVariants, false),
                VariantPool.Get(ChromosomeUtilities.ChrM, position, position, "A", "T", VariantType.SNV,
                    null, false, false, false, null, AnnotationBehavior.SmallVariants, false)
            };
            
            var formatIndices = new FormatIndices();
            formatIndices.Set("GT:SQ:AD:AF:F1R2:F2R1:DP:SB:MB:PS");
            var sample = SampleFieldExtractor.ExtractSample("1|2:3.96:0,15,85:0.195:13,6:20,2:100:17,16,4,4:13,20,4,4:534234", formatIndices, simplePosition,variants, provider);

            Assert.Equal(new[] { 15 / 100.0, 85 / 100.0 }, sample.VariantFrequencies);
            Assert.Equal(new[] { "14.29", "null" }, sample.HeteroplasmyPercentile);
            
            VariantPool.Return((Variant)variants[0]);
            VariantPool.Return((Variant)variants[1]);
        }
    }
}