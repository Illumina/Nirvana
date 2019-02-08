using System;
using System.Collections.Generic;
using Genome;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.DbSnp;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class DbSnpReaderTests
    {

        private readonly IDictionary<string, IChromosome> _refChromDict;
        private static readonly IChromosome Chr1 = new Chromosome("chr1", "1", 0);
        private static readonly IChromosome Chr4 = new Chromosome("chr4", "4", 3);
        private static readonly IChromosome Chr17 = new Chromosome("chr17", "17", 16);
        private static readonly IChromosome ChrX = new Chromosome("chrX", "X", 22);

        public DbSnpReaderTests()
        {
            _refChromDict = new Dictionary<string, IChromosome>
            {
                {"1",Chr1 },
                {"4",Chr4 },
                {"17",Chr17},
                {"X", ChrX}
            };
        }

        [Fact]
        public void MissingEntry()
        {
            const string vcfLine =
                "1	241369	rs11490246	C	T	.	.	RS=11490246;RSPOS=241369;dbSNPBuildID=120;SSR=0;SAO=0;VP=0x050000000005000126000100;WGT=1;VC=SNV;ASP;GNO;KGPhase3;CAF=0,1;COMMON=0";

            var dbsnpReader = new DbSnpReader(null, _refChromDict);
            var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine)[0];

            Assert.Equal(11490246, dbSnpEntry.RsId);
        }

        [Fact]
        public void MissingEntry2()
        {
            const string vcfLine =
                "17	828	rs62053745	T	C	.	.	RS=62053745;RSPOS=828;dbSNPBuildID=129;SSR=0;SAO=0;VP=0x050100080005140136000100;WGT=1;VC=SNV;SLO;INT;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.2576,0.7424;COMMON=1";

            var dbsnpReader = new DbSnpReader(null, _refChromDict);
            var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine)[0];

            Assert.Equal(62053745, dbSnpEntry.RsId);
        }

        [Fact]
        public void MissingDbsnpId()
        {
            const string vcfLine =
                "X	21505833	rs12395602	G	A,C,T	.	.	RS=12395602;RSPOS=21505833;dbSNPBuildID=120;SSR=0;SAO=0;VP=0x05010008000505051f000101;WGT=1;VC=SNV;SLO;INT;ASP;VLD;G5;HD;GNO;KGPhase1";

            var dbsnpReader = new DbSnpReader(null, _refChromDict);
            var dbSnpEntries = dbsnpReader.ExtractItem(vcfLine);

            Assert.Equal(3, dbSnpEntries.Count);
            Assert.Equal("A", dbSnpEntries[0].AltAllele);
            Assert.Equal(12395602, dbSnpEntries[0].RsId);
            Assert.Equal("C", dbSnpEntries[1].AltAllele);
            Assert.Equal(12395602, dbSnpEntries[1].RsId);
            Assert.Equal("T", dbSnpEntries[2].AltAllele);
            Assert.Equal(12395602, dbSnpEntries[2].RsId);
        }

        [Obsolete("We should not have skipped unit tests.")]
        [Fact(Skip = "redo test with AlleleFrequency object")]
        public void NoMinorAllele()
        {
            const string vcfLine = "17	828	rs62053745	T	C	.	.	RS=62053745;RSPOS=828;dbSNPBuildID=129;SSR=0;SAO=0;VP=0x050100080005140136000100;WGT=1;VC=SNV;SLO;INT;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=.,0.7424;COMMON=1";
            var dbsnpReader      = new DbSnpReader(null, _refChromDict);
            var dbSnpEntry       = dbsnpReader.ExtractItem(vcfLine)[0];

            Assert.Equal("C", dbSnpEntry.AltAllele);            
        }

        [Obsolete("We should not have skipped unit tests.")]
        [Fact(Skip = "redo test with AlleleFrequency object")]
        public void DisregardZeroFreq()
        {
            const string vcfLine = "1	241369	rs11490246	C	T	.	.	RS=11490246;RSPOS=241369;dbSNPBuildID=120;SSR=0;SAO=0;VP=0x050100000005000126000100;WGT=1;VC=SNV;SLO;ASP;GNO;KGPhase3;CAF=0,1;COMMON=0";
            var dbsnpReader      = new DbSnpReader(null, _refChromDict);
            var dbSnpEntry       = dbsnpReader.ExtractItem(vcfLine)[0];

            Assert.Equal("T", dbSnpEntry.AltAllele);            
        }

        [Fact]
        public void EqualityAndHash()
        {
            var dbsnpItem = new DbSnpItem(new Chromosome("chr1", "1", 0), 100, 101, "A", "C");
            var dbsnpHash = new HashSet<DbSnpItem> { dbsnpItem };

            Assert.Single(dbsnpHash);
            Assert.Contains(dbsnpItem, dbsnpHash);
        }
    }
}
