using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Illumina.VariantAnnotation.Interface;
using Illumina.VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Illumina.VariantAnnotation.FileHandling.SupplementaryAnnotations;
using InputFileParsers.ClinVar;
using InputFileParsers.SupplementaryData;
using Xunit;

namespace NirvanaUnitTests.FileHandling
{
    public sealed class ClinVarTests
    {
        private static readonly FileInfo TestClinVarFile = new FileInfo(@"Resources\testClinVar.vcf");

        [Fact]
        public void ClinVarReaderTest()
        {
            var clinVarReader = new ClinVarReader(TestClinVarFile);
            int count = clinVarReader.Count();

            Assert.Equal(16,count);
        }

        [Fact]
        public void OneAltAlleleTest()
        {
            const string vcfLine = "1	883516	rs267598747	G	A	.	.	RS=267598747;RSPOS=883516;dbSNPBuildID=137;SSR=0;SAO=3;VP=0x050060000305000002100120;GENEINFO=NOC2L:26155;WGT=1;VC=SNV;PM;REF;SYN;ASP;LSD;CLNALLE=1;CLNHGVS=NC_000001.10:g.883516G>A;CLNSRC=ClinVar;CLNORIGIN=2;CLNSRCID=NM_015658.3:c.1654C>T;CLNSIG=255;CLNDSDB=MedGen:SNOMED_CT;CLNDSDBID=C0025202:2092003;CLNDBN=Malignant_melanoma;CLNREVSTAT=no_assertion;CLNACC=RCV000064926.2";

            var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

            var sa= new SupplementaryAnnotation();

            foreach (var clinVarItem in clinVarItems)
            {
                clinVarItem.SetSupplementaryAnnotations(sa);
            }

            Assert.Equal(sa.ClinVarEntries[0].ID, "RCV000064926.2");
        }

        //

        [Fact]
        public void MedgenError()
        {
			const string vcfLine = "17	39912145	rs1126821	T	A	.	.	RS=1126821;RSPOS=39912145;RV;dbSNPBuildID=86;SSR=0;SAO=1;VP=0x050160000a0517053f110100;GENEINFO=JUP:3728;WGT=1;VC=SNV;PM;SLO;NSM;REF;ASP;VLD;G5A;G5;HD;GNO;KGPhase1;KGPhase3;LSD;OM;CLNALLE=1;CLNHGVS=NC_000017.10:g.39912145T>A;CLNSRC=ClinVar|.|GeneDx|GeneReviews;CLNORIGIN=1;CLNSRCID=NM_002230.2:c.2089A>T|.|13090|NBK1131;CLNSIG=2|2|2;CLNDSDB=GeneReviews:MedGen:OMIM:Orphanet|MedGen|MedGen;CLNDSDBID=NBK1131:C1832600:601214:ORPHA34217|CN169374|CN221809;CLNDBN=Naxos_disease|not_specified|not_provided;CLNREVSTAT=single|single|single;CLNACC=RCV000020467.1|RCV000039075.1|RCV000126389.1;CAF=0.4127,0.5873;COMMON=1";

            var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

            var sa = new SupplementaryAnnotation();

            foreach (var clinVarItem in clinVarItems)
            {
                clinVarItem.SetSupplementaryAnnotations(sa);
            }

			Assert.Equal(sa.ClinVarEntries[0].MedGenID, "C1832600");
            Assert.Equal(sa.ClinVarEntries[0].GeneReviewsID, "NBK1131");
			Assert.Equal(sa.ClinVarEntries[1].MedGenID, "CN169374");
			Assert.Equal(sa.ClinVarEntries[2].MedGenID, "CN221809");
            
        }

	    [Fact]
	    public void RefAlleleStudy()
	    {
			// NIR-890
			const string vcfLine = "1	8021910	rs373653682	GGTGCTGGACGGTGTCCCT	G	.	.	RS=373653682;RSPOS=8021928;dbSNPBuildID=138;SSR=0;SAO=0;VP=0x0500000a0005000002000200;GENEINFO=PARK7:11315;WGT=1;VC=DIV;INT;R5;ASP;CLNALLE=0;CLNHGVS=NC_000001.10:g.8021928_8021945dup18;CLNSRC=ClinVar;CLNORIGIN=1;CLNSRCID=NM_007262.4:c.-24+75_-24+92dup;CLNSIG=5;CLNDSDB=GeneReviews:MedGen:OMIM:Orphanet;CLNDSDBID=NBK1223:C1853445:606324:ORPHA2828;CLNDBN=Parkinson_disease_7;CLNREVSTAT=single;CLNACC=RCV000007484.1;CAF=0.9071,0.09285;COMMON=1";
			var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

			var sa = new SupplementaryAnnotation();
		    clinVarItems[0].SetSupplementaryAnnotations(sa);

			Assert.Equal( "RCV000007484.1", sa.ClinVarEntries[0].ID);
	    }

