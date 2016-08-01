using System;
using System.Collections.Generic;
using System.IO;
using SAUtils.InputFileParsers.ClinVar;
using SAUtils.InputFileParsers.CustomAnnotation;
using SAUtils.InputFileParsers.DbSnp;
using SAUtils.InputFileParsers.EVS;
using SAUtils.InputFileParsers.ExAc;
using SAUtils.InputFileParsers.OneKGen;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using Xunit;
using System.Linq;

namespace UnitTests.SupplementaryAnnotations
{

    [Collection("Chromosome 1 collection")]
    public sealed class SaReadWriteTest : RandomFileBase
    {
        #region members

        private readonly DataSourceVersion _expectedDataSourceVersion;
        private readonly List<DataSourceVersion> _expectedDataSourceVersions;
        private readonly SupplementaryAnnotation _expectedAnnotation1;
        private readonly SupplementaryAnnotation _expectedAnnotation2;
        private readonly SupplementaryAnnotation _expectedAnnotation3;
        private readonly SupplementaryInterval _expectedInterval;
        private const string AltAllele = "T";
        private readonly string _randomPath;

        #endregion

        // constructor
        public SaReadWriteTest()
        {

            // create our expected data source versions
            _expectedDataSourceVersion = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);
            _expectedDataSourceVersions = new List<DataSourceVersion> { _expectedDataSourceVersion };

            // create our expected supplementary annotations
            var expectedAlleleSpecificAnnotation1 = new SupplementaryAnnotation.AlleleSpecificAnnotation
            {
                DbSnp = new List<long> { 1 }
            };

            _expectedAnnotation1 = new SupplementaryAnnotation(100)
            {
                AlleleSpecificAnnotations = { [AltAllele] = expectedAlleleSpecificAnnotation1 }
            };

            var expectedAlleleSpecificAnnotation2 = new SupplementaryAnnotation.AlleleSpecificAnnotation
            {
                DbSnp = new List<long> { 2 }
            };

            _expectedAnnotation2 = new SupplementaryAnnotation(101)
            {
                AlleleSpecificAnnotations = { [AltAllele] = expectedAlleleSpecificAnnotation2 }
            };

            var expectedAlleleSpecificAnnotation3 = new SupplementaryAnnotation.AlleleSpecificAnnotation
            {
                DbSnp = new List<long> { 3 }
            };

            _expectedAnnotation3 = new SupplementaryAnnotation(102)
            {
                AlleleSpecificAnnotations = { [AltAllele] = expectedAlleleSpecificAnnotation3 }
            };

            _expectedInterval = new SupplementaryInterval(1, 1000, "chr1", null, VariantType.copy_number_variation, null);

            _randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // write the supplementary annotation file
            WriteSupplementaryAnnotationFile(_randomPath);
        }


        private void WriteSupplementaryAnnotationFile(string filePath)
        {
            using (var writer = new SupplementaryAnnotationWriter(filePath, "chr1", _expectedDataSourceVersions))
            {
                writer.SetIntervalList(new List<SupplementaryInterval> { _expectedInterval });
                writer.Write(_expectedAnnotation1, _expectedAnnotation1.ReferencePosition);
                writer.Write(_expectedAnnotation2, _expectedAnnotation2.ReferencePosition);
                writer.Write(_expectedAnnotation3, _expectedAnnotation3.ReferencePosition);
            }
        }

