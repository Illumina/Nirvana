using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.InputFileParsers.ClinVar;
using SAUtils.InputFileParsers.CustomAnnotation;
using SAUtils.InputFileParsers.DbSnp;
using SAUtils.InputFileParsers.EVS;
using SAUtils.InputFileParsers.ExAc;
using SAUtils.InputFileParsers.OneKGen;
using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.SupplementaryAnnotations
{
    [Collection("ChromosomeRenamer")]
    public sealed class SaReadWriteTests : RandomFileBase
    {
        private readonly DataSourceVersion _expectedDataSourceVersion;
        private readonly List<DataSourceVersion> _expectedDataSourceVersions;
        private readonly SupplementaryAnnotationPosition _expectedAnnotation1;
        private readonly SupplementaryAnnotationPosition _expectedAnnotation2;
        private readonly SupplementaryAnnotationPosition _expectedAnnotation3;
        private readonly SupplementaryInterval _expectedInterval;
        private const string AltAllele = "T";
        private readonly string _randomPath;

        private readonly ChromosomeRenamer _renamer;
        private readonly ICompressedSequence _sequence;
        private readonly CompressedSequenceReader _reader;

        /// <summary>
        /// constructor
        /// </summary>
        public SaReadWriteTests(ChromosomeRenamerFixture fixture)
        {
            _renamer  = fixture.Renamer;
            _sequence = fixture.Sequence;
            _reader   = fixture.Reader;

            // create our expected data source versions
            _expectedDataSourceVersion  = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);
            _expectedDataSourceVersions = new List<DataSourceVersion> { _expectedDataSourceVersion };

            // create our expected supplementary annotations
            var dbSnp1 = new DbSnpAnnotation
            {
                DbSnp = new List<long> { 1 }
            };

            _expectedAnnotation1 = new SupplementaryAnnotationPosition(100);
            new SupplementaryPositionCreator(_expectedAnnotation1).AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, AltAllele, dbSnp1);

            var dbSnp2 = new DbSnpAnnotation
            {
                DbSnp = new List<long> { 2 }
            };

            _expectedAnnotation2 = new SupplementaryAnnotationPosition(101);
            new SupplementaryPositionCreator(_expectedAnnotation2).AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, AltAllele, dbSnp2);

            var dbSnp3 = new DbSnpAnnotation
            {
                DbSnp = new List<long> { 3 }
            };

            _expectedAnnotation3 = new SupplementaryAnnotationPosition(102);
            new SupplementaryPositionCreator(_expectedAnnotation3).AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, AltAllele, dbSnp3);

            _expectedInterval = new SupplementaryInterval(1, 1000, "chr1", null, VariantType.copy_number_variation, null, _renamer);

            _randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // write the supplementary annotation file
            WriteSupplementaryAnnotationFile(_randomPath);
        }

        private void WriteSupplementaryAnnotationFile(string filePath)
        {
            using (var writer = new SupplementaryAnnotationWriter(filePath, "chr1", _expectedDataSourceVersions))
            {
                writer.SetIntervalList(new List<SupplementaryInterval> { _expectedInterval });
                writer.Write(new SupplementaryPositionCreator(_expectedAnnotation1), _expectedAnnotation1.ReferencePosition);
                writer.Write(new SupplementaryPositionCreator(_expectedAnnotation2), _expectedAnnotation2.ReferencePosition);
                writer.Write(new SupplementaryPositionCreator(_expectedAnnotation3), _expectedAnnotation3.ReferencePosition);
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
            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(_randomPath))
            {
                var observedDataSourceVersions = reader.Header.DataSourceVersions;
                var refSeq                     = reader.Header.ReferenceSequenceName;
                var dataVersion                = reader.Header.DataVersion;
                var creationTime               = reader.Header.CreationTimeTicks;
                var genomeAssembly             = reader.Header.GenomeAssembly;

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


                var expDbsnp1 =
                    ((DbSnpAnnotation)
                        _expectedAnnotation1.AlleleSpecificAnnotations[AltAllele].Annotations[
                            DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)]).DbSnp;
                var expDbsnp2 =
                    ((DbSnpAnnotation)
                        _expectedAnnotation2.AlleleSpecificAnnotations[AltAllele].Annotations[
                            DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)]).DbSnp;
                var expDbsnp3 =
                    ((DbSnpAnnotation)
                        _expectedAnnotation3.AlleleSpecificAnnotations[AltAllele].Annotations[
                            DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)]).DbSnp;

                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(100) as SupplementaryAnnotationPosition;
                var observedAnnotation2 = reader.GetAnnotation(101) as SupplementaryAnnotationPosition;
                var observedAnnotation3 = reader.GetAnnotation(102) as SupplementaryAnnotationPosition;

                var obsDbsnp1 =
                    ((DbSnpAnnotation)
                        observedAnnotation1.AlleleSpecificAnnotations[AltAllele].Annotations[
                            DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)]).DbSnp;
                var obsDbsnp2 =
                    ((DbSnpAnnotation)
                        observedAnnotation2.AlleleSpecificAnnotations[AltAllele].Annotations[
                            DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)]).DbSnp;
                var obsDbsnp3 =
                    ((DbSnpAnnotation)
                        observedAnnotation3.AlleleSpecificAnnotations[AltAllele].Annotations[
                            DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)]).DbSnp;


                Assert.Equal(expDbsnp1, obsDbsnp1);
                Assert.Equal(expDbsnp2, obsDbsnp2);
                Assert.Equal(expDbsnp3, obsDbsnp3);

                // jump around the file
                var observedJumpAnnotation2 = reader.GetAnnotation(_expectedAnnotation2.ReferencePosition) as SupplementaryAnnotationPosition;
                var observedJumpAnnotation1 = reader.GetAnnotation(_expectedAnnotation1.ReferencePosition) as SupplementaryAnnotationPosition;
                var observedJumpAnnotation3 = reader.GetAnnotation(_expectedAnnotation3.ReferencePosition) as SupplementaryAnnotationPosition;
                var obsJumpDbsnp1 =
                    ((DbSnpAnnotation)
                        observedJumpAnnotation1.AlleleSpecificAnnotations[AltAllele].Annotations[
                            DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)]).DbSnp;
                var obsJumpDbsnp2 =
                    ((DbSnpAnnotation)
                        observedJumpAnnotation2.AlleleSpecificAnnotations[AltAllele].Annotations[
                            DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)]).DbSnp;
                var obsJumpDbsnp3 =
                    ((DbSnpAnnotation)
                        observedJumpAnnotation3.AlleleSpecificAnnotations[AltAllele].Annotations[
                            DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)]).DbSnp;


                Assert.Equal(expDbsnp1, obsJumpDbsnp1);
                Assert.Equal(expDbsnp2, obsJumpDbsnp2);
                Assert.Equal(expDbsnp3, obsJumpDbsnp3);

                var observedInterval = reader.GetSupplementaryIntervals(_renamer);
                Assert.Equal(_expectedInterval, observedInterval.First());
            }
        }

        [Fact]
        public void RwDbsnpGlobalAlleles()
        {

            //NIR-1262
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "147", DateTime.Parse("2016-07-26").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion };

            const string vcfLine1 =
                "2	141724543	rs112783784	A	C,T	.	.	RS=112783784;RSPOS=141724543;dbSNPBuildID=132;SSR=0;SAO=0;VP=0x050100080015140136000100;WGT=1;VC=SNV;SLO;INT;OTH;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.04113,0.9105,0.04832;COMMON=1";

            const string vcfLine2 =
                "2	141724543	rs4300776	A	C	.	.	RS=4300776;RSPOS=141724543;dbSNPBuildID=111;SSR=0;SAO=0;VP=0x050100080015000102000100;WGT=1;VC=SNV;SLO;INT;OTH;ASP;GNO;CAF=0.04113,0.9105;COMMON=1";

            var sa = new SupplementaryAnnotationPosition(141724543);
            var saCreator = new SupplementaryPositionCreator(sa);

            var dbsnpReader = new DbSnpReader(_renamer);
            foreach (var dbSnpItem in dbsnpReader.ExtractItem(vcfLine1))
            {
                dbSnpItem.SetSupplementaryAnnotations(saCreator);
            }

            foreach (var dbSnpItem in dbsnpReader.ExtractItem(vcfLine2))
            {
                dbSnpItem.SetSupplementaryAnnotations(saCreator);
            }

            // write the supplementary annotation file
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                saCreator.FinalizePositionalAnnotations();
                writer.Write(saCreator, sa.ReferencePosition);
            }

            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation = reader.GetAnnotation(141724543) as SupplementaryAnnotationPosition;
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
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

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
            var sa = new SupplementaryAnnotationPosition(69428);
            var saCreator = new SupplementaryPositionCreator(sa);

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItem = dbsnpReader.ExtractItem(vcfLine1)[0];
            dbSnpItem.SetSupplementaryAnnotations(saCreator);

            var oneKGenReader = new OneKGenReader(_renamer);
            var oneKGenItem = oneKGenReader.ExtractItems(vcfLine2)[0];
            oneKGenItem.SetSupplementaryAnnotations(saCreator);

            var evsReader = new EvsReader(_renamer);
            var evsItem = evsReader.ExtractItems(vcfLine3)[0];
            evsItem.SetSupplementaryAnnotations(saCreator);

            // the preceeding code has been unit tested in  MergeDbSnp1kpEvs()

            // write the supplementary annotation file
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(saCreator, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                var observedDataSourceVersions = reader.Header.DataSourceVersions;

                // check the data source versions
                Assert.Equal(observedDataSourceVersions.Count, 3);

                var observedDataSourceVersion = observedDataSourceVersions[0];
                Assert.Equal(dbSnpVersion.Name, observedDataSourceVersion.Name);
                Assert.Equal(dbSnpVersion.Version, observedDataSourceVersion.Version);
                Assert.Equal(dbSnpVersion.ReleaseDateTicks, observedDataSourceVersion.ReleaseDateTicks);

                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(69428) as SupplementaryAnnotationPosition;
                Assert.NotNull(observedAnnotation1);

                var expDbSnp =
                    sa.AlleleSpecificAnnotations[altAllele].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)]
                        as DbSnpAnnotation;
                Assert.NotNull(expDbSnp);

                var expOneKg =
                    sa.AlleleSpecificAnnotations[altAllele].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)]
                        as OneKGenAnnotation;
                Assert.NotNull(expOneKg);

                var expEvs =
                    sa.AlleleSpecificAnnotations[altAllele].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)]
                        as EvsAnnotation;
                Assert.NotNull(expEvs);

                var obsDbSnp = observedAnnotation1.AlleleSpecificAnnotations[altAllele].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)]
                        as DbSnpAnnotation;
                Assert.NotNull(obsDbSnp);

                var obsOneKg = observedAnnotation1.AlleleSpecificAnnotations[altAllele].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)]
                        as OneKGenAnnotation;
                Assert.NotNull(obsOneKg);

                var obsEvs = observedAnnotation1.AlleleSpecificAnnotations[altAllele].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)]
                        as EvsAnnotation;
                Assert.NotNull(obsEvs);

                Assert.Equal(expDbSnp.DbSnp, obsDbSnp.DbSnp);

                Assert.Equal(expEvs.EvsAll, obsEvs.EvsAll);
                Assert.Equal(expOneKg.OneKgAllAc, obsOneKg.OneKgAllAc);

                Assert.Equal(expEvs.EvsCoverage, obsEvs.EvsCoverage);
                Assert.Equal(expEvs.NumEvsSamples, obsEvs.NumEvsSamples);
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

            var sa = new SupplementaryAnnotationPosition(1564953);
            var saCreator = new SupplementaryPositionCreator(sa);

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItem = dbsnpReader.ExtractItem(vcfLine1)[0];
            var additionalItems = new List<SupplementaryDataItem>
                {
                    dbSnpItem.SetSupplementaryAnnotations(saCreator)
                };

            var oneKGenReader = new OneKGenReader(_renamer);
            var oneKGenItem = oneKGenReader.ExtractItems(vcfLine2)[0];
            additionalItems.Add(oneKGenItem.SetSupplementaryAnnotations(saCreator));

            var evsReader = new EvsReader(_renamer);
            var evsItemsList = evsReader.ExtractItems(vcfLine3);

            foreach (var evsItem in evsItemsList)
            {
                additionalItems.Add(evsItem.SetSupplementaryAnnotations(saCreator));
            }

            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(saCreator);
            }

            // write the supplementary annotation file
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(saCreator, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                var observedDataSourceVersions = reader.Header.DataSourceVersions;

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
                var observedAnnotation = reader.GetAnnotation(1564953) as SupplementaryAnnotationPosition;
                Assert.NotNull(observedAnnotation);

                var expectedInsOneKgAllAc = ((OneKGenAnnotation)sa.AlleleSpecificAnnotations["iG"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)]).OneKgAllAc;
                var expectedDelHasOneKg = sa.AlleleSpecificAnnotations["1"].HasDataSource(DataSourceCommon.DataSource.OneKg);

                var expectedInsEvsAfr = ((EvsAnnotation)sa.AlleleSpecificAnnotations["iG"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)]).EvsAfr;

                var expectedInsHasDbSnp = sa.AlleleSpecificAnnotations["iG"].HasDataSource(DataSourceCommon.DataSource.DbSnp);

                var obsAsaIns = observedAnnotation.AlleleSpecificAnnotations["iG"];
                var obsAsaDel = observedAnnotation.AlleleSpecificAnnotations["1"];

                Assert.Equal(expectedInsOneKgAllAc, ((OneKGenAnnotation)obsAsaIns.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)]).OneKgAllAc);
                Assert.Equal(expectedDelHasOneKg, obsAsaDel.HasDataSource(DataSourceCommon.DataSource.OneKg));

                Assert.Equal(expectedInsEvsAfr, ((EvsAnnotation)obsAsaIns.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)]).EvsAfr);
                Assert.Equal(expectedInsHasDbSnp, obsAsaIns.HasDataSource(DataSourceCommon.DataSource.DbSnp));
            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact]
        public void ReadWriteExacDbsnp()
        {
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var cosmicVersion = new DataSourceVersion("COSMIC", "GRCh37_v71", DateTime.Parse("2014-10-21").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, cosmicVersion };

            // create our expected supplementary annotations
            const string vcfLine1 = "2	48010488	rs1042821	G	A	.	.	RS=1042821;RSPOS=48010488;RV;dbSNPBuildID=86;SSR=0;SAO=1;VP=0x050168420a05150136100100;GENEINFO=MSH6:2956;WGT=1;VC=SNV;PM;PMC;SLO;NSM;REF;U5;R5;ASP;VLD;G5;GNO;KGPhase1;KGPhase3;LSD;CAF=0.7991,0.2009;COMMON=1";
            const string vcfLine2 =
                "2	48010488	rs1042821	G	A,C	14068898.15	PASS	AC=21019,1;AC_AFR=1700,0;AC_AMR=1015,1;AC_Adj=19510,1;AC_EAS=1973,0;AC_FIN=743,0;AC_Het=15722,1,0;AC_Hom=1894,0;AC_NFE=10593,0;AC_OTH=147,0;AC_SAS=3339,0;AF=0.178,8.487e-06;AN=117830;AN_AFR=6388;AN_AMR=9014;AN_Adj=91130;AN_EAS=6792;AN_FIN=5078;AN_NFE=48404;AN_OTH=664;AN_SAS=14790;BaseQRankSum=-4.850e-01;ClippingRankSum=-1.400e-01;DB;DP=1206681;FS=0.000;GQ_MEAN=129.86;GQ_STDDEV=221.88;Het_AFR=1322,0,0;Het_AMR=931,1,0;Het_EAS=1511,0,0;Het_FIN=665,0,0;Het_NFE=8585,0,0;Het_OTH=111,0,0;Het_SAS=2597,0,0;Hom_AFR=189,0;Hom_AMR=42,0;Hom_EAS=231,0;Hom_FIN=39,0;Hom_NFE=1004,0;Hom_OTH=18,0;Hom_SAS=371,0;InbreedingCoeff=0.0376;MQ=60.00;MQ0=0;MQRankSum=0.00;NCC=3737;POSITIVE_TRAIN_SITE;QD=17.46;ReadPosRankSum=0.181;VQSLOD=5.87;culprit=MQ;DP_HIST=3051|9435|11318|5521|9711|11342|4131|1270|615|404|328|266|264|262|196|186|126|115|97|277,133|968|2180|3402|3564|2815|1772|954|551|389|321|263|261|261|196|186|126|115|97|277,0|0|0|1|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0;GQ_HIST=949|2966|347|492|15135|1435|1335|854|421|526|590|416|13672|1951|445|462|255|174|211|16279,24|79|81|124|135|96|110|118|97|180|228|137|182|191|126|171|180|151|192|16229,0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|1";

            var sa = new SupplementaryAnnotationPosition(48010488);
            var saCreator = new SupplementaryPositionCreator(sa);

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItem1 = dbsnpReader.ExtractItem(vcfLine1)[0];
            dbSnpItem1.SetSupplementaryAnnotations(saCreator);

            var exacReader = new ExacReader(_renamer);
            foreach (var exacItem in exacReader.ExtractItems(vcfLine2))
            {
                exacItem.SetSupplementaryAnnotations(saCreator);
            }


            // write the supplementary annotation file
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr2", expectedDataSourceVersions))
            {
                writer.Write(saCreator, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(48010488) as SupplementaryAnnotationPosition;
                Assert.NotNull(observedAnnotation1);


                var expDbSnpA =
                    sa.AlleleSpecificAnnotations["A"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as DbSnpAnnotation;
                var obsDbSnpA =
                    observedAnnotation1.AlleleSpecificAnnotations["A"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as DbSnpAnnotation;
                Assert.NotNull(expDbSnpA);
                Assert.NotNull(obsDbSnpA);

                // we want to make sure we are reading the values we have written
                Assert.Equal(expDbSnpA.DbSnp, obsDbSnpA.DbSnp);


                var expExacA =
                    sa.AlleleSpecificAnnotations["A"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Exac)] as ExacAnnotation;
                var obsExacA =
                    observedAnnotation1.AlleleSpecificAnnotations["A"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Exac)] as ExacAnnotation;

                Assert.NotNull(expExacA);
                Assert.NotNull(obsExacA);

                Assert.Equal(expExacA.ExacAllAn, obsExacA.ExacAllAn);
                Assert.Equal(expExacA.ExacCoverage, obsExacA.ExacCoverage);
                Assert.Equal(Convert.ToDouble(expExacA.ExacAllAc), Convert.ToDouble(obsExacA.ExacAllAc));
                Assert.Equal(Convert.ToDouble(expExacA.ExacAfrAc), Convert.ToDouble(obsExacA.ExacAfrAc));
                Assert.Equal(Convert.ToDouble(expExacA.ExacAmrAc), Convert.ToDouble(obsExacA.ExacAmrAc));
                Assert.Equal(Convert.ToDouble(expExacA.ExacEasAc), Convert.ToDouble(obsExacA.ExacEasAc));
                Assert.Equal(Convert.ToDouble(expExacA.ExacFinAc), Convert.ToDouble(obsExacA.ExacFinAc));
                Assert.Equal(Convert.ToDouble(expExacA.ExacNfeAc), Convert.ToDouble(obsExacA.ExacNfeAc));
                Assert.Equal(Convert.ToDouble(expExacA.ExacOthAc), Convert.ToDouble(obsExacA.ExacOthAc));
                Assert.Equal(Convert.ToDouble(expExacA.ExacSasAc), Convert.ToDouble(obsExacA.ExacSasAc));


                var expExacC =
                    sa.AlleleSpecificAnnotations["C"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Exac)] as ExacAnnotation;
                var obsExacC =
                    observedAnnotation1.AlleleSpecificAnnotations["C"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Exac)] as ExacAnnotation;

                Assert.NotNull(expExacC);
                Assert.NotNull(obsExacC);

                Assert.Equal(expExacC.ExacCoverage, obsExacC.ExacCoverage);
                Assert.Equal(Convert.ToDouble(expExacC.ExacAllAc), Convert.ToDouble(obsExacC.ExacAllAc));
                Assert.Equal(Convert.ToDouble(expExacC.ExacAfrAc), Convert.ToDouble(obsExacC.ExacAfrAc));
                Assert.Equal(Convert.ToDouble(expExacC.ExacAmrAc), Convert.ToDouble(obsExacC.ExacAmrAc));
                Assert.Equal(Convert.ToDouble(expExacC.ExacEasAc), Convert.ToDouble(obsExacC.ExacEasAc));
                Assert.Equal(Convert.ToDouble(expExacC.ExacFinAc), Convert.ToDouble(obsExacC.ExacFinAc));
                Assert.Equal(Convert.ToDouble(expExacC.ExacNfeAc), Convert.ToDouble(obsExacC.ExacNfeAc));
                Assert.Equal(Convert.ToDouble(expExacC.ExacOthAc), Convert.ToDouble(obsExacC.ExacOthAc));
                Assert.Equal(Convert.ToDouble(expExacC.ExacSasAc), Convert.ToDouble(obsExacC.ExacSasAc));

            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }


        [Fact]
        public void ReadWriteDbSnpCosmic()
        {
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var cosmicVersion = new DataSourceVersion("COSMIC", "GRCh37_v71", DateTime.Parse("2014-10-21").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, cosmicVersion };

            // create our expected supplementary annotations
            const string vcfLine1 = "1	10228	rs143255646	TA	T	.	.	RS=143255646;RSPOS=10229;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000020005000002000200;WGT=1;VC=DIV;R5;ASP";

            var sa = new SupplementaryAnnotationPosition(10229);
            var saCreator = new SupplementaryPositionCreator(sa);

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItem1 = dbsnpReader.ExtractItem(vcfLine1)[0];
            var additionalItems = new List<SupplementaryDataItem>
                {
                    dbSnpItem1.SetSupplementaryAnnotations(saCreator)
                };

            var cosmicItem1 = new CosmicItem("1", 10229, "COSM1000", "TA", "T", "TP53",
                new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy("", "carcinoma", "oesophagus") }, null);
            var cosmicItem2 = new CosmicItem("1", 10229, "COSM1000", "TA", "T", "TP53",
                new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy("01", "carcinoma", "large_intestine") }, null);

            additionalItems.Add(cosmicItem1.SetSupplementaryAnnotations(saCreator));
            additionalItems.Add(cosmicItem2.SetSupplementaryAnnotations(saCreator));

            //sa.Clear();
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(saCreator);
            }

            Assert.Equal(1, sa.CosmicItems.Count);
            // the preceeding code has been unit tested in  MergeDbSnpCosmic()

            // write the supplementary annotation file
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(saCreator, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(10229) as SupplementaryAnnotationPosition;
                Assert.NotNull(observedAnnotation1);

                var expDbSnp =
                    sa.AlleleSpecificAnnotations["1"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as
                        DbSnpAnnotation;
                Assert.NotNull(expDbSnp);
                var obsDbSnp =
                    observedAnnotation1.AlleleSpecificAnnotations["1"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as
                        DbSnpAnnotation;
                Assert.NotNull(obsDbSnp);

                Assert.Equal(expDbSnp.DbSnp, obsDbSnp.DbSnp);
                Assert.True(observedAnnotation1.ContainsCosmicId(sa.CosmicItems[0].ID));
                Assert.Equal(1, observedAnnotation1.CosmicItems.Count);
            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact(Skip = "Requires reference sequence to continue.")]
        public void ReadWriteClinVar()
        {
            //test to make sure that we write a ClinVar entry and read back the same thing.
            var xmlReader = new ClinVarXmlReader(new FileInfo(@"Resources\RCV000152657.xml"), _reader, _sequence);

            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var clinVarVersion = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, clinVarVersion };

            var sa = new SupplementaryAnnotationPosition(10183457);
            var saCreator = new SupplementaryPositionCreator(sa);

            foreach (var clinVarItem in xmlReader)
            {
                clinVarItem.SetSupplementaryAnnotations(saCreator);
                saCreator.MergeSaCreator(saCreator);
            }

            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr9", expectedDataSourceVersions))
            {
                writer.Write(saCreator, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                var observedAnnotation = reader.GetAnnotation(10183457) as SupplementaryAnnotationPosition;

                Assert.NotNull(observedAnnotation);

                for (var i = 0; i < sa.ClinVarItems.Count; i++)
                {
                    Assert.Equal(sa.ClinVarItems[i].ID, observedAnnotation.ClinVarItems[i].ID);
                    Assert.Equal(sa.ClinVarItems[i].Significance, observedAnnotation.ClinVarItems[i].Significance);
                    Assert.Equal(sa.ClinVarItems[i].LastUpdatedDate, observedAnnotation.ClinVarItems[i].LastUpdatedDate);
                    Assert.True(sa.ClinVarItems[i].Phenotypes.SequenceEqual(observedAnnotation.ClinVarItems[i].Phenotypes));
                    Assert.True(sa.ClinVarItems[i].MedGenIDs.SequenceEqual(observedAnnotation.ClinVarItems[i].MedGenIDs));
                    Assert.True(sa.ClinVarItems[i].OrphanetIDs.SequenceEqual(observedAnnotation.ClinVarItems[i].OrphanetIDs));
                    Assert.Equal(sa.ClinVarItems[i].AlleleOrigins, observedAnnotation.ClinVarItems[i].AlleleOrigins);
                    Assert.True(sa.ClinVarItems[i].OmimIDs.SequenceEqual(observedAnnotation.ClinVarItems[i].OmimIDs));
                    Assert.True(sa.ClinVarItems[i].PubmedIds.SequenceEqual(observedAnnotation.ClinVarItems[i].PubmedIds));
                }
            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact]
        public void ReadWriteWithSuppIntervals()
        {
            // NIR-1359
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion };

            // This is the case where Nirvana throws an error: Too many bytes in what should have been a 7 bit encoded Int32.

            var sa = new SupplementaryAnnotationPosition(5073770);
            var saCreator = new SupplementaryPositionCreator(sa);


            // adding a supplementary interval
            var intValues = new Dictionary<string, int>();
            var doubleValues = new Dictionary<string, double>();
            var freqValues = new Dictionary<string, double>();
            var stringValues = new Dictionary<string, string>();
            var boolValues = new List<string>();

            var suppInterval = new SupplementaryInterval(5073770, 5073970, "chr1", "<DUP>", VariantType.duplication, "ClinVar", _renamer, intValues,
                doubleValues, freqValues, stringValues, boolValues);

            suppInterval.AddStringValue("ID", "RandomClin001");

            // the above code was unit tested in MergeDbSnpClinVar()
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr9", expectedDataSourceVersions))
            {
                writer.SetIntervalList(new List<SupplementaryInterval> { suppInterval });
                writer.Write(saCreator, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // read the stored intervals
                var suppIntervals = reader.GetSupplementaryIntervals(_renamer).ToList();
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
            var randomPath = GetRandomPath(true);

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var clinVarVersion = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, clinVarVersion };

            var customFile = new FileInfo(Resources.TopPath("customCosmic.vcf"));

            var customReader = new CustomAnnotationReader(customFile, _renamer);

            // all items from this file should be of type cosmic.
            var customItems = customReader.ToList();

            var sa = new SupplementaryAnnotationPosition(69224);
            var saCreator = new SupplementaryPositionCreator(sa);

            foreach (var customItem in customItems)
            {
                // NOTE that the two custom items are for different position, but for the purpose of our test, this is not an issue.
                customItem.SetSupplementaryAnnotations(saCreator);
            }

            // the above code was unit tested in MergeDbSnpClinVar()
            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(saCreator, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(69224);

                Assert.NotNull(observedAnnotation1);

                for (var i = 0; i < sa.CustomItems.Count; i++)
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
            var xmlReader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000087262.xml")), _reader, _sequence);

            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var clinVarVersion = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, clinVarVersion };

            // This is the case where Nirvana throws an error: Too many bytes in what should have been a 7 bit encoded Int32.

            var sa = new SupplementaryAnnotationPosition(225592188);
            var saCreator = new SupplementaryPositionCreator(sa);

            foreach (var clinVarItem in xmlReader)
            {
                clinVarItem.SetSupplementaryAnnotations(saCreator);
                saCreator.MergeSaCreator(saCreator);
            }

            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr1", expectedDataSourceVersions))
            {
                writer.Write(saCreator, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(225592188) as SupplementaryAnnotationPosition;

                Assert.NotNull(observedAnnotation1);

                for (var i = 0; i < sa.ClinVarItems.Count; i++)
                {
                    Assert.Equal(sa.ClinVarItems[i].Phenotypes, observedAnnotation1.ClinVarItems[i].Phenotypes);
                }
            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

        [Fact]
        public void ReadAndWriteExacWithMultipleAlleles()
        {
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected data source versions
            var exacVersion = new DataSourceVersion("ExAC", "0.3.1", DateTime.Parse("2016-03-16").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { exacVersion };

            // create our expected supplementary annotations: note AN_adj is modified in this 
            const string vcfline =
                "19	3121452	.	TA	T,TAA	17262.47	AC_Adj0_Filter	AC=6,9;AC_AFR=0,0;AC_AMR=0,0;AC_Adj=0,0;AC_EAS=0,0;AC_FIN=0,0;AC_Het=0,0,0;AC_Hom=0,0;AC_NFE=0,0;AC_OTH=0,0;AC_SAS=0,0;AF=4.587e-03,6.881e-03;AN=1308;AN_AFR=0;AN_AMR=0;AN_Adj=3;AN_EAS=0;AN_FIN=0;AN_NFE=0;AN_OTH=0;AN_SAS=0;BaseQRankSum=0.437;DP=2838";

            var sa = new SupplementaryAnnotationPosition(3121453);
            var saCreator = new SupplementaryPositionCreator(sa);

            var exacReader = new ExacReader(_renamer);
            var additionalItems = new List<SupplementaryDataItem>();
            foreach (var exacItem in exacReader.ExtractItems(vcfline))
            {
                var currentItem = exacItem.SetSupplementaryAnnotations(saCreator);
                additionalItems.Add(currentItem);
            }
            var currentSa = new SupplementaryAnnotationPosition(3121453);
            var currentSaCreator = new SupplementaryPositionCreator(currentSa);
            foreach (var exacItem in additionalItems)
            {
                exacItem.SetSupplementaryAnnotations(currentSaCreator);
            }

            // write the supplementary annotation file
            using (
                var writer = new SupplementaryAnnotationWriter(randomPath, "chr19",
                    expectedDataSourceVersions))
            {
                writer.Write(currentSaCreator, currentSa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = reader.GetAnnotation(3121453) as SupplementaryAnnotationPosition;

                Assert.NotNull(observedAnnotation1);

                // we want to make sure we are reading the values we have written

                var expExaciA =
                    currentSa.AlleleSpecificAnnotations["iA"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Exac)] as
                        ExacAnnotation;
                Assert.NotNull(expExaciA);
                var obsExaciA =
                    observedAnnotation1.AlleleSpecificAnnotations["iA"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Exac)] as
                        ExacAnnotation;
                Assert.NotNull(obsExaciA);

                Assert.Equal(expExaciA.ExacAllAn, obsExaciA.ExacAllAn);

                Assert.Equal(expExaciA.ExacCoverage, obsExaciA.ExacCoverage);
                Assert.Equal(expExaciA.ExacAllAc, obsExaciA.ExacAllAc);
                Assert.NotNull(obsExaciA.ExacAllAc);
                Assert.Null(obsExaciA.ExacFinAc);
                Assert.Null(obsExaciA.ExacFinAn);

                // we want to make sure we are reading the values we have written

                var expExac1 =
                    currentSa.AlleleSpecificAnnotations["1"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Exac)] as
                        ExacAnnotation;
                Assert.NotNull(expExac1);
                var obsExac1 =
                    observedAnnotation1.AlleleSpecificAnnotations["1"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Exac)] as
                        ExacAnnotation;
                Assert.NotNull(obsExac1);

                Assert.Equal(expExac1.ExacAllAn, obsExac1.ExacAllAn);

                Assert.Equal(expExac1.ExacCoverage, obsExac1.ExacCoverage);
                Assert.Equal(expExac1.ExacAllAc, obsExac1.ExacAllAc);
                Assert.NotNull(obsExac1.ExacAllAc);
                Assert.Null(obsExac1.ExacFinAc);
                Assert.Null(obsExac1.ExacFinAn);

            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }
    }
}