	    [Fact]
	    public void AlleleSpecificClnOrigin()
	    {
		    const string vcfLine =
				"1	11854476	rs1801131	T	G	.	.	RS=1801131;RSPOS=11854476;RV;dbSNPBuildID=89;SSR=0;SAO=0;VP=0x050178000a0517053f110101;GENEINFO=MTHFR:4524;WGT=1;VC=SNV;PM;TPA;PMC;SLO;NSM;REF;ASP;VLD;G5A;G5;HD;GNO;KGPhase1;KGPhase3;LSD;OM;CLNALLE=1;CLNHGVS=NC_000001.10:g.11854476T>G;CLNSRC=ClinVar|.|University_of_Bologna|GTR|OMIM_Allelic_Variant;CLNORIGIN=1;CLNSRCID=NM_005957.4:c.1286A>C|.|rs1801131|GTR000500035|607093.0004;CLNSIG=2|255|0;CLNDSDB=MedGen|.|MedGen:OMIM:Orphanet;CLNDSDBID=C1856059|.|C0238198:606764:ORPHA44890;CLNDBN=MTHFR_deficiency\x2c_thermolabile_type|Schizophrenia\x2c_susceptibility_to|Gastrointestinal_Stromal_Tumors;CLNREVSTAT=single|single|single;CLNACC=RCV000003698.1|RCV000003699.1|RCV000144922.1;CAF=0.7506,0.2494;COMMON=1";

			var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

			var sa = new SupplementaryAnnotation();

			foreach (var clinVarItem in clinVarItems)
			{
				clinVarItem.SetSupplementaryAnnotations(sa);
			}

			Assert.Equal(sa.ClinVarEntries[0].AlleleOrigin, "germline");
			Assert.Equal(sa.ClinVarEntries[1].AlleleOrigin, "germline");
			Assert.Equal(sa.ClinVarEntries[2].AlleleOrigin, "germline");
			
	    }

	    [Fact]
	    public void Parsing20160302Version()
	    {
			// NIR-1419
		    const string vcfLine = "1	27549217	rs587779767	AG	A	.	.	RS=587779767;RSPOS=27549218;RV;dbSNPBuildID=142;SSR=0;SAO=1;VP=0x050068001205000002110200;GENEINFO=AHDC1:27245;WGT=1;VC=DIV;PM;PMC;NSF;REF;ASP;LSD;OM;CLNALLE=1;CLNHGVS=NC_000001.11:g.27549218delG;CLNSRC=OMIM_Allelic_Variant;CLNORIGIN=1;CLNSRCID=615790.0002;CLNSIG=5|5|5|5|5|5;CLNDSDB=MedGen:OMIM|Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:MedGen|Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:MedGen|Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontology:Human_Phenotype_Ontolog;CLNDSDBID=CN188260:615829|HPO0000750:HPO0002116:HPO0002117:HPO0002336:HPO0002399:HPO0002498:HPO0006936:HPO0007004:HPO0007127:HPO0007170:HPO0007172:CN000706|HPO0000754:HPO0001255:HPO0001263:HPO0001277:HPO0001292:HPO0002433:HPO0002473:HPO0002532:HPO0006793:HPO0006867:HPO0006885:HPO0006935:HPO0007005:HPO0007094:HPO0007106:HPO0007174:HPO0007224:HPO0007228:HPO0007342:CN001157|HPO0000730:HPO0001249:HPO0001267:HPO0001286:HPO0002122:HPO0002192:HPO0002316:HPO0002382:HPO0002386:HPO0002402:HPO0002458:HPO0002482:HPO0002499:HPO0002543:HPO0003767:HPO0006833:HPO0007154:HPO0007176:HPO0007180:C1843367|HPO0001319:HPO0008976:C1834679|HPO0010535:CN009366;CLNDBN=Xia-Gibbs_syndrome|Delayed_speech_and_language_development|Global_developmental_delay|Intellectual_disability|Neonatal_hypotonia|Sleep_apnea;CLNREVSTAT=no_criteria|no_criteria|no_criteria|no_criteria|no_criteria|no_criteria;CLNACC=RCV000119839.2|RCV000144168.1|RCV000144168.1|RCV000144168.1|RCV000144168.1|RCV000144168.1";

			var clinVarItem = ClinVarReader.ExtractClinVarItems(vcfLine)[0];
			
			Assert.Equal("RCV000119839.2", clinVarItem.ID);
	    }

	    [Fact]
	    public void NonEnglishChars()
	    {
			// NIR-900
			const string vcfLine =
				"1	225592188	rs387906416	TAGAAGA	CTTCTAG	.	.	RS=387906416;RSPOS=225592188;RV;dbSNPBuildID=137;SSR=0;SAO=0;VP=0x050060000605000002110800;GENEINFO=LBR:3930;WGT=1;VC=MNV;PM;NSN;REF;ASP;LSD;OM;CLNALLE=1;CLNHGVS=NC_000001.10:g.225592188_225592194delTAGAAGAinsCTTCTAG;CLNSRC=ClinVar|OMIM_Allelic_Variant;CLNORIGIN=1;CLNSRCID=NM_194442.2:c.1599_1605delTCTTCTAinsCTAGAAG|600024.0003;CLNSIG=5|5;CLNDSDB=MedGen:OMIM:Orphanet:SNOMED_CT|MedGen:OMIM:SNOMED_CT;CLNDSDBID=C1300226:215140:ORPHA1426:389261002|C0030779:169400:85559002;CLNDBN=Greenberg_dysplasia|Pelger-Huët_anomaly;CLNREVSTAT=single|single;CLNACC=RCV000010137.2|RCV000087262.2";

			var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

			var sa = new SupplementaryAnnotation();

			foreach (var clinVarItem in clinVarItems)
			{
				clinVarItem.SetSupplementaryAnnotations(sa);
			}

			Assert.Equal(sa.ClinVarEntries[0].Phenotype, "Greenberg_dysplasia");
			Assert.Equal(sa.ClinVarEntries[1].Phenotype, "Pelger-Huët_anomaly");

		    var convertedPhenotype = SupplementaryAnnotation.ClinVar.ConvertMixedFormatString(sa.ClinVarEntries[1].Phenotype);

			Assert.Equal("Pelger-Huët_anomaly", convertedPhenotype);

	    }

	    [Fact]
	    public void MultipleIds()
	    {
		    const string vcfLine =
			    "1	55518316	rs2483205	C	T	.	.	RS=2483205;RSPOS=55518316;dbSNPBuildID=100;SSR=0;SAO=1;VP=0x05016808000517053e100100;GENEINFO=PCSK9:255738;WGT=1;VC=SNV;PM;PMC;SLO;INT;ASP;VLD;G5A;G5;HD;GNO;KGPhase1;KGPhase3;LSD;CLNALLE=1;CLNHGVS=NC_000001.10:g.55518316C>T;CLNSRC=ClinVar;CLNORIGIN=0;CLNSRCID=NM_174936.3:c.658-7C>T;CLNSIG=2;CLNDSDB=MedGen:OMIM:OMIM:SNOMED_CT:SNOMED_CT;CLNDSDBID=C0020445:143890:144400:397915002:398036000;CLNDBN=Familial_hypercholesterolemia;CLNREVSTAT=single;CLNACC=RCV000030351.1;CAF=0.6058,0.3942;COMMON=1";

			var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

			var sa = new SupplementaryAnnotation();

			foreach (var clinVarItem in clinVarItems)
			{
				clinVarItem.SetSupplementaryAnnotations(sa);
			}

			Assert.Equal(sa.ClinVarEntries[0].MedGenID, "C0020445");
			Assert.Equal(sa.ClinVarEntries[0].OmimID, "143890,144400");
			Assert.Equal(sa.ClinVarEntries[0].SnoMedCtID, "397915002,398036000");
	    }

	    [Fact]
        public void MissingPhenotypeReadBack()
        {
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // create our expected key/value pairs
            var expectedKeyValuePairs = new Dictionary<string, string> { { "foo", "bar" } };

            // create our expected data source versions
            var dbSnpVersion = new DataSourceVersion("dbSNP", "142", DateTime.Parse("2015-01-02").Ticks);
            var clinVarVersion = new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks);

            var expectedDataSourceVersions = new List<DataSourceVersion> { dbSnpVersion, clinVarVersion };

            const string vcfLine = "17	2266812	rs2003968	T	A,C,G	.	.	RS=2003968;RSPOS=2266812;dbSNPBuildID=92;SSR=0;SAO=3;VP=0x050160000b0517053e100120;GENEINFO=SGSM2:9905;WGT=1;VC=SNV;PM;SLO;NSM;REF;SYN;ASP;VLD;G5A;G5;HD;GNO;KGPhase1;KGPhase3;LSD;CLNALLE=3;CLNHGVS=NC_000017.10:g.2266812T>G;CLNSRC=ClinVar;CLNORIGIN=2;CLNSRCID=NM_001098509.1:c.726T>G;CLNSIG=255;CLNDSDB=MedGen:SNOMED_CT;CLNDSDBID=C0025202:2092003;CLNDBN=Malignant_melanoma;CLNREVSTAT=no_assertion;CLNACC=RCV000071372.2;CAF=0.4337,.,0.5663,.;COMMON=1";

            var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

            var sa = new SupplementaryAnnotation();

            foreach (var clinVarItem in clinVarItems)
            {
                clinVarItem.SetSupplementaryAnnotations(sa);
            }

            using (var writer = new SupplementaryAnnotationWriter(randomPath, "chr17", 512*20, expectedKeyValuePairs, expectedDataSourceVersions))
            {
                writer.Write(sa, sa.ReferencePosition);
            }

            // read the supplementary annotation file
            using (var reader = new SupplementaryAnnotationReader(randomPath))
            {
                // extract the three annotations
                var observedAnnotation1 = new SupplementaryAnnotation();

                Assert.True(reader.GetNextAnnotation(observedAnnotation1));

                for (int i = 0; i < sa.ClinVarEntries.Count; i++)
                {
                    Assert.Equal(sa.ClinVarEntries[i].ID, observedAnnotation1.ClinVarEntries[i].ID);
                    Assert.Equal(sa.ClinVarEntries[i].Phenotype, observedAnnotation1.ClinVarEntries[i].Phenotype);
					Assert.Equal(ReviewStatusEnum.no_assertion, sa.ClinVarEntries[i].ReviewStatus);
                }
            }

            File.Delete(randomPath);
            File.Delete(randomPath + ".idx");
        }

	    [Fact]
	    public void UnknownInsertion()
	    {
			// NIR-898
		    const string vcfLine =
			    "13	40298638	rs66629036	TA	T	.	.	RS=66629036;RSPOS=40298639;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000080005000002000200;GENEINFO=COG6:57511;WGT=1;VC=DIV;INT;ASP;CLNALLE=-1,1;CLNHGVS=NC_000013.10:g.40298639A>T,NC_000013.10:g.40298641delA;CLNSRC=ClinVar,ClinVar;CLNORIGIN=1,1;CLNSRCID=NM_001145079.1:c.1693-6A>T,NM_001145079.1:c.1693-4delA;CLNSIG=2,2;CLNDSDB=MedGen,MedGen;CLNDSDBID=CN169374,CN169374;CLNDBN=not_specified,not_specified;CLNREVSTAT=single,single;CLNACC=RCV000082046.1,RCV000082045.1";

			var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

			var sa = new SupplementaryAnnotation();

			
			foreach (var clinVarItem in clinVarItems)
			{
				clinVarItem.SetSupplementaryAnnotations(sa);
			}

			// the second item is not recorded in sa as it gets a position increment

			Assert.Equal(sa.ClinVarEntries[0].ID, "RCV000082046.1");
			Assert.Equal(ReviewStatusEnum.single_submitter, sa.ClinVarEntries[0].ReviewStatus);
	    }

