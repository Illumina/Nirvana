using System.Collections.Generic;
using Genome;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.DGV;
using Variants;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class DgvTests
    {
        private readonly IDictionary<string, IChromosome> _refChromDict;

        public DgvTests()
        {
            _refChromDict = new Dictionary<string, IChromosome>
            {
                {"1",new Chromosome("chr1", "1",0) },
                {"4",new Chromosome("chr4", "4", 3) }
            };
        }

        [Fact]
        public void ExtractDgvCnv()
        {
            const string dgvLine = "nsv482937	1	1	2300000	CNV	loss	Iafrate_et_al_2004	15286789	BAC aCGH,FISH			nssv2995976	M		39	0	1		ACAP3,AGRN,WASH7P	";

            var dgvItem = DgvReader.ExtractDgvItem(dgvLine, _refChromDict);
            var jsonString = dgvItem.GetJsonString();
            Assert.Equal("\"chromosome\":\"1\",\"begin\":1,\"end\":2300000,\"variantType\":\"copy_number_loss\",\"id\":\"nsv482937\",\"sampleSize\":39,\"observedLosses\":1,\"variantFreqAll\":0.02564", jsonString );
        }

        [Fact]
        public void ExtractDgvComplex()
        {
            const string dgvLine = "esv2421662	1	12841928	12971833	OTHER	complex	Altshuler_et_al_2010	20811451	SNP array			essv5038349,essv5012238	M		1184	20	70		HNRNPCL1,LOC649330,PRAMEF1,PRAMEF10,PRAMEF11,PRAMEF2,PRAMEF4	NA10838,NA10847";

            var dgvItem = DgvReader.ExtractDgvItem(dgvLine, _refChromDict);
            var jsonString = dgvItem.GetJsonString();
            Assert.Equal("\"chromosome\":\"1\",\"begin\":12841928,\"end\":12971833,\"variantType\":\"complex_structural_alteration\",\"id\":\"esv2421662\",\"sampleSize\":1184,\"observedGains\":20,\"observedLosses\":70,\"variantFreqAll\":0.07601", jsonString);
            
        }

        [Fact]
        public void EmptyObservedLossesAndGains()
        {
            const string dgvLine = "nsv161172	1	88190	89153	CNV	deletion	Mills_et_al_2006	16902084	Sequencing			nssv179750	M		24					";

            var dgvItem = DgvReader.ExtractDgvItem(dgvLine, _refChromDict);
            var jsonString = dgvItem.GetJsonString();
            Assert.Equal("\"chromosome\":\"1\",\"begin\":88190,\"end\":89153,\"variantType\":\"copy_number_loss\",\"id\":\"nsv161172\",\"sampleSize\":24", jsonString);
            //Assert.Equal("1", dgvInterval.Chromosome.EnsemblName);
            //Assert.Equal(88190, dgvInterval.Start);
            //Assert.Equal(89153, dgvInterval.End);
            //Assert.Equal("copy_number_loss", dgvInterval.VariantType.ToString());
            //Assert.Equal("dgv", dgvInterval.Source);
            //Assert.Equal("nsv161172", dgvInterval.StringValues["id"]);
            //Assert.Equal(24, dgvInterval.IntValues["sampleSize"]);
            //Assert.False(dgvInterval.IntValues.ContainsKey("observedGains"));
            //Assert.False(dgvInterval.IntValues.ContainsKey("observedLosses"));
            //Assert.False(dgvInterval.PopulationFrequencies.ContainsKey("variantFreqAll"));

        }

        [Fact]
        public void EqualityAndHash()
        {
            var dgvItem = new DgvItem("dgv101", new Chromosome("chr1", "1", 0), 100, 200, 123, 34, 32, VariantType.complex_structural_alteration);

            var dgvHash = new HashSet<DgvItem> { dgvItem };

            Assert.Single(dgvHash);
            Assert.Contains(dgvItem, dgvHash);
        }
    }
}