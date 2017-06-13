using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.DbSnp;
using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.SaUtilsTests.InputFileParsers
{
    [Collection("ChromosomeRenamer")]
    public sealed class DbSnpReaderTests
    {
        private static readonly Stream TestDbSnpStream = ResourceUtilities.GetReadStream(Resources.TopPath("TestDbSnpParser.vcf"));

        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public DbSnpReaderTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        private static IEnumerable<DbSnpItem> CreateTruthDbSnpItemSequence()
        {
            yield return new DbSnpItem("1", 820164, 74632680, "A", 0, "G", 0);
            yield return new DbSnpItem("1", 820181, 191755837, "C", 0.9995, "T", 0.0004591);
            yield return new DbSnpItem("4", 78820304, 112709112, "C", 0.9913, "A", 0.008724);
            yield return new DbSnpItem("4", 78820304, 112709112, "C", 0.9913, "T", 0.008724);
        }

        [Fact]
        public void TestDbSnpReader()
        {
            var dbSnpReader = new DbSnpReader(TestDbSnpStream, _renamer);
            Assert.True(dbSnpReader.SequenceEqual(CreateTruthDbSnpItemSequence()));
        }

        [Fact]
        public void MissingEntry()
        {
            const string vcfLine =
                "1	241369	rs11490246	C	T	.	.	RS=11490246;RSPOS=241369;dbSNPBuildID=120;SSR=0;SAO=0;VP=0x050000000005000126000100;WGT=1;VC=SNV;ASP;GNO;KGPhase3;CAF=0,1;COMMON=0";

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine)[0];

            Assert.Equal(11490246, dbSnpEntry.RsId);
            Assert.Equal(1, dbSnpEntry.AltAlleleFreq);
        }

        [Fact(Skip = "new SA")]
        public void EqualFrequencies()
        {
            //// NIR-942
            //const string vcfLine =
            //    "1	1242707	rs2274262	A	G	.	.	RS=2274262;RSPOS=1242707;RV;dbSNPBuildID=100;SSR=0;SAO=0;VP=0x0501004a000507013e000100;WGT=1;VC=SNV;SLO;U5;INT;R5;ASP;VLD;G5A;G5;GNO;KGPhase1;KGPhase3;CAF=0.5,0.5;COMMON=1";

            //var dbsnpReader = new DbSnpReader(_renamer);
            //var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine)[0];

            //var sa = new SupplementaryAnnotationPosition(1242707);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //dbSnpEntry.SetSupplementaryAnnotations(saCreator);

            //saCreator.FinalizePositionalAnnotations();

            //Assert.Equal("A", sa.GlobalMajorAllele);
            //Assert.Equal("G", sa.GlobalMinorAllele);
        }

        [Fact(Skip = "new SA")]
        public void RefGlobalMajor()
        {
            //// NIR-942
            //const string vcfLine =
            //    "1	1242707	rs2274262	A	G,T	.	.	RS=2274262;RSPOS=1242707;RV;dbSNPBuildID=100;SSR=0;SAO=0;VP=0x0501004a000507013e000100;WGT=1;VC=SNV;SLO;U5;INT;R5;ASP;VLD;G5A;G5;GNO;KGPhase1;KGPhase3;CAF=0.4,0.4,0.2;COMMON=1";

            //var dbsnpReader = new DbSnpReader(_renamer);
            //var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine)[0];

            //var sa = new SupplementaryAnnotationPosition(1242707);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //dbSnpEntry.SetSupplementaryAnnotations(saCreator);

            //saCreator.FinalizePositionalAnnotations();

            //Assert.Equal("A", sa.GlobalMajorAllele);
            //Assert.Equal("G", sa.GlobalMinorAllele);
        }


        [Fact(Skip = "new SA")]
        public void RefGlobalMinor()
        {
            //// NIR-942
            //const string vcfLine =
            //    "1	1242707	rs2274262	A	G,T	.	.	RS=2274262;RSPOS=1242707;RV;dbSNPBuildID=100;SSR=0;SAO=0;VP=0x0501004a000507013e000100;WGT=1;VC=SNV;SLO;U5;INT;R5;ASP;VLD;G5A;G5;GNO;KGPhase1;KGPhase3;CAF=0.2,0.2,0.6;COMMON=1";

            //var dbsnpReader = new DbSnpReader(_renamer);
            //var sa = new SupplementaryAnnotationPosition(1242707);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //foreach (var dbSnpEntry in dbsnpReader.ExtractItem(vcfLine))
            //{
            //    dbSnpEntry.SetSupplementaryAnnotations(saCreator);
            //}

            //saCreator.FinalizePositionalAnnotations();

            //Assert.Equal("T", sa.GlobalMajorAllele);
            //Assert.Equal("G", sa.GlobalMinorAllele);
        }

        [Fact(Skip = "new SA")]
        public void ArbitraryGlobalAlleles()
        {
            //// NIR-942
            //const string vcfLine =
            //    "1	1242707	rs2274262	A	G,T	.	.	RS=2274262;RSPOS=1242707;RV;dbSNPBuildID=100;SSR=0;SAO=0;VP=0x0501004a000507013e000100;WGT=1;VC=SNV;SLO;U5;INT;R5;ASP;VLD;G5A;G5;GNO;KGPhase1;KGPhase3;CAF=0.2,0.4,0.4;COMMON=1";

            //var dbsnpReader = new DbSnpReader(_renamer);
            //var sa = new SupplementaryAnnotationPosition(1242707);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //foreach (var dbSnpEntry in dbsnpReader.ExtractItem(vcfLine))
            //{
            //    dbSnpEntry.SetSupplementaryAnnotations(saCreator);
            //}

            //saCreator.FinalizePositionalAnnotations();

            //Assert.Equal("G", sa.GlobalMajorAllele);
            //Assert.Equal("T", sa.GlobalMinorAllele);
        }

        [Fact]
        public void MissingEntry2()
        {
            const string vcfLine =
                "17	828	rs62053745	T	C	.	.	RS=62053745;RSPOS=828;dbSNPBuildID=129;SSR=0;SAO=0;VP=0x050100080005140136000100;WGT=1;VC=SNV;SLO;INT;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.2576,0.7424;COMMON=1";

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine)[0];

            Assert.Equal(62053745, dbSnpEntry.RsId);
        }


        [Fact(Skip = "new SA")]
        public void GlobalMajorTest()
        {
            //const string vcfLine =
            //    "17	828	rs62053745	T	C	.	.	RS=62053745;RSPOS=828;dbSNPBuildID=129;SSR=0;SAO=0;VP=0x050100080005140136000100;WGT=1;VC=SNV;SLO;INT;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.2576,0.7424;COMMON=1";

            //var dbsnpReader = new DbSnpReader(_renamer);
            //var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine)[0];

            //var sa = new SupplementaryAnnotationPosition(1242707);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //dbSnpEntry.SetSupplementaryAnnotations(saCreator);

            //saCreator.FinalizePositionalAnnotations();

            //Assert.Equal("C", sa.GlobalMajorAllele);
            //Assert.Equal("T", sa.GlobalMinorAllele);

            //Assert.Equal("0.7424", sa.GlobalMajorAlleleFrequency);
            //Assert.Equal("0.2576", sa.GlobalMinorAlleleFrequency);
        }

        [Fact(Skip = "new SA")]
        public void MissingDbsnpId()
        {
            //// refactorSA. Annotation for C is missing in the database. have to debug that.

            //const string vcfLine =
            //    "X	21505833	rs12395602	G	A,C,T	.	.	RS=12395602;RSPOS=21505833;dbSNPBuildID=120;SSR=0;SAO=0;VP=0x05010008000505051f000101;WGT=1;VC=SNV;SLO;INT;ASP;VLD;G5;HD;GNO;KGPhase1";

            //var dbsnpReader = new DbSnpReader(_renamer);
            //var dbSnpEntries = dbsnpReader.ExtractItem(vcfLine);

            //var sa = new SupplementaryAnnotationPosition(21505833);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //foreach (var dbSnpEntry in dbSnpEntries)
            //{
            //    dbSnpEntry.SetSupplementaryAnnotations(saCreator);
            //}

            //saCreator.FinalizePositionalAnnotations();

            //var dbSnpA =
            //    sa.AlleleSpecificAnnotations["A"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as
            //        DbSnpAnnotation;
            //Assert.NotNull(dbSnpA);

            //var dbSnpC =
            //    sa.AlleleSpecificAnnotations["C"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as
            //        DbSnpAnnotation;
            //Assert.NotNull(dbSnpC);

            //var dbSnpT =
            //    sa.AlleleSpecificAnnotations["T"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as
            //        DbSnpAnnotation;
            //Assert.NotNull(dbSnpT);

            //Assert.Equal(12395602, dbSnpA.DbSnp[0]);
            //Assert.Equal(12395602, dbSnpC.DbSnp[0]);
            //Assert.Equal(12395602, dbSnpT.DbSnp[0]);
        }

        [Fact(Skip = "new SA")]
        public void NoMinorAllele()
        {
            //const string vcfLine =
            //    "17	828	rs62053745	T	C	.	.	RS=62053745;RSPOS=828;dbSNPBuildID=129;SSR=0;SAO=0;VP=0x050100080005140136000100;WGT=1;VC=SNV;SLO;INT;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=.,0.7424;COMMON=1";

            //var dbsnpReader = new DbSnpReader(_renamer);
            //var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine)[0];

            //var sa = new SupplementaryAnnotationPosition(828);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //dbSnpEntry.SetSupplementaryAnnotations(saCreator);

            //saCreator.FinalizePositionalAnnotations();

            //Assert.Equal("C", sa.GlobalMajorAllele);
            //Assert.Equal("0.7424", sa.GlobalMajorAlleleFrequency);
            //Assert.Null(sa.GlobalMinorAllele);
            //Assert.Null(sa.GlobalMinorAlleleFrequency);
        }

        [Fact(Skip = "new SA")]
        public void DisregardZeroFreq()
        {
            //const string vcfLine =
            //    "1	241369	rs11490246	C	T	.	.	RS=11490246;RSPOS=241369;dbSNPBuildID=120;SSR=0;SAO=0;VP=0x050100000005000126000100;WGT=1;VC=SNV;SLO;ASP;GNO;KGPhase3;CAF=0,1;COMMON=0";
            //var dbsnpReader = new DbSnpReader(_renamer);
            //var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine)[0];

            //var sa = new SupplementaryAnnotationPosition(828);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //dbSnpEntry.SetSupplementaryAnnotations(saCreator);

            //saCreator.FinalizePositionalAnnotations();

            //Assert.Equal("T", sa.GlobalMajorAllele);
            //Assert.Equal("1", sa.GlobalMajorAlleleFrequency);
            //Assert.Null(sa.GlobalMinorAllele);
            //Assert.Null(sa.GlobalMinorAlleleFrequency);
        }

        [Fact(Skip = "new SA")]
        public void NoMinorAllele1()
        {
            //const string vcfLine =
            //    "17	828	rs62053745	T	C	.	.	RS=62053745;RSPOS=828;dbSNPBuildID=129;SSR=0;SAO=0;VP=0x050100080005140136000100;WGT=1;VC=SNV;SLO;INT;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.7424,.;COMMON=1";

            //var dbsnpReader = new DbSnpReader(_renamer);
            //var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine)[0];

            //var sa = new SupplementaryAnnotationPosition(828);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //dbSnpEntry.SetSupplementaryAnnotations(saCreator);

            //saCreator.FinalizePositionalAnnotations();

            //Assert.Equal("T", sa.GlobalMajorAllele);
            //Assert.Equal("0.7424", sa.GlobalMajorAlleleFrequency);
            //Assert.Null(sa.GlobalMinorAllele);
            //Assert.Null(sa.GlobalMinorAlleleFrequency);
        }

        [Fact]
        public void EqualityAndHash()
        {
            var dbsnpItem = new DbSnpItem("chr1", 100, 101, "A", 0, "C", 0);

            var dbsnpHash = new HashSet<DbSnpItem> { dbsnpItem };

            Assert.Equal(1, dbsnpHash.Count);
            Assert.True(dbsnpHash.Contains(dbsnpItem));
        }
    }
}