	    [Fact]
        public void MissingAlleleTest()
        {
            const string vcfLine = @"1	984971	rs111818381	G	A,C	.	.	RS=111818381;RSPOS=984971;dbSNPBuildID=132;SSR=0;SAO=0;VP=0x050360000a05040536100100;GENEINFO=AGRN:375790;WGT=1;VC=SNV;PM;S3D;SLO;NSM;REF;ASP;VLD;HD;GNO;KGPhase1;KGPhase3;LSD;CLNALLE=1;CLNHGVS=NC_000001.10:g.984971G>A;CLNSRC=ClinVar|.|University_of_Chicago;CLNORIGIN=1;CLNSRCID=NM_198576.3:c.4540G>A|.|NM_198576.2(AGRN):c.4540;CLNSIG=3;CLNDSDB=MedGen;CLNDSDBID=CN169374;CLNDBN=not_specified;CLNREVSTAT=single;CLNACC=RCV000116271.2;CAF=0.9956,0.004393,.;COMMON=1";

            var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

            var sa = new SupplementaryAnnotation();

            foreach (var clinVarItem in clinVarItems)
            {
                clinVarItem.SetSupplementaryAnnotations(sa);
            }

            Assert.Equal(sa.ClinVarEntries[0].ID, "RCV000116271.2");
        }