        [Fact]
        public void EndOfFileTest()
        {
            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(_randomPath))
            {
                var observedJumpAnnotation = reader.GetAnnotation(_expectedAnnotation3.ReferencePosition + 1);
                Assert.Null(observedJumpAnnotation);
            }
        }

        [Fact]
        public void ReadAndWrite()
        {
            var header = SupplementaryAnnotationReader.GetHeader(_randomPath);

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(_randomPath))
            {
                var observedDataSourceVersions = header.DataSourceVersions;
                var refSeq = header.ReferenceSequenceName;
                var dataVersion = header.DataVersion;
                var creationTime = header.CreationTimeTicks;
                var genomeAssembly = header.GenomeAssembly;

                // check the data source versions
                Assert.Equal(observedDataSourceVersions.Count, 1);

                var observedDataSourceVersion = observedDataSourceVersions[0];
                Assert.Equal(_expectedDataSourceVersion.Name, observedDataSourceVersion.Name);
                Assert.Equal(_expectedDataSourceVersion.Version, observedDataSourceVersion.Version);
                Assert.Equal(_expectedDataSourceVersion.ReleaseDateTicks, observedDataSourceVersion.ReleaseDateTicks);
                Assert.NotNull(refSeq);
                Assert.Equal(SupplementaryAnnotationCommon.DataVersion, dataVersion);
                Assert.True(DateTime.MinValue.Ticks != creationTime);
                Assert.True(genomeAssembly == GenomeAssembly.Unknown);

                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(100);
                var observedAnnotation2 = reader.GetAnnotation(101);
                var observedAnnotation3 = reader.GetAnnotation(102);

                Assert.Equal(_expectedAnnotation1.AlleleSpecificAnnotations[AltAllele].DbSnp,
                    observedAnnotation1.AlleleSpecificAnnotations[AltAllele].DbSnp);
                Assert.Equal(_expectedAnnotation2.AlleleSpecificAnnotations[AltAllele].DbSnp,
                    observedAnnotation2.AlleleSpecificAnnotations[AltAllele].DbSnp);
                Assert.Equal(_expectedAnnotation3.AlleleSpecificAnnotations[AltAllele].DbSnp,
                    observedAnnotation3.AlleleSpecificAnnotations[AltAllele].DbSnp);

                // jump around the file
                var observedJumpAnnotation2 = reader.GetAnnotation(_expectedAnnotation2.ReferencePosition);
                var observedJumpAnnotation1 = reader.GetAnnotation(_expectedAnnotation1.ReferencePosition);
                var observedJumpAnnotation3 = reader.GetAnnotation(_expectedAnnotation3.ReferencePosition);

                Assert.Equal(_expectedAnnotation1.AlleleSpecificAnnotations[AltAllele].DbSnp,
                    observedJumpAnnotation1.AlleleSpecificAnnotations[AltAllele].DbSnp);
                Assert.Equal(_expectedAnnotation2.AlleleSpecificAnnotations[AltAllele].DbSnp,
                    observedJumpAnnotation2.AlleleSpecificAnnotations[AltAllele].DbSnp);
                Assert.Equal(_expectedAnnotation3.AlleleSpecificAnnotations[AltAllele].DbSnp,
                    observedJumpAnnotation3.AlleleSpecificAnnotations[AltAllele].DbSnp);

                var observedInterval = reader.GetSupplementaryIntervals();
                Assert.Equal(_expectedInterval, observedInterval.First());
            }
        }

        [Fact]
        public void RwDbsnpGlobalAlleles()
        {

            //NIR-1262
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "147", DateTime.Parse("2016-07-26").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion };

            const string vcfLine1 =
                "2	141724543	rs112783784	A	C,T	.	.	RS=112783784;RSPOS=141724543;dbSNPBuildID=132;SSR=0;SAO=0;VP=0x050100080015140136000100;WGT=1;VC=SNV;SLO;INT;OTH;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.04113,0.9105,0.04832;COMMON=1";

            const string vcfLine2 =
                "2	141724543	rs4300776	A	C	.	.	RS=4300776;RSPOS=141724543;dbSNPBuildID=111;SSR=0;SAO=0;VP=0x050100080015000102000100;WGT=1;VC=SNV;SLO;INT;OTH;ASP;GNO;CAF=0.04113,0.9105;COMMON=1";

            var sa = new SupplementaryAnnotation(141724543);

            var dbsnpReader = new DbSnpReader();
            foreach (var dbSnpItem in dbsnpReader.ExtractItem(vcfLine1))
            {
                dbSnpItem.SetSupplementaryAnnotations(sa);
            }

            foreach (var dbSnpItem in dbsnpReader.ExtractItem(vcfLine2))
            {
                dbSnpItem.SetSupplementaryAnnotations(sa);
            }

            // write the supplementary annotation file
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                sa.FinalizePositionalAnnotations();
                writer.Write(sa, sa.ReferencePosition);
            }

            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation = reader.GetAnnotation(141724543);
                Assert.NotNull(observedAnnotation);

                Assert.Equal("C", observedAnnotation.GlobalMajorAllele);
                Assert.Equal("0.9105", observedAnnotation.GlobalMajorAlleleFrequency);

                Assert.Equal("T", observedAnnotation.GlobalMinorAllele);
                Assert.Equal("0.04832", observedAnnotation.GlobalMinorAlleleFrequency);

            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact]
        public void ReadAndWriteDbSnp1KgEvs()
        {
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var oneKGenVersion = new DataSourceVersion("1000 Genomes Project", "phase3_shapeit2_mvncall_integrated_v5.", DateTime.Parse("2013-05-02").Ticks);
            var evsDataSource = new DataSourceVersion("EVS", "V2", DateTime.Parse("2013-11-13").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, oneKGenVersion, evsDataSource };

            // create our expected supplementary annotations
            const string vcfLine1 = "1	69428	rs140739101	T	G	.	.	RS=140739101;RSPOS=69428;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050200000a05140026000100;WGT=1;VC=SNV;S3D;NSM;REF;ASP;VLD;KGPhase3;CAF=0.981,0.01897;COMMON=1";
            const string vcfLine2 = "1	69428	rs140739101	T	G	100	PASS	AC=95;AF=0.0189696;AN=5008;NS=2504;DP=17611;EAS_AF=0.003;AMR_AF=0.036;AFR_AF=0.0015;EUR_AF=0.0497;SAS_AF=0.0153;AA=.|||";
            const string vcfLine3 = "1	69428	rs140739101	T	G	.	PASS	BSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            const string altAllele = "G";
            var sa = new SupplementaryAnnotation(69428);

            var dbsnpReader = new DbSnpReader();
            var dbSnpItem = dbsnpReader.ExtractItem(vcfLine1)[0];
            dbSnpItem.SetSupplementaryAnnotations(sa);

            var oneKGenReader = new OneKGenReader(null);
            var oneKGenItem = oneKGenReader.ExtractItems(vcfLine2)[0];
            oneKGenItem.SetSupplementaryAnnotations(sa);

            var evsReader = new EvsReader(null);
            var evsItem = evsReader.ExtractItems(vcfLine3)[0];
            evsItem.SetSupplementaryAnnotations(sa);

            // the preceeding code has been unit tested in  MergeDbSnp1kpEvs()

            // write the supplementary annotation file
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(sa, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            var header = SupplementaryAnnotationReader.GetHeader(randomPath);

            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                var observedDataSourceVersions = header.DataSourceVersions;

                // check the data source versions
                Assert.Equal(observedDataSourceVersions.Count, 3);

                var observedDataSourceVersion = observedDataSourceVersions[0];
                Assert.Equal(dbSnpVersion.Name, observedDataSourceVersion.Name);
                Assert.Equal(dbSnpVersion.Version, observedDataSourceVersion.Version);
                Assert.Equal(dbSnpVersion.ReleaseDateTicks, observedDataSourceVersion.ReleaseDateTicks);

                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(69428);
                Assert.NotNull(observedAnnotation1);

                Assert.Equal(sa.AlleleSpecificAnnotations[altAllele].DbSnp, observedAnnotation1.AlleleSpecificAnnotations[altAllele].DbSnp);

                Assert.Equal(sa.AlleleSpecificAnnotations[altAllele].EvsAll, observedAnnotation1.AlleleSpecificAnnotations[altAllele].EvsAll);
                Assert.Equal(sa.AlleleSpecificAnnotations[altAllele].OneKgAllAc, observedAnnotation1.AlleleSpecificAnnotations[altAllele].OneKgAllAc);

                Assert.Equal(sa.AlleleSpecificAnnotations[altAllele].EvsCoverage, observedAnnotation1.AlleleSpecificAnnotations[altAllele].EvsCoverage);
                Assert.Equal(sa.AlleleSpecificAnnotations[altAllele].NumEvsSamples, observedAnnotation1.AlleleSpecificAnnotations[altAllele].NumEvsSamples);
            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact]
        public void MultiAlleleMergeDbSnp1KpEvsSaRw()
        {
            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var oneKGenVersion = new DataSourceVersion("1000 Genomes Project", "phase3_shapeit2_mvncall_integrated_v5.", DateTime.Parse("2013-05-02").Ticks);
            var evsDataSource = new DataSourceVersion("EVS", "V2", DateTime.Parse("2013-11-13").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, oneKGenVersion, evsDataSource };

            // create our expected supplementary annotations
            const string vcfLine1 = "1	1564952	rs112177324	TG	T	.	.	RS=112177324;RSPOS=1564953;dbSNPBuildID=132;SSR=0;SAO=0;VP=0x05010008000514013e000200;WGT=1;VC=DIV;SLO;INT;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.8468,0.1506;COMMON=1";
            const string vcfLine2 = "1	1564952	rs112177324	TG	TGG,T	100	PASS	AC=13,754;AF=0.00259585,0.150559;AN=5008;NS=2504;DP=8657;EAS_AF=0,0.0933;AMR_AF=0.0014,0.2046;AFR_AF=0.0091,0.0182;EUR_AF=0,0.3588;SAS_AF=0,0.136";
            const string vcfLine3 = "1	1564952	rs112177324	TG	TGG,T	.	PASS	BSNP=dbSNP_132;EA_AC=2,3039,4701;AA_AC=44,279,3231;TAC=46,3318,7932;MAF=39.2793,9.0884,29.7805;GTS=A1A1,A1A2,A1R,A2A2,A2R,RR;EA_GTC=0,1,1,707,1624,1538;AA_GTC=4,4,32,41,193,1503;GTC=4,5,33,748,1817,3041;DP=10;GL=MIB2;CP=0.8;CG=-0.0;AA=.;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_080875.2:intron,NM_080875.2:intron,NM_001170689.1:intron,NM_001170689.1:intron,NM_001170688.1:intron,NM_001170688.1:intron,NM_001170687.1:intron,NM_001170687.1:intron,NM_001170686.1:intron,NM_001170686.1:intron;HGVS_CDNA_VAR=NM_080875.2:c.2908+7del1,NM_080875.2:c.2908+6_2908+7insG,NM_001170689.1:c.2187-66del1,NM_001170689.1:c.2187-67_2187-66insG,NM_001170688.1:c.2713+7del1,NM_001170688.1:c.2713+6_2713+7insG,NM_001170687.1:c.2866+7del1,NM_001170687.1:c.2866+6_2866+7insG,NM_001170686.1:c.2896+7del1,NM_001170686.1:c.2896+6_28967insG;HGVS_PROTEIN_VAR=.,.,.,.,.,.,.,.,.,.;CDS_SIZES=NM_080875.2:3213,NM_080875.2:3213,NM_001170689.1:2262,NM_001170689.1:2262,NM_001170688.1:3018,NM_001170688.1:3018,NM_001170687.1:3171,NM_001170687.1:3171,NM_001170686.1:3201,NM_001170686.1:3201;GS=.,.,.,.,.,.,.,.,.,.;PH=.,.,.,.,.,.,.,.,.,.;EA_AGE=.;AA_AGE=.";

            var sa = new SupplementaryAnnotation(1564953);

            var dbsnpReader = new DbSnpReader();
            var dbSnpItem = dbsnpReader.ExtractItem(vcfLine1)[0];
            var additionalItems = new List<SupplementaryDataItem>
            {
                dbSnpItem.SetSupplementaryAnnotations(sa)
            };

            var oneKGenReader = new OneKGenReader(null);
            var oneKGenItem = oneKGenReader.ExtractItems(vcfLine2)[0];
            additionalItems.Add(oneKGenItem.SetSupplementaryAnnotations(sa));

            var evsReader = new EvsReader(null);
            var evsItemsList = evsReader.ExtractItems(vcfLine3);

            foreach (var evsItem in evsItemsList)
            {
                additionalItems.Add(evsItem.SetSupplementaryAnnotations(sa));
            }

            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }

            // write the supplementary annotation file
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(sa, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            var header = SupplementaryAnnotationReader.GetHeader(randomPath);

            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                var observedDataSourceVersions = header.DataSourceVersions;

                // check the data source versions
                Assert.Equal(observedDataSourceVersions.Count, 3);

                var observedDataSourceVersion = observedDataSourceVersions[0];
                Assert.Equal(dbSnpVersion.Name, observedDataSourceVersion.Name);
                Assert.Equal(dbSnpVersion.Version, observedDataSourceVersion.Version);
                Assert.Equal(dbSnpVersion.ReleaseDateTicks, observedDataSourceVersion.ReleaseDateTicks);

                // checking the global alleles
                Assert.Null(sa.GlobalMajorAllele);
                Assert.Null(sa.GlobalMajorAlleleFrequency);
                Assert.Null(sa.GlobalMinorAllele);
                Assert.Null(sa.GlobalMinorAlleleFrequency);

                // extract the three annotations
                var observedAnnotation = reader.GetAnnotation(1564953);
                Assert.NotNull(observedAnnotation);

                var expectedInsOneKgAllAc = sa.AlleleSpecificAnnotations["iG"].OneKgAllAc;
                var expectedDelOneKgAllAc = sa.AlleleSpecificAnnotations["1"].OneKgAllAc;
                string expectedInsEvsAfr = sa.AlleleSpecificAnnotations["iG"].EvsAfr;
                var expectedInsDbSnp = sa.AlleleSpecificAnnotations["iG"].DbSnp;

                Assert.Equal(expectedInsOneKgAllAc, observedAnnotation.AlleleSpecificAnnotations["iG"].OneKgAllAc);
                Assert.Equal(expectedDelOneKgAllAc, observedAnnotation.AlleleSpecificAnnotations["1"].OneKgAllAc);
                Assert.Equal(expectedInsEvsAfr, observedAnnotation.AlleleSpecificAnnotations["iG"].EvsAfr + "0");
                Assert.Equal(expectedInsDbSnp, observedAnnotation.AlleleSpecificAnnotations["iG"].DbSnp);
            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact]
        public void ReadWriteExacDbsnp()
        {
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var cosmicVersion = new DataSourceVersion("COSMIC", "GRCh37_v71", DateTime.Parse("2014-10-21").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, cosmicVersion };

            // create our expected supplementary annotations
            const string vcfLine1 = "2	48010488	rs1042821	G	A	.	.	RS=1042821;RSPOS=48010488;RV;dbSNPBuildID=86;SSR=0;SAO=1;VP=0x050168420a05150136100100;GENEINFO=MSH6:2956;WGT=1;VC=SNV;PM;PMC;SLO;NSM;REF;U5;R5;ASP;VLD;G5;GNO;KGPhase1;KGPhase3;LSD;CAF=0.7991,0.2009;COMMON=1";
            const string vcfLine2 =
                "2	48010488	rs1042821	G	A,C	14068898.15	PASS	AC=21019,1;AC_AFR=1700,0;AC_AMR=1015,1;AC_Adj=19510,1;AC_EAS=1973,0;AC_FIN=743,0;AC_Het=15722,1,0;AC_Hom=1894,0;AC_NFE=10593,0;AC_OTH=147,0;AC_SAS=3339,0;AF=0.178,8.487e-06;AN=117830;AN_AFR=6388;AN_AMR=9014;AN_Adj=91130;AN_EAS=6792;AN_FIN=5078;AN_NFE=48404;AN_OTH=664;AN_SAS=14790;BaseQRankSum=-4.850e-01;ClippingRankSum=-1.400e-01;DB;DP=1206681;FS=0.000;GQ_MEAN=129.86;GQ_STDDEV=221.88;Het_AFR=1322,0,0;Het_AMR=931,1,0;Het_EAS=1511,0,0;Het_FIN=665,0,0;Het_NFE=8585,0,0;Het_OTH=111,0,0;Het_SAS=2597,0,0;Hom_AFR=189,0;Hom_AMR=42,0;Hom_EAS=231,0;Hom_FIN=39,0;Hom_NFE=1004,0;Hom_OTH=18,0;Hom_SAS=371,0;InbreedingCoeff=0.0376;MQ=60.00;MQ0=0;MQRankSum=0.00;NCC=3737;POSITIVE_TRAIN_SITE;QD=17.46;ReadPosRankSum=0.181;VQSLOD=5.87;culprit=MQ;DP_HIST=3051|9435|11318|5521|9711|11342|4131|1270|615|404|328|266|264|262|196|186|126|115|97|277,133|968|2180|3402|3564|2815|1772|954|551|389|321|263|261|261|196|186|126|115|97|277,0|0|0|1|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0;GQ_HIST=949|2966|347|492|15135|1435|1335|854|421|526|590|416|13672|1951|445|462|255|174|211|16279,24|79|81|124|135|96|110|118|97|180|228|137|182|191|126|171|180|151|192|16229,0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|1";

            var sa = new SupplementaryAnnotation(48010488);
            var dbsnpReader = new DbSnpReader();
            var dbSnpItem1 = dbsnpReader.ExtractItem(vcfLine1)[0];
            dbSnpItem1.SetSupplementaryAnnotations(sa);

            var exacReader = new ExacReader(null);
            foreach (var exacItem in exacReader.ExtractItems(vcfLine2))
            {
                exacItem.SetSupplementaryAnnotations(sa);
            }


            // write the supplementary annotation file
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr2", expectedDataSourceVersions))
            {
                writer.Write(sa, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(48010488);
                Assert.NotNull(observedAnnotation1);

                // we want to make sure we are reading the values we have written
                Assert.Equal(sa.AlleleSpecificAnnotations["A"].DbSnp, observedAnnotation1.AlleleSpecificAnnotations["A"].DbSnp);
                Assert.Equal(sa.AlleleSpecificAnnotations["A"].ExacAllAn, observedAnnotation1.AlleleSpecificAnnotations["A"].ExacAllAn);

                Assert.Equal(sa.AlleleSpecificAnnotations["A"].ExacCoverage, observedAnnotation1.AlleleSpecificAnnotations["A"].ExacCoverage);
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["A"].ExacAllAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["A"].ExacAllAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["A"].ExacAfrAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["A"].ExacAfrAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["A"].ExacAmrAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["A"].ExacAmrAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["A"].ExacEasAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["A"].ExacEasAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["A"].ExacFinAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["A"].ExacFinAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["A"].ExacNfeAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["A"].ExacNfeAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["A"].ExacOthAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["A"].ExacOthAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["A"].ExacSasAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["A"].ExacSasAc));

                Assert.Equal(sa.AlleleSpecificAnnotations["C"].ExacCoverage, observedAnnotation1.AlleleSpecificAnnotations["C"].ExacCoverage);
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["C"].ExacAllAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["C"].ExacAllAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["C"].ExacAfrAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["C"].ExacAfrAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["C"].ExacAmrAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["C"].ExacAmrAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["C"].ExacEasAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["C"].ExacEasAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["C"].ExacFinAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["C"].ExacFinAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["C"].ExacNfeAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["C"].ExacNfeAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["C"].ExacOthAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["C"].ExacOthAc));
                Assert.Equal(Convert.ToDouble(sa.AlleleSpecificAnnotations["C"].ExacSasAc),
                    Convert.ToDouble(observedAnnotation1.AlleleSpecificAnnotations["C"].ExacSasAc));

            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }


        [Fact]
        public void ReadWriteDbSnpCosmic()
        {
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var cosmicVersion = new DataSourceVersion("COSMIC", "GRCh37_v71", DateTime.Parse("2014-10-21").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, cosmicVersion };

            // create our expected supplementary annotations
            const string vcfLine1 = "1	10228	rs143255646	TA	T	.	.	RS=143255646;RSPOS=10229;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000020005000002000200;WGT=1;VC=DIV;R5;ASP";

            var sa = new SupplementaryAnnotation(10229);
            var dbsnpReader = new DbSnpReader();
            var dbSnpItem1 = dbsnpReader.ExtractItem(vcfLine1)[0];
            var additionalItems = new List<SupplementaryDataItem>
            {
                dbSnpItem1.SetSupplementaryAnnotations(sa)
            };

            var cosmicItem1 = new CosmicItem("1", 10229, "COSM1000", "TA", "T", "TP53",
                new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy("", "carcinoma", "oesophagus") });
            var cosmicItem2 = new CosmicItem("1", 10229, "COSM1000", "TA", "T", "TP53",
                new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy("01", "carcinoma", "large_intestine") });

            additionalItems.Add(cosmicItem1.SetSupplementaryAnnotations(sa));
            additionalItems.Add(cosmicItem2.SetSupplementaryAnnotations(sa));

            sa = new SupplementaryAnnotation(10229);
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }

            Assert.Equal(1, sa.CosmicItems.Count);
            // the preceeding code has been unit tested in  MergeDbSnpCosmic()

            // write the supplementary annotation file
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(sa, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(10229);
                Assert.NotNull(observedAnnotation1);

                Assert.Equal(sa.AlleleSpecificAnnotations["1"].DbSnp, observedAnnotation1.AlleleSpecificAnnotations["1"].DbSnp);
                Assert.True(CosmicUtilities.ContainsId(sa, sa.CosmicItems[0].ID));
                Assert.Equal(1, observedAnnotation1.CosmicItems.Count);
            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact]
        public void ReadWriteDbSnpClinVar()
        {
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var clinVarVersion = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, clinVarVersion };

            const string vcfLine = "1	2160305	rs387907306	G	A,T	.	.	RS=387907306;RSPOS=2160305;dbSNPBuildID=137;SSR=0;SAO=0;VP=0x050060000a05000002110100;GENEINFO=SKI:6497;WGT=1;VC=SNV;PM;NSM;REF;ASP;LSD;OM;CLNALLE=1,2;CLNHGVS=NC_000001.10:g.2160305G>A,NC_000001.10:g.2160305G>T;CLNSRC=ClinVar|OMIM_Allelic_Variant,ClinVar|OMIM_Allelic_Variant;CLNORIGIN=1,1;CLNSRCID=NM_003036.3:c.100G>A|164780.0004,NM_003036.3:c.100G>T|164780.0005;CLNSIG=5,5;CLNDSDB=GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT,GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT;CLNDSDBID=NBK1277:C1321551:182212:ORPHA2462:83092002,NBK1277:C1321551:182212:ORPHA2462:83092002;CLNDBN=Shprintzen-Goldberg_syndrome,Shprintzen-Goldberg_syndrome;CLNREVSTAT=single,single;CLNACC=RCV000030819.24,RCV000030820.24";

            var dbsnpReader = new DbSnpReader();
            var dbSnpItems = dbsnpReader.ExtractItem(vcfLine);

            var sa = new SupplementaryAnnotation(2160305);

            foreach (var dbSnpItem in dbSnpItems)
            {
                dbSnpItem.SetSupplementaryAnnotations(sa);

            }
            var clinvarReader = new ClinVarReader(null);
            var clinVarItems = clinvarReader.ExtractClinVarItems(vcfLine);

            foreach (var clinVarItem in clinVarItems)
            {
                var sa1 = new SupplementaryAnnotation();
                clinVarItem.SetSupplementaryAnnotations(sa1);
                sa.MergeAnnotations(sa1);
            }

            // the above code was unit tested in MergeDbSnpClinVar()
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(sa, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(2160305);

                Assert.NotNull(observedAnnotation1);

                Assert.Equal(sa.AlleleSpecificAnnotations["A"].DbSnp, observedAnnotation1.AlleleSpecificAnnotations["A"].DbSnp);

                for (int i = 0; i < sa.ClinVarItems.Count; i++)
                {
                    Assert.Equal(sa.ClinVarItems[i].ID, observedAnnotation1.ClinVarItems[i].ID);
                    Assert.Equal(sa.ClinVarItems[i].Significance, observedAnnotation1.ClinVarItems[i].Significance);
                }
            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact]
        public void ReadWriteClinVarCitationEvaluation()
        {
            //NIR-1689
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var clinVarVersion = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { clinVarVersion };

            const string vcfLine = "1	2160305	rs387907306	G	A,T	.	.	RS=387907306;RSPOS=2160305;dbSNPBuildID=137;SSR=0;SAO=0;VP=0x050060000a05000002110100;GENEINFO=SKI:6497;WGT=1;VC=SNV;PM;NSM;REF;ASP;LSD;OM;CLNALLE=1,2;CLNHGVS=NC_000001.10:g.2160305G>A,NC_000001.10:g.2160305G>T;CLNSRC=ClinVar|OMIM_Allelic_Variant,ClinVar|OMIM_Allelic_Variant;CLNORIGIN=1,1;CLNSRCID=NM_003036.3:c.100G>A|164780.0004,NM_003036.3:c.100G>T|164780.0005;CLNSIG=5,5;CLNDSDB=GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT,GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT;CLNDSDBID=NBK1277:C1321551:182212:ORPHA2462:83092002,NBK1277:C1321551:182212:ORPHA2462:83092002;CLNDBN=Shprintzen-Goldberg_syndrome,Shprintzen-Goldberg_syndrome;CLNREVSTAT=single,single;CLNACC=RCV000030819.24,RCV000030820.24";


            var sa = new SupplementaryAnnotation(2160305);

            var clinvarReader = new ClinVarReader(null);
            var clinVarItems = clinvarReader.ExtractClinVarItems(vcfLine);

            //Last evaluated date is Feb 01, 2015 and Pubmed Ids are: 23023332, 23103230, 24736733
            clinVarItems[0].PubMedIds = new HashSet<long> { 23023332, 23103230, 24736733 };
            clinVarItems[0].LastEvaluatedDate = DateTime.Parse("Feb 01, 2015").Ticks;

            foreach (var clinVarItem in clinVarItems)
            {
                clinVarItem.SetSupplementaryAnnotations(sa);
            }

            // the above code was unit tested in MergeDbSnpClinVar()
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(sa, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedSa = reader.GetAnnotation(2160305);
                Assert.NotNull(observedSa);

                for (int i = 0; i < sa.ClinVarItems.Count; i++)
                {
                    Assert.Equal(sa.ClinVarItems[i].ID, observedSa.ClinVarItems[i].ID);
                    Assert.Equal(sa.ClinVarItems[i].Significance, observedSa.ClinVarItems[i].Significance);
                    Assert.Equal(sa.ClinVarItems[i].LastEvaluatedDate, observedSa.ClinVarItems[i].LastEvaluatedDate);
                    Assert.Equal(sa.ClinVarItems[i].PubmedIds, observedSa.ClinVarItems[i].PubmedIds);
                }

            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact]
        public void ReadWriteClinVar()
        {
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var clinVarVersion = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, clinVarVersion };

            // This is the case where Nirvana throws an error: Too many bytes in what should have been a 7 bit encoded Int32.
            const string vcfLine = "9	5073770	rs77375493	G	A,T	.	.	RS=77375493;RSPOS=5073770;dbSNPBuildID=131;SSR=0;SAO=3;VP=0x050260000a05000416110120;GENEINFO=JAK2:3717;WGT=1;VC=SNV;PM;S3D;NSM;REF;ASP;HD;KGPhase1;LSD;OM;CLNALLE=1,2;CLNHGVS=NC_000009.11:g.5073770G>A,NC_000009.11:g.5073770G>T;CLNSRC=ClinVar|OMIM_Allelic_Variant,ClinVar|OMIM_Allelic_Variant;CLNORIGIN=1,2;CLNSRCID=NM_004972.3:c.1849G>A|147796.0004,NM_004972.3:c.1849G>T|147796.0001;CLNSIG=5,255|255|255|255|255|255;CLNDSDB=MedGen:OMIM:Orphanet,MedGen:OMIM:Orphanet|MedGen:OMIM:Orphanet|GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT|.|MedGen:OMIM:Orphanet|MedGen:OMIM:Orphanet;CLNDSDBID=C3281125:614521:ORPHA3318,C0032463:263300:ORPHA729|C0001815:254450:ORPHA824|NBK47457:C0023467:601626:ORPHA519:91861009|.|C3281125:614521:ORPHA3318|C1851490:133100:ORPHA90042;CLNDBN=Thrombocythemia_3,Polycythemia_vera|Myelofibrosis|AML_-_Acute_myeloid_leukemia|Budd-Chiari_syndrome\x2c_susceptibility_to\x2c_somatic|Thrombocythemia_3|Familial_erythrocytosis\x2c_1;CLNREVSTAT=single,single|single|single|single|single|single;CLNACC=RCV000022629.24,RCV000015769.6|RCV000015770.6|RCV000015771.6|RCV000015772.66|RCV000022627.6|RCV000022628.6";

            var sa = new SupplementaryAnnotation(5073770);
            var clinvarReader = new ClinVarReader(null);
            var clinVarItems = clinvarReader.ExtractClinVarItems(vcfLine);

            foreach (var clinVarItem in clinVarItems)
            {
                sa = new SupplementaryAnnotation(5073770);
                clinVarItem.SetSupplementaryAnnotations(sa);
                sa.MergeAnnotations(sa);
            }

            // the above code was unit tested in MergeDbSnpClinVar()
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr9", expectedDataSourceVersions))
            {
                writer.Write(sa, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(5073770);

                Assert.NotNull(observedAnnotation1);

                for (int i = 0; i < sa.ClinVarItems.Count; i++)
                {
                    Assert.Equal(sa.ClinVarItems[i].ID, observedAnnotation1.ClinVarItems[i].ID);
                    Assert.Equal(sa.ClinVarItems[i].Significance, observedAnnotation1.ClinVarItems[i].Significance);
                }
            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact]
        public void ReadWriteWithSuppIntervals()
        {
            // NIR-1359
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var clinVarVersion = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, clinVarVersion };

            // This is the case where Nirvana throws an error: Too many bytes in what should have been a 7 bit encoded Int32.
            const string vcfLine = "9	5073770	rs77375493	G	A,T	.	.	RS=77375493;RSPOS=5073770;dbSNPBuildID=131;SSR=0;SAO=3;VP=0x050260000a05000416110120;GENEINFO=JAK2:3717;WGT=1;VC=SNV;PM;S3D;NSM;REF;ASP;HD;KGPhase1;LSD;OM;CLNALLE=1,2;CLNHGVS=NC_000009.11:g.5073770G>A,NC_000009.11:g.5073770G>T;CLNSRC=ClinVar|OMIM_Allelic_Variant,ClinVar|OMIM_Allelic_Variant;CLNORIGIN=1,2;CLNSRCID=NM_004972.3:c.1849G>A|147796.0004,NM_004972.3:c.1849G>T|147796.0001;CLNSIG=5,255|255|255|255|255|255;CLNDSDB=MedGen:OMIM:Orphanet,MedGen:OMIM:Orphanet|MedGen:OMIM:Orphanet|GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT|.|MedGen:OMIM:Orphanet|MedGen:OMIM:Orphanet;CLNDSDBID=C3281125:614521:ORPHA3318,C0032463:263300:ORPHA729|C0001815:254450:ORPHA824|NBK47457:C0023467:601626:ORPHA519:91861009|.|C3281125:614521:ORPHA3318|C1851490:133100:ORPHA90042;CLNDBN=Thrombocythemia_3,Polycythemia_vera|Myelofibrosis|AML_-_Acute_myeloid_leukemia|Budd-Chiari_syndrome\x2c_susceptibility_to\x2c_somatic|Thrombocythemia_3|Familial_erythrocytosis\x2c_1;CLNREVSTAT=single,single|single|single|single|single|single;CLNACC=RCV000022629.24,RCV000015769.6|RCV000015770.6|RCV000015771.6|RCV000015772.66|RCV000022627.6|RCV000022628.6";

            var sa = new SupplementaryAnnotation(5073770);
            var clinvarReader = new ClinVarReader(null);
            var clinVarItems = clinvarReader.ExtractClinVarItems(vcfLine);

            foreach (var clinVarItem in clinVarItems)
            {
                sa = new SupplementaryAnnotation(5073770);
                clinVarItem.SetSupplementaryAnnotations(sa);
                sa.MergeAnnotations(sa);
            }

            // adding a supplementary interval
            var intValues = new Dictionary<string, int>();
            var doubleValues = new Dictionary<string, double>();
            var freqValues = new Dictionary<string, double>();
            var stringValues = new Dictionary<string, string>();
            var boolValues = new List<string>();

            var suppInterval = new SupplementaryInterval(5073770, 5073970, "chr1", "<DUP>", VariantType.duplication, "ClinVar", intValues,
                doubleValues, freqValues, stringValues, boolValues);

            suppInterval.AddStringValue("ID", "RandomClin001");

            // the above code was unit tested in MergeDbSnpClinVar()
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr9", expectedDataSourceVersions))
            {
                writer.SetIntervalList(new List<SupplementaryInterval> { suppInterval });
                writer.Write(sa, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(5073770);

                Assert.NotNull(observedAnnotation1);

                for (int i = 0; i < sa.ClinVarItems.Count; i++)
                {
                    Assert.Equal(sa.ClinVarItems[i].ID, observedAnnotation1.ClinVarItems[i].ID);
                    Assert.Equal(sa.ClinVarItems[i].Significance, observedAnnotation1.ClinVarItems[i].Significance);
                }

                // read the stored intervals
                var suppIntervals = reader.GetSupplementaryIntervals().ToList();
                Assert.Equal(1, suppIntervals.Count);

                foreach (var interval in suppIntervals)
                {
                    Assert.Equal(5073770, interval.Start);
                    Assert.Equal(5073970, interval.End);
                    Assert.Equal("<DUP>", interval.AlternateAllele);
                    Assert.Equal("ClinVar", interval.Source);
                    Assert.Equal("duplication", interval.VariantType.ToString());

                    foreach (var keyValuePair in interval.StringValues)
                    {
                        if (keyValuePair.Key == "ID")
                            Assert.Equal("RandomClin001", keyValuePair.Value);
                        if (keyValuePair.Key == "vid")
                            Assert.Equal("1:5073770:5073970", keyValuePair.Value);
                    }
                }

            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact]
        public void ReadWriteCustomAnnotation()
        {
            string randomPath = GetRandomPath(true);

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var clinVarVersion = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, clinVarVersion };

            var customFile = new FileInfo(@"Resources\customCosmic.vcf");

            var customReader = new CustomAnnotationReader(customFile);

            // all items from this file should be of type cosmic.
            var customItems = customReader.ToList();

            var sa = new SupplementaryAnnotation(69224);
            foreach (var customItem in customItems)
            {
                // NOTE that the two custom items are for different position, but for the purpose of our test, this is not an issue.
                customItem.SetSupplementaryAnnotations(sa);
            }

            // the above code was unit tested in MergeDbSnpClinVar()
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(sa, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(69224);

                Assert.NotNull(observedAnnotation1);

                for (int i = 0; i < sa.CustomItems.Count; i++)
                {
                    Assert.Equal(sa.CustomItems[i].Id, observedAnnotation1.CustomItems[i].Id);
                    Assert.Equal(sa.CustomItems[i].AnnotationType, observedAnnotation1.CustomItems[i].AnnotationType);
                    Assert.Equal(sa.CustomItems[i].IsAlleleSpecific, observedAnnotation1.CustomItems[i].IsAlleleSpecific);
                    Assert.True(sa.CustomItems[i].StringFields.SequenceEqual(observedAnnotation1.CustomItems[i].StringFields));
                    if (sa.CustomItems[i].BooleanFields.Count > 0)
                        Assert.True(sa.CustomItems[i].BooleanFields.SequenceEqual(observedAnnotation1.CustomItems[i].BooleanFields));
                }
            }
        }

        [Fact]
        public void Utf8ClinVar()
        {
            // NIR-900
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var clinVarVersion = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, clinVarVersion };

            // This is the case where Nirvana throws an error: Too many bytes in what should have been a 7 bit encoded Int32.
            const string vcfLine = "1	225592188	rs387906416	TAGAAGA	CTTCTAG	.	.	RS=387906416;RSPOS=225592188;RV;dbSNPBuildID=137;SSR=0;SAO=0;VP=0x050060000605000002110800;GENEINFO=LBR:3930;WGT=1;VC=MNV;PM;NSN;REF;ASP;LSD;OM;CLNALLE=1;CLNHGVS=NC_000001.10:g.225592188_225592194delTAGAAGAinsCTTCTAG;CLNSRC=ClinVar|OMIM_Allelic_Variant;CLNORIGIN=1;CLNSRCID=NM_194442.2:c.1599_1605delTCTTCTAinsCTAGAAG|600024.0003;CLNSIG=5|5;CLNDSDB=MedGen:OMIM:Orphanet:SNOMED_CT|MedGen:OMIM:SNOMED_CT;CLNDSDBID=C1300226:215140:ORPHA1426:389261002|C0030779:169400:85559002;CLNDBN=Greenberg_dysplasia|Pelger-Huët_anomaly;CLNREVSTAT=single|single;CLNACC=RCV000010137.2|RCV000087262.2";

            var sa = new SupplementaryAnnotation(225592188);
            var clinvarReader = new ClinVarReader(null);
            var clinVarItems = clinvarReader.ExtractClinVarItems(vcfLine);

            foreach (var clinVarItem in clinVarItems)
            {
                sa = new SupplementaryAnnotation(225592188);
                clinVarItem.SetSupplementaryAnnotations(sa);
                sa.MergeAnnotations(sa);
            }

            // the above code was unit tested in MergeDbSnpClinVar()
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(sa, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(225592188);

                Assert.NotNull(observedAnnotation1);

                for (int i = 0; i < sa.ClinVarItems.Count; i++)
                {
                    Assert.Equal(sa.ClinVarItems[i].Phenotype, observedAnnotation1.ClinVarItems[i].Phenotype);
                }
            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }


        [Fact]
        public void ReadAndWriteExacWithMultipleAlleles()
        {
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var exacVersion = new DataSourceVersion("ExAC", "0.3.1", DateTime.Parse("2016-03-16").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { exacVersion };

            // create our expected supplementary annotations: note AN_adj is modified in this 
            const string vcfline =
                "19	3121452	.	TA	T,TAA	17262.47	AC_Adj0_Filter	AC=6,9;AC_AFR=0,0;AC_AMR=0,0;AC_Adj=0,0;AC_EAS=0,0;AC_FIN=0,0;AC_Het=0,0,0;AC_Hom=0,0;AC_NFE=0,0;AC_OTH=0,0;AC_SAS=0,0;AF=4.587e-03,6.881e-03;AN=1308;AN_AFR=0;AN_AMR=0;AN_Adj=3;AN_EAS=0;AN_FIN=0;AN_NFE=0;AN_OTH=0;AN_SAS=0;BaseQRankSum=0.437;DP=2838";

            var sa = new SupplementaryAnnotation(3121453);

            var exacReader = new ExacReader(null);
            var additionalItems = new List<SupplementaryDataItem>();
            foreach (var exacItem in exacReader.ExtractItems(vcfline))
            {
                var currentItem = exacItem.SetSupplementaryAnnotations(sa);
                additionalItems.Add(currentItem);
            }
            var currentSa = new SupplementaryAnnotation(3121453);
            foreach (var exacItem in additionalItems)
            {
                exacItem.SetSupplementaryAnnotations(currentSa);
            }

            // write the supplementary annotation file
            using (
                var writer = new SupplementaryAnnotationWriter(randomPath, "chr19",
                    expectedDataSourceVersions))
            {
                writer.Write(currentSa, currentSa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(3121453);

                Assert.NotNull(observedAnnotation1);

                // we want to make sure we are reading the values we have written
                Assert.Equal(currentSa.AlleleSpecificAnnotations["iA"].ExacAllAn, observedAnnotation1.AlleleSpecificAnnotations["iA"].ExacAllAn);

                Assert.Equal(currentSa.AlleleSpecificAnnotations["iA"].ExacCoverage, observedAnnotation1.AlleleSpecificAnnotations["iA"].ExacCoverage);
                Assert.Equal(currentSa.AlleleSpecificAnnotations["iA"].ExacAllAc, observedAnnotation1.AlleleSpecificAnnotations["iA"].ExacAllAc);
                Assert.NotNull(observedAnnotation1.AlleleSpecificAnnotations["iA"].ExacAllAc);
                Assert.Null(observedAnnotation1.AlleleSpecificAnnotations["iA"].ExacFinAc);
                Assert.Null(observedAnnotation1.AlleleSpecificAnnotations["iA"].ExacFinAn);

                // we want to make sure we are reading the values we have written
                Assert.Equal(currentSa.AlleleSpecificAnnotations["1"].ExacAllAn, observedAnnotation1.AlleleSpecificAnnotations["1"].ExacAllAn);

                Assert.Equal(currentSa.AlleleSpecificAnnotations["1"].ExacCoverage, observedAnnotation1.AlleleSpecificAnnotations["1"].ExacCoverage);
                Assert.Equal(currentSa.AlleleSpecificAnnotations["1"].ExacAllAc, observedAnnotation1.AlleleSpecificAnnotations["1"].ExacAllAc);
                Assert.NotNull(observedAnnotation1.AlleleSpecificAnnotations["1"].ExacAllAc);
                Assert.Null(observedAnnotation1.AlleleSpecificAnnotations["1"].ExacFinAc);
                Assert.Null(observedAnnotation1.AlleleSpecificAnnotations["1"].ExacFinAn);


            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }
    }
}
