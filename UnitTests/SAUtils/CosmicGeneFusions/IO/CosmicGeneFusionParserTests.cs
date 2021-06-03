using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SAUtils.CosmicGeneFusions.Conversion;
using SAUtils.CosmicGeneFusions.IO;
using Xunit;

namespace UnitTests.SAUtils.CosmicGeneFusions.IO
{
    public sealed class CosmicGeneFusionParserTests
    {
        [Fact]
        public void Parse_ExpectedResults()
        {
            var lines = new List<string>
            {
                "SAMPLE_ID	SAMPLE_NAME	PRIMARY_SITE	SITE_SUBTYPE_1	SITE_SUBTYPE_2	SITE_SUBTYPE_3	PRIMARY_HISTOLOGY	HISTOLOGY_SUBTYPE_1	HISTOLOGY_SUBTYPE_2	HISTOLOGY_SUBTYPE_3	FUSION_ID	TRANSLOCATION_NAME	5'_CHROMOSOME	5'_STRAND	5'_GENE_ID	5'_GENE_NAME	5'_LAST_OBSERVED_EXON	5'_GENOME_START_FROM	5'_GENOME_START_TO	5'_GENOME_STOP_FROM	5'_GENOME_STOP_TO	3'_CHROMOSOME	3'_STRAND	3'_GENE_ID	3'_GENE_NAME	3'_FIRST_OBSERVED_EXON	3'_GENOME_START_FROM	3'_GENOME_START_TO	3'_GENOME_STOP_FROM	3'_GENOME_STOP_TO	FUSION_TYPE	PUBMED_PMID",
                "749711	HCC1187	breast	NS	NS	NS	carcinoma	ductal_carcinoma	NS	NS	665	ENST00000360863.10(RGS22):r.1_3555_ENST00000369518.1(SYCP1):r.2100_3452	8	-	197199	RGS22	22	99981937	99981937	100106116	100106116	1	+	212470	SYCP1_ENST00000369518	24	114944339	114944339	114995367	114995367	Inferred Breakpoint	20033038",
                "749711	HCC1187	breast	NS	NS	NS	carcinoma	ductal_carcinoma	NS	NS	665	ENST00000360863.10(RGS22):r.1_3555_ENST00000369518.1(SYCP1):r.2100_3452	8	-	197199	RGS22	22	99981937	99981937	100106116	100106116	1	+	212470	SYCP1_ENST00000369518	24	114944339	114944339	114995367	114995367	Observed mRNA	20033038",
                "749712	HCC1395	breast	NS	NS	NS	carcinoma	ductal_carcinoma	NS	NS	667	ENST00000395686.7(ERO1A):r.1_658_ENST00000395631.6(FERMT2):r.744_3369	14	-	282967	ERO1A	5	52671795	52671795	52695705	52695705	14	-	268960	FERMT2_ENST00000395631	5	52857268	52857268	52881469	52881469	Inferred Breakpoint	20033038"
            };

            using var    ms     = new MemoryStream();
            StreamReader reader = GetCosmicTestData(ms, lines);

            Dictionary<int, HashSet<RawCosmicGeneFusion>> actualFusionIdToEntries = CosmicGeneFusionParser.Parse(reader);
            Assert.Equal(2, actualFusionIdToEntries.Count);

            HashSet<RawCosmicGeneFusion> geneFusions = actualFusionIdToEntries[665];
            Assert.NotNull(geneFusions);
            Assert.Single(geneFusions);

            RawCosmicGeneFusion actualFusion = geneFusions.First();
            Assert.Equal(749711,                                                                    actualFusion.SampleId);
            Assert.Equal(665,                                                                       actualFusion.FusionId);
            Assert.Equal("breast",                                                                  actualFusion.PrimarySite);
            Assert.Equal("NS",                                                                      actualFusion.SiteSubtype1);
            Assert.Equal("carcinoma",                                                               actualFusion.PrimaryHistology);
            Assert.Equal("ductal carcinoma",                                                        actualFusion.HistologySubtype1);
            Assert.Equal("ENST00000360863.10(RGS22):r.1_3555_ENST00000369518.1(SYCP1):r.2100_3452", actualFusion.HgvsNotation);
            Assert.Equal(20033038,                                                                  actualFusion.PubMedId);

            geneFusions = actualFusionIdToEntries[667];
            Assert.NotNull(geneFusions);
            Assert.Single(geneFusions);

            actualFusion = geneFusions.First();
            Assert.Equal(749712,                                                                  actualFusion.SampleId);
            Assert.Equal(667,                                                                     actualFusion.FusionId);
            Assert.Equal("breast",                                                                actualFusion.PrimarySite);
            Assert.Equal("NS",                                                                    actualFusion.SiteSubtype1);
            Assert.Equal("carcinoma",                                                             actualFusion.PrimaryHistology);
            Assert.Equal("ductal carcinoma",                                                      actualFusion.HistologySubtype1);
            Assert.Equal("ENST00000395686.7(ERO1A):r.1_658_ENST00000395631.6(FERMT2):r.744_3369", actualFusion.HgvsNotation);
            Assert.Equal(20033038,                                                                actualFusion.PubMedId);
        }

        [Fact]
        public void Parse_IncorrectColumnCount_ThrowException()
        {
            var lines = new List<string>
            {
                "SAMPLE_ID	SAMPLE_NAME	PRIMARY_SITE	SITE_SUBTYPE_1	SITE_SUBTYPE_2	SITE_SUBTYPE_3	PRIMARY_HISTOLOGY	HISTOLOGY_SUBTYPE_1	HISTOLOGY_SUBTYPE_2	HISTOLOGY_SUBTYPE_3	FUSION_ID	TRANSLOCATION_NAME	5'_CHROMOSOME	5'_STRAND	5'_GENE_ID	5'_GENE_NAME	5'_LAST_OBSERVED_EXON	5'_GENOME_START_FROM	5'_GENOME_START_TO	5'_GENOME_STOP_FROM	5'_GENOME_STOP_TO	3'_CHROMOSOME	3'_STRAND	3'_GENE_ID	3'_GENE_NAME	3'_FIRST_OBSERVED_EXON	3'_GENOME_START_FROM	3'_GENOME_START_TO	3'_GENOME_STOP_FROM	3'_GENOME_STOP_TO	FUSION_TYPE	PUBMED_PMID",
                "749711	HCC1187"
            };

            using var    ms     = new MemoryStream();
            StreamReader reader = GetCosmicTestData(ms, lines);

            Assert.Throws<InvalidDataException>(delegate { CosmicGeneFusionParser.Parse(reader); });
        }

        private static StreamReader GetCosmicTestData(Stream stream, List<string> lines)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                foreach (string line in lines) writer.WriteLine(line);
            }

            stream.Position = 0;

            return new StreamReader(stream);
        }

        [Fact]
        public void RemoveUnderlines_ExpectedResults()
        {
            const string input          = "spindle_epithelial_tumour_with_thymus_like_differentiation";
            const string expectedResult = "spindle epithelial tumour with thymus like differentiation";
            string       actualResult   = CosmicGeneFusionParser.RemoveUnderlines(input);
            Assert.Equal(expectedResult, actualResult);
        }
    }
}