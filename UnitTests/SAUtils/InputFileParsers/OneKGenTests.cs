using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Genome;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.OneKGen;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class OneKGenTests
    {
        private const string VcfLine1 = "1	10352	rs555500075	T	TA	100	PAS	AC=2191;AF=0.4375;AN=5008;NS=2504;DP=88915;EAS_AF=0.4306;AMR_AF=0.4107;AFR_AF=0.4788;EUR_AF=0.4264;SAS_AF=0.4192;AA=|||unknown(NO_COVERAGE); VT=INDEL;EAS_AN=1008;EAS_AC=434;EUR_AN=1006;EUR_AC=429;AFR_AN=1322;AFR_AC=633;AMR_AN=694;AMR_AC=285;SAS_AN=978;SAS_AC=410";
        private const string VcfLine2 = "1	15274	rs62636497	A	G,T	100	PASS	AC=1739,3210;AF=0.347244,0.640974;AN=5008;NS=2504;DP=23255;EAS_AF=0.4812,0.5188;AMR_AF=0.2752,0.7205;AFR_AF=0.323,0.6369;EUR_AF=0.2922,0.7078;SAS_AF=0.3497,0.6472;AA=g|||;VT=SNP;MULTI_ALLELIC;EAS_AN=1008;EAS_AC=485,523;EUR_AN=1006;EUR_AC=294,712;AFR_AN=1322;AFR_AC=427,842;AMR_AN=694;AMR_AC=191,500;SAS_AN=978;SAS_AC=342,633";

        private readonly OneKGenReader _oneKGenReader;
        private readonly IDictionary<string, IChromosome> _refChromDict;

        public OneKGenTests()
        {
            _refChromDict = new Dictionary<string, IChromosome>
            {
                {"1",new Chromosome("chr1", "1",0) },
                {"4",new Chromosome("chr4", "4", 3) },
                {"X",new Chromosome("chrX", "X", 22) }
            };
            _oneKGenReader = new OneKGenReader(_refChromDict);
        }

        private static string GetAlleleFrequency(string jsonString, string description)
        {
            var regexMatch = Regex.Match(jsonString, $"\"{description}\":([0|1]\\.?\\d+)?");
            return regexMatch.Success ? regexMatch.Groups[1].ToString() : null;
        }

        [Fact]
        public void AlleleFrequencyTest()
        {
            var oneKItem = _oneKGenReader.ExtractItems(VcfLine1)[0].GetJsonString();

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
            var oneKGenItems = _oneKGenReader.ExtractItems(VcfLine2);

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

            var oneKItems = _oneKGenReader.ExtractItems(vcfLine);
            var json1 = oneKItems[0].GetJsonString();
            Assert.Equal("0.456424", GetAlleleFrequency(json1, "allAf"));
            Assert.Equal("0.836491", GetAlleleFrequency(json1, "afrAf"));
            Assert.Equal("0.343511", GetAlleleFrequency(json1, "amrAf"));
            Assert.Equal("0.117801", GetAlleleFrequency(json1, "easAf"));
            Assert.Equal("0.573107", GetAlleleFrequency(json1, "eurAf"));
            Assert.Equal("0.243733", GetAlleleFrequency(json1, "sasAf"));

        }

        [Fact]
        public void HashCode()
        {
            var chr1 = new Chromosome("chr1", "1", 0);
            var onekItem = new OneKGenItem(chr1, 100, "rs1001", "A", "C", "a", null, null, null, null, null, null, null, null, null, null, null, null, null, 0, null, null, null, null, null, 100, 0, 0);

            var onekHash = new HashSet<OneKGenItem> { onekItem };

            Assert.Single(onekHash);
            Assert.Contains(onekItem, onekHash);
        }

        [Fact]
        public void MissingSubPopulationFrequencies()
        {
            var vcfLine =
                "1\t10616\trs376342519\tCCGCCGTTGCAAAGGCGCGCCG\tC\t100\tPASS\tAN=5008;AC=4973;AF=0.993011;AA=;EAS_AN=1008;EAS_AC=999;EAS_AF=0.9911;EUR_AN=1006;EUR_AC=1000;EUR_AF=0.994;AFR_AN=1322;AFR_AC=1308;AFR_AF=0.9894;AMR_AN=694;AMR_AC=691;AMR_AF=0.9957;SAS_AN=978;SAS_AC=975;SAS_AF=0.9969";

            var items = _oneKGenReader.ExtractItems(vcfLine).ToList();

            Assert.Single(items);
            Assert.Equal("\"allAf\":0.993011,\"afrAf\":0.98941,\"amrAf\":0.995677,\"easAf\":0.991071,\"eurAf\":0.994036,\"sasAf\":0.996933,\"allAn\":5008,\"afrAn\":1322,\"amrAn\":694,\"easAn\":1008,\"eurAn\":1006,\"sasAn\":978,\"allAc\":4973,\"afrAc\":1308,\"amrAc\":691,\"easAc\":999,\"eurAc\":1000,\"sasAc\":975", items[0].GetJsonString());

        }

        [Fact]
        public void OnekGenSvReader()
        {
            var inputFile = Resources.InputFiles("1000G_SVs.tsv");

            var svReader = new OneKGenSvReader(inputFile, _refChromDict);

            var svItemList = svReader.GetItems().ToList();


            Assert.Equal("\"chromosome\":\"1\",\"begin\":668631,\"end\":850204,\"variantType\":\"copy_number_gain\",\"id\":\"esv3584976\",\"variantFreqAll\":0.02396,\"variantFreqAfr\":0.00303,\"variantFreqEas\":0.11111,\"variantFreqEur\":0.00199,\"variantFreqSas\":0.00204,\"sampleSize\":2504,\"sampleSizeAfr\":661,\"sampleSizeAmr\":347,\"sampleSizeEas\":504,\"sampleSizeEur\":503,\"sampleSizeSas\":489,\"observedGains\":60", svItemList[0].GetJsonString());
            
            //checking out the next item that should be a copy number variant (both loss and gain)
            Assert.Equal("\"chromosome\":\"1\",\"begin\":713045,\"end\":755966,\"variantType\":\"copy_number_variation\",\"id\":\"esv3584977;esv3584978\",\"variantFreqAll\":0.08187,\"variantFreqAfr\":0.06051,\"variantFreqAmr\":0.05476,\"variantFreqEas\":0.11905,\"variantFreqEur\":0.08549,\"variantFreqSas\":0.08793,\"sampleSize\":2504,\"sampleSizeAfr\":661,\"sampleSizeAmr\":347,\"sampleSizeEas\":504,\"sampleSizeEur\":503,\"sampleSizeSas\":489,\"observedGains\":202,\"observedLosses\":3", svItemList[1].GetJsonString());
            
            //next one is a del (copy_number_loss)
            Assert.Equal("\"chromosome\":\"1\",\"begin\":738571,\"end\":742020,\"variantType\":\"copy_number_loss\",\"id\":\"esv3584979\",\"variantFreqAll\":0.0004,\"variantFreqEas\":0.00198,\"sampleSize\":2504,\"sampleSizeAfr\":661,\"sampleSizeAmr\":347,\"sampleSizeEas\":504,\"sampleSizeEur\":503,\"sampleSizeSas\":489,\"observedLosses\":1", svItemList[2].GetJsonString());
            
        }
    }
}