        [Fact]
        public void NoAlleleTest()
        {
            const string vcfLine="1	24122692	rs3180383	G	A,C	.	.	RS=3180383;RSPOS=24122692;dbSNPBuildID=105;SSR=0;SAO=1;VP=0x050260000b05000002110100;GENEINFO=GALE:2582;WGT=1;VC=SNV;PM;S3D;NSM;REF;SYN;ASP;LSD;OM;CLNALLE=-1;CLNHGVS=NC_000001.10:g.24122692G>T;CLNSRC=ClinVar|OMIM_Allelic_Variant;CLNORIGIN=1;CLNSRCID=NM_001008216.1:c.937C>A|606953.0006;CLNSIG=5;CLNDSDB=GeneReviews:MedGen:OMIM:Orphanet:Orphanet:SNOMED_CT;CLNDSDBID=NBK51671:C0751161:230350:ORPHA352:ORPHA79238:8849004;CLNDBN=UDPglucose-4-epimerase_deficiency;CLNREVSTAT=single;CLNACC=RCV000003865.1";

            var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);
            Assert.Equal("N", clinVarItems[0].AltAllele);
            Assert.Equal("RCV000003865.1", clinVarItems[0].ID);
        }

        [Fact]
        public void PipeSeparatorTest()
        {
            const string vcfLine =
                "1	1959699	rs41307846	G	A	.	.	RS=41307846;RSPOS=1959699;dbSNPBuildID=127;SSR=0;SAO=1;VP=0x050260000a05040136110100;GENEINFO=GABRD:2563;WGT=1;VC=SNV;PM;S3D;NSM;REF;ASP;VLD;GNO;KGPhase1;KGPhase3;LSD;OM;CLNALLE=1;CLNHGVS=NC_000001.10:g.1959699G>A;CLNSRC=ClinVar|OMIM_Allelic_Variant;CLNORIGIN=1;CLNSRCID=NM_000815.4:c.659G>A|137163.0002;CLNSIG=255|255|255;CLNDSDB=MedGen|MedGen|MedGen:OMIM;CLNDSDBID=C3150401|CN043549|C2751603:613060;CLNDBN=Generalized_epilepsy_with_febrile_seizures_plus_type_5|Epilepsy\\x2c_juvenile_myoclonic_7|Epilepsy\\x2c_idiopathic_generalized_10;CLNREVSTAT=single|single|single;CLNACC=RCV000017599.1|RCV000017600.1|RCV000022558.1;CAF=0.9942,0.005791;COMMON=1";

            var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

            var sa = new SupplementaryAnnotation();

            foreach (var clinVarItem in clinVarItems)
            {
                clinVarItem.SetSupplementaryAnnotations(sa);
            }
            Assert.Equal(sa.ClinVarEntries[0].ID, "RCV000017599.1");
            Assert.Equal(sa.ClinVarEntries[0].Significance, "other");
            Assert.Equal(sa.ClinVarEntries[0].AlleleOrigin, "germline");
            Assert.Equal(sa.ClinVarEntries[0].Phenotype, "Generalized_epilepsy_with_febrile_seizures_plus_type_5");
			Assert.Equal(ReviewStatusEnum.single_submitter, sa.ClinVarEntries[0].ReviewStatus);

            Assert.Equal(sa.ClinVarEntries[1].ID, "RCV000017600.1");
            Assert.Equal(sa.ClinVarEntries[1].MedGenID, "CN043549");
            Assert.Equal(sa.ClinVarEntries[1].AlleleOrigin,"germline");
            Assert.Equal(sa.ClinVarEntries[1].Phenotype, "Epilepsy\\x2c_juvenile_myoclonic_7");
			Assert.Equal(ReviewStatusEnum.single_submitter, sa.ClinVarEntries[1].ReviewStatus);

			Assert.Equal(sa.ClinVarEntries[2].ID, "RCV000022558.1");
            Assert.Equal(sa.ClinVarEntries[2].MedGenID, "C2751603");
			Assert.Equal(sa.ClinVarEntries[2].OmimID, "613060");
            Assert.Equal(sa.ClinVarEntries[2].Phenotype, "Epilepsy\\x2c_idiopathic_generalized_10");
			Assert.Equal(ReviewStatusEnum.single_submitter, sa.ClinVarEntries[2].ReviewStatus);
		}

        [Fact]
        public void MultiAlleleTest()
        {
            const string vcfLine = "1	2160305	rs387907306	G	A,T	.	.	RS=387907306;RSPOS=2160305;dbSNPBuildID=137;SSR=0;SAO=0;VP=0x050060000a05000002110100;GENEINFO=SKI:6497;WGT=1;VC=SNV;PM;NSM;REF;ASP;LSD;OM;CLNALLE=1,2;CLNHGVS=NC_000001.10:g.2160305G>A,NC_000001.10:g.2160305G>T;CLNSRC=ClinVar|OMIM_Allelic_Variant,ClinVar|OMIM_Allelic_Variant;CLNORIGIN=1,1;CLNSRCID=NM_003036.3:c.100G>A|164780.0004,NM_003036.3:c.100G>T|164780.0005;CLNSIG=5,5;CLNDSDB=GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT,GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT;CLNDSDBID=NBK1277:C1321551:182212:ORPHA2462:83092002,NBK1277:C1321551:182212:ORPHA2462:83092002;CLNDBN=Shprintzen-Goldberg_syndrome,Shprintzen-Goldberg_syndrome;CLNREVSTAT=single,single;CLNACC=RCV000030819.24,RCV000030820.24";

            var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

            var sa = new SupplementaryAnnotation();

            foreach (var clinVarItem in clinVarItems)
            {
                clinVarItem.SetSupplementaryAnnotations(sa);
            }
            Assert.Equal(2, sa.ClinVarEntries.Count);

            foreach (var clinVarEntry in sa.ClinVarEntries)
            {
                if (clinVarEntry.SaAltAllele.Equals("A"))
                {
                    Assert.Equal(clinVarEntry.ID, "RCV000030819.24");
                    Assert.Equal(clinVarEntry.OrphanetID, "ORPHA2462");
                    Assert.Equal(clinVarEntry.Phenotype, "Shprintzen-Goldberg_syndrome");
                }
                if (clinVarEntry.SaAltAllele.Equals("T"))
                {
                    Assert.Equal(clinVarEntry.ID, "RCV000030820.24");
                    Assert.Equal(clinVarEntry.OrphanetID, "ORPHA2462");
                    Assert.Equal(clinVarEntry.Phenotype, "Shprintzen-Goldberg_syndrome");
                }
            }
        }

	    [Fact]
	    public void RefStudyPosition()
	    {
			// NIR-946

		    const string vcfLine =
			    "1	237965133	rs55683196	A	AT,ATT	.	.	RS=55683196;RSPOS=237965145;dbSNPBuildID=129;SSR=0;SAO=0;VP=0x050000080005000002000200;GENEINFO=RYR2:6262;WGT=1;VC=DIV;INT;ASP;CLNALLE=1,0;CLNHGVS=NC_000001.10:g.237965145_237965146insT,NC_000001.10:g.237965145delT;CLNSRC=ClinVar,ClinVar;CLNORIGIN=1,1;CLNSRCID=NM_001035.2:c.14091-11_14091-10insT,NM_001035.2:c.14091-11delT;CLNSIG=3,3;CLNDSDB=MedGen,MedGen;CLNDSDBID=CN169374,CN169374;CLNDBN=not_specified,not_specified;CLNREVSTAT=single,single;CLNACC=RCV000036690.1,RCV000036691.1;CAF=0.622,0.378,.;COMMON=1";

			var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

			var sa = new SupplementaryAnnotation();
		    
			// the second clinvar item is the one associated with ref
			// SetSupplementaryAnnotation returns null when the position is unchanged.
			Assert.Null(clinVarItems[1].SetSupplementaryAnnotations(sa));
			Assert.Equal("RCV000036691.1", sa.ClinVarEntries[0].ID);
			
	    }

	    [Fact]
        public void ComplexVariants()
        {
            const string vcfLine = "7	55249022	rs397517114	G	GCCCACG,GGCCACG	.	.	RS=397517114;RSPOS=55249022;dbSNPBuildID=138;SSR=0;SAO=0;VP=0x050060000005000002100200;GENEINFO=EGFR:1956|EGFR-AS1:100507500;WGT=1;VC=DIV;PM;ASP;LSD;CLNALLE=1,2;CLNHGVS=NC_000007.13:g.55249017_55249022dupCCCACG,NC_000007.13:g.55249022_55249023insGCCACG;CLNSRC=ClinVar|ClinVar,ClinVar|ClinVar;CLNORIGIN=2,2;CLNSRCID=NM_005228.3:c.2320_2321insCCCACG|NR_047551.1:n.1241_1242insCGTGGG,NM_005228.3:c.2320_2321insGCCACG|NR_047551.1:n.1241_1242insCGTGGC;CLNSIG=255,255;CLNDSDB=MedGen:SNOMED_CT,MedGen:SNOMED_CT;CLNDSDBID=C0007131:254637007,C0007131:254637007;CLNDBN=Non-small_cell_lung_cancer,Non-small_cell_lung_cancer;CLNREVSTAT=single,single;CLNACC=RCV000038416.1,RCV000038419.1";

            var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

            var sa = new SupplementaryAnnotation();

            var additionalSuppItems = new List<ISupplementaryDataItem>();

            foreach (var clinVarItem in clinVarItems)
            {
                additionalSuppItems.Add(clinVarItem.SetSupplementaryAnnotations(sa));
            }

            sa.Clear();
            foreach (var supplementaryDataItem in additionalSuppItems)
            {
                supplementaryDataItem.SetSupplementaryAnnotations(sa);
            }

            foreach (var clinVarEntry in sa.ClinVarEntries)
            {
                if (clinVarEntry.SaAltAllele.Equals("iCCCACG"))
                {
                    Assert.Equal(clinVarEntry.ID, "RCV000038416.1");
                    Assert.Equal(clinVarEntry.SnoMedCtID, "254637007");
                    Assert.Equal(clinVarEntry.Significance, "other");
                }
                if (clinVarEntry.SaAltAllele.Equals("iGCCACG"))
                {
                    Assert.Equal(clinVarEntry.ID, "RCV000038419.1");
                    Assert.Equal(clinVarEntry.AlleleOrigin, "somatic");
                   
                }
            }
        }

        [Fact]
        public void SignificanceTest()
        {
            const string vcfLine =
                "4	55593610	rs121913517	T	A,C,G	.	.	RS=121913517;RSPOS=55593610;dbSNPBuildID=133;SSR=0;SAO=3;VP=0x050060000a05000002110124;GENEINFO=KIT:3815;WGT=1;VC=SNV;PM;NSM;REF;ASP;LSD;OM;NOV;CLNALLE=1,2;CLNHGVS=NC_000004.11:g.55593610T>A,NC_000004.11:g.55593610T>C;CLNSRC=ClinVar|OMIM_Allelic_Variant,ClinVar|OMIM_Allelic_Variant;CLNORIGIN=2,1;CLNSRCID=NM_000222.2:c.1676T>A|164920.0014,NM_000222.2:c.1676T>C|164920.0023;CLNSIG=255,5;CLNDSDB=MedGen:OMIM:Orphanet,.;CLNDSDBID=C0238198:606764:ORPHA44890,.;CLNDBN=Gastrointestinal_Stromal_Tumors,Gastrointestinal_stromal_tumor\x2c_familial;CLNREVSTAT=single,single;CLNACC=RCV000014870.3,RCV000014879.24";

            var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

            var sa = new SupplementaryAnnotation();

            foreach (var clinVarItem in clinVarItems)
            {
                clinVarItem.SetSupplementaryAnnotations(sa);
            }

            Assert.Equal(sa.ClinVarEntries[0].ID, "RCV000014870.3");
            Assert.Equal(sa.ClinVarEntries[0].Significance, "other");
            Assert.Equal(sa.ClinVarEntries[1].Significance, "Pathogenic");

        }
    }
}
