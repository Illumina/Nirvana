using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Genome;
using SAUtils.InputFileParsers.OneKGen;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class OneKGenTests
    {
        private readonly IDictionary<string, IChromosome> _refChromDict;

        public OneKGenTests()
        {
            _refChromDict = new Dictionary<string, IChromosome>
            {
                {"1",new Chromosome("chr1", "1",0) },
                {"4",new Chromosome("chr4", "4", 3) },
                {"X",new Chromosome("chrX", "X", 22) }
            };
            
        }

        private static string GetAlleleFrequency(string jsonString, string description)
        {
            var regexMatch = Regex.Match(jsonString, $"\"{description}\":([0|1]\\.?\\d+)?");
            return regexMatch.Success ? regexMatch.Groups[1].ToString() : null;
        }

        [Fact]
        public void AlleleFrequencyTest()
        {
            const string vcfLine =
                "1	10352	rs555500075	T	TA	100	PAS	AC=2191;AF=0.4375;AN=5008;NS=2504;DP=88915;EAS_AF=0.4306;AMR_AF=0.4107;AFR_AF=0.4788;EUR_AF=0.4264;SAS_AF=0.4192;AA=|||unknown(NO_COVERAGE); VT=INDEL;EAS_AN=1008;EAS_AC=434;EUR_AN=1006;EUR_AC=429;AFR_AN=1322;AFR_AC=633;AMR_AN=694;AMR_AC=285;SAS_AN=978;SAS_AC=410";
            var oneKGenReader = new OneKGenReader(null, ParserTestUtils.GetSequenceProvider(10352,"T",'C', _refChromDict));
            var oneKItem = oneKGenReader.ExtractItems(vcfLine).First().GetJsonString();

            Assert.Equal("0.4375", GetAlleleFrequency(oneKItem, "allAf"));
            Assert.Equal("0.47882", GetAlleleFrequency(oneKItem, "afrAf"));
            Assert.Equal("0.410663", GetAlleleFrequency(oneKItem, "amrAf"));
            Assert.Equal("0.430556", GetAlleleFrequency(oneKItem, "easAf"));
            Assert.Equal("0.426441", GetAlleleFrequency(oneKItem, "eurAf"));
            Assert.Equal("0.419223", GetAlleleFrequency(oneKItem, "sasAf"));
            Assert.DoesNotContain("ancestralAllele", oneKItem);
        }

        [Fact]
        public void MultiAltAlleleTest()
        {
            const string vcfLine =
                "1	15274	rs62636497	A	G,T	100	PASS	AC=1739,3210;AF=0.347244,0.640974;AN=5008;NS=2504;DP=23255;EAS_AF=0.4812,0.5188;AMR_AF=0.2752,0.7205;AFR_AF=0.323,0.6369;EUR_AF=0.2922,0.7078;SAS_AF=0.3497,0.6472;AA=g|||;VT=SNP;MULTI_ALLELIC;EAS_AN=1008;EAS_AC=485,523;EUR_AN=1006;EUR_AC=294,712;AFR_AN=1322;AFR_AC=427,842;AMR_AN=694;AMR_AC=191,500;SAS_AN=978;SAS_AC=342,633";

            var oneKGenReader = new OneKGenReader(null, ParserTestUtils.GetSequenceProvider(15274, "A", 'C', _refChromDict));
            var oneKGenItems = oneKGenReader.ExtractItems(vcfLine).ToList();

            Assert.Equal(2, oneKGenItems.Count);

            var json1 = oneKGenItems[0].GetJsonString();
            var json2 = oneKGenItems[1].GetJsonString();

            Assert.Equal("0.347244", GetAlleleFrequency(json1, "allAf"));
            Assert.Equal("0.322995", GetAlleleFrequency(json1, "afrAf"));
            Assert.Equal("0.275216", GetAlleleFrequency(json1, "amrAf"));
            Assert.Equal("0.481151", GetAlleleFrequency(json1, "easAf"));
            Assert.Equal("0.292247", GetAlleleFrequency(json1, "eurAf"));
            Assert.Equal("0.349693", GetAlleleFrequency(json1, "sasAf"));

            Assert.Equal("0.640974", GetAlleleFrequency(json2, "allAf"));
            Assert.Equal("0.636914", GetAlleleFrequency(json2, "afrAf"));
            Assert.Equal("0.720461", GetAlleleFrequency(json2, "amrAf"));
            Assert.Equal("0.518849", GetAlleleFrequency(json2, "easAf"));
            Assert.Equal("0.707753", GetAlleleFrequency(json2, "eurAf")); //double check this one: 0.7077535
            Assert.Equal("0.647239", GetAlleleFrequency(json2, "sasAf"));
        }

        [Fact]
        public void PrioritizingSymbolicAllele4Svs()
        {
            const string vcfLine =
                "X	101155257	rs373174489	GTGCAAAAGCTCTTTAGTTTAATTAGGTCTCAGCTATTTATCTTTGTTCTTAT	G	100	PASS	AN=3775;AC=1723;AF=0.456424;AA=;EAS_AN=764;EAS_AC=90;EAS_AF=0.1178;EUR_AN=766;EUR_AC=439;EUR_AF=0.5731;AFR_AN=1003;AFR_AC=839;AFR_AF=0.8365;AMR_AN=524;AMR_AC=180;AMR_AF=0.3435;SAS_AN=718;SAS_AC=175;SAS_AF=0.2437";

            var oneKGenReader = new OneKGenReader(null, ParserTestUtils.GetSequenceProvider(101155257, "GTGCAAAAGCTCTTTAGTTTAATTAGGTCTCAGCTATTTATCTTTGTTCTTAT", 'C', _refChromDict));
            var oneKItems = oneKGenReader.ExtractItems(vcfLine);
            var json1 = oneKItems.First().GetJsonString();
            Assert.Equal("0.456424", GetAlleleFrequency(json1, "allAf"));
            Assert.Equal("0.836491", GetAlleleFrequency(json1, "afrAf"));
            Assert.Equal("0.343511", GetAlleleFrequency(json1, "amrAf"));
            Assert.Equal("0.117801", GetAlleleFrequency(json1, "easAf"));
            Assert.Equal("0.573107", GetAlleleFrequency(json1, "eurAf"));
            Assert.Equal("0.243733", GetAlleleFrequency(json1, "sasAf"));

        }

        [Fact]
        public void MissingSubPopulationFrequencies()
        {
            var vcfLine =
                "1\t10616\trs376342519\tCCGCCGTTGCAAAGGCGCGCCG\tC\t100\tPASS\tAN=5008;AC=4973;AF=0.993011;AA=;EAS_AN=1008;EAS_AC=999;EAS_AF=0.9911;EUR_AN=1006;EUR_AC=1000;EUR_AF=0.994;AFR_AN=1322;AFR_AC=1308;AFR_AF=0.9894;AMR_AN=694;AMR_AC=691;AMR_AF=0.9957;SAS_AN=978;SAS_AC=975;SAS_AF=0.9969";

            var oneKGenReader = new OneKGenReader(null, ParserTestUtils.GetSequenceProvider(10616, "CCGCCGTTGCAAAGGCGCGCCG", 'C', _refChromDict));
            var items = oneKGenReader.ExtractItems(vcfLine).ToList();

            Assert.Single(items);
            Assert.Equal("\"allAf\":0.993011,\"afrAf\":0.98941,\"amrAf\":0.995677,\"easAf\":0.991071,\"eurAf\":0.994036,\"sasAf\":0.996933,\"allAn\":5008,\"afrAn\":1322,\"amrAn\":694,\"easAn\":1008,\"eurAn\":1006,\"sasAn\":978,\"allAc\":4973,\"afrAc\":1308,\"amrAc\":691,\"easAc\":999,\"eurAc\":1000,\"sasAc\":975", items[0].GetJsonString());

        }

        private static Stream GetOneKgSvStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##vcfFormat=4.2");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("1\t668630\tesv3584976\tG\t<CN2>\t100\tPASS\tAC=64;AF=0.0127796;AN=5008;CIEND=-150,150;CIPOS=-150,150;CS=DUP_delly;END=850204;NS=2504;SVTYPE=DUP;IMPRECISE;DP=22135;EAS_AF=0.0595;AMR_AF=0;AFR_AF=0.0015;EUR_AF=0.001;SAS_AF=0.001;VT=SV;EX_TARGET");
            writer.WriteLine("1\t713044\tesv3584977;esv3584978\tC\t<CN0>,<CN2>\t100\tPASS\tAC=3,206;AF=0.000599042,0.0411342;AN=5008;CS=DUP_gs;END=755966;NS=2504;SVTYPE=CNV;DP=20698;EAS_AF=0.001,0.0615;AMR_AF=0.0014,0.0259;AFR_AF=0,0.0303;EUR_AF=0.001,0.0417;SAS_AF=0,0.045;VT=SV;EX_TARGET");
            writer.WriteLine("1\t738570\tesv3584979\tG\t<CN0>\t100\tPASS\tAC=1;AF=0.000199681;AN=5008;CIEND=0,354;CIPOS=-348,0;CS=DEL_union;END=742020;NS=2504;SVTYPE=DEL;DP=19859;EAS_AF=0.001;AMR_AF=0;AFR_AF=0;EUR_AF=0;SAS_AF=0;VT=SV;EX_TARGET");
            writer.WriteLine("1\t645710\tesv3584975\tA\t<INS:ME:ALU>\t100\tPASS\tAC=35;AF=0.00698882;AN=5008;CS=ALU_umary;MEINFO=AluYa4_5,1,223,-;NS=2504;SVLEN=222;SVTYPE=ALU;TSD=null;DP=12290;EAS_AF=0.0069;AMR_AF=0.0072;AFR_AF=0;EUR_AF=0.0189;SAS_AF=0.0041;VT=SV");
            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void OnekGenSvReader()
        {
            using (var reader = new StreamReader(GetOneKgSvStream()))
            {
                var svReader = new OneKGenSvReader(reader, _refChromDict);

                var svItemList = svReader.GetItems().ToList();

                Assert.Equal(4, svItemList.Count);

                Assert.Equal("\"chromosome\":\"1\",\"begin\":668631,\"end\":850204,\"variantType\":\"copy_number_gain\",\"id\":\"esv3584976\",\"allAn\":5008,\"allAc\":64,\"allAf\":0.01278,\"afrAf\":0.0015,\"amrAf\":0,\"eurAf\":0.001,\"easAf\":0.0595,\"sasAf\":0.001", svItemList[0].GetJsonString());

                Assert.Equal("\"chromosome\":\"1\",\"begin\":713045,\"end\":755966,\"variantType\":\"copy_number_variation\",\"id\":\"esv3584977;esv3584978\",\"allAn\":5008,\"allAc\":209,\"allAf\":0.041733,\"afrAf\":0.0303,\"amrAf\":0.0273,\"eurAf\":0.0427,\"easAf\":0.0625,\"sasAf\":0.045", svItemList[1].GetJsonString());

                Assert.Equal("\"chromosome\":\"1\",\"begin\":738571,\"end\":742020,\"variantType\":\"copy_number_loss\",\"id\":\"esv3584979\",\"allAn\":5008,\"allAc\":1,\"allAf\":0.0002,\"afrAf\":0,\"amrAf\":0,\"eurAf\":0,\"easAf\":0.001,\"sasAf\":0", svItemList[2].GetJsonString());

                Assert.Equal("\"chromosome\":\"1\",\"begin\":645711,\"end\":645932,\"variantType\":\"mobile_element_insertion\",\"id\":\"esv3584975\",\"allAn\":5008,\"allAc\":35,\"allAf\":0.006989,\"afrAf\":0,\"amrAf\":0.0072,\"eurAf\":0.0189,\"easAf\":0.0069,\"sasAf\":0.0041", svItemList[3].GetJsonString());


            }

        }
    }
}
