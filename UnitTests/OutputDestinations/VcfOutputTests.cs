using System.Collections.Generic;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.OutputDestinations
{
    [Collection("Chromosome 1 collection")]
    public sealed class VcfOutputTests : RandomFileBase
    {
        #region members

        private readonly VcfUtilities _vcfUtilities = new VcfUtilities();

        #endregion

        [Fact]
        public void AlleleFrequency1KgOutputTest()
        {
            var sa = new SupplementaryAnnotation(115256529)
            {
                AlleleSpecificAnnotations =
                {
                    ["C"] = new SupplementaryAnnotation.AlleleSpecificAnnotation
                    {
                        DbSnp = new List<long> {11554290},
						OneKgAllAn = 5008,
						OneKgAllAc = 2130,
						OneKgAmrAn = 694,
						OneKgAmrAc = 250
                    }
                }
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            _vcfUtilities.FieldContains("chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1", saFilename,
                "AF1000G=0.425319", VcfCommon.InfoIndex);
        }

        [Fact]
        public void AllSuppAnnotOutputTest()
        {
            const string altAllele = "C";

            var sa = new SupplementaryAnnotation(115256529)
            {
	            AlleleSpecificAnnotations =
	            {
		            ["C"] = new SupplementaryAnnotation.AlleleSpecificAnnotation
		            {
			            DbSnp = new List<long> {11554290},
			            OneKgAllAn = 5008,
			            OneKgAllAc = 2130,
			            OneKgAmrAn = 694,
			            OneKgAmrAc = 250
		            }
	            }
            };

	        var cosmicItem1 = new CosmicItem("chr1", 115256529, "COSM1000", "T", altAllele, "TP53",
		        new HashSet<CosmicItem.CosmicStudy>() {new CosmicItem.CosmicStudy("", "carcinoma", "oesophagus")},
		        altAllele);
           
            var cosmicItem2 = new CosmicItem("chr1", 115256529, "COSM1001", "T", altAllele, "TP53",
				new HashSet<CosmicItem.CosmicStudy>() { new CosmicItem.CosmicStudy("01", "carcinoma", "large_intestine") },
				altAllele);
			
            sa.AddCosmic(cosmicItem1);
            sa.AddCosmic(cosmicItem2);

            var clinvarItem1 = new ClinVarItem(null, 0, "A", altAllele, 0, "")
            {
                AltAllele    = altAllele,
                SaAltAllele  = altAllele,
                ID           = "NM_003036.3:c.100G>A|164780.0004",
                OrphanetID   = "ORPHA2462",
                Significance = "other"
            };

            sa.ClinVarItems.Add(clinvarItem1);

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            var infoField = _vcfUtilities.GetObservedField("chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1",
                saFilename, VcfCommon.InfoIndex);
            Assert.NotNull(infoField);

            Assert.Contains("AF1000G=0.425319", infoField);
            Assert.Contains("cosmic=1|COSM1000,1|COSM1001", infoField);
            Assert.Contains("clinvar=1|other", infoField);
        }

        [Fact]
        public void ClinVarOutputTest()
        {
            var sa = new SupplementaryAnnotation(115256529);

            var clinvarItem1 = new ClinVarItem(null, 0, "A", "C", 0, "")
            {
                SaAltAllele = "C",
                ID = "NM_003036.3:c.100G>A|164780.0004",
                OrphanetID = "ORPHA2462",
                Significance = "other"
            };

            sa.ClinVarItems.Add(clinvarItem1);

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            _vcfUtilities.FieldContains("chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1", saFilename,
                "clinvar=1|other", VcfCommon.InfoIndex);
        }

        [Fact]
        public void CosmicOutputTest()
        {
            var sa = new SupplementaryAnnotation(115256529);
	        var altAllele = "C";

			var cosmicItem1 = new CosmicItem("chr1", 115256529, "COSM1000", "T", altAllele, "TP53",
							new HashSet<CosmicItem.CosmicStudy>() { new CosmicItem.CosmicStudy("", "carcinoma", "oesophagus") },
							altAllele);

			var cosmicItem2 = new CosmicItem("chr1", 115256529, "COSM1001", "T", altAllele, "TP53",
				new HashSet<CosmicItem.CosmicStudy>() { new CosmicItem.CosmicStudy("01", "carcinoma", "large_intestine") },
				altAllele);

			sa.AddCosmic(cosmicItem1);
            sa.AddCosmic(cosmicItem2);

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            _vcfUtilities.FieldContains("chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1", saFilename,
                "cosmic=1|COSM1000", VcfCommon.InfoIndex);
        }

        [Fact]
        public void DbSnpOutputTest()
        {
            var sa = new SupplementaryAnnotation(115256529)
            {
                AlleleSpecificAnnotations =
                    {
                        ["C"] = new SupplementaryAnnotation.AlleleSpecificAnnotation
                        {
                            DbSnp = new List<long> {11554290}
                        }
                    }
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            _vcfUtilities.FieldContains("chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1", saFilename,
                "rs11554290", VcfCommon.IdIndex);
        }

        [Fact]
        public void EvsOutputTest()
        {
            var sa = new SupplementaryAnnotation(115256529)
            {
                AlleleSpecificAnnotations =
                    {
                        ["C"] = new SupplementaryAnnotation.AlleleSpecificAnnotation
                        {
                            DbSnp = new List<long> {121913237},
                            EvsAll = "0.0001",
                            EvsCoverage = "102",
                            NumEvsSamples = "3456"
                        }
                    }
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            _vcfUtilities.FieldContains("chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1",
                saFilename, "EVS=0.0001|102|3456", VcfCommon.InfoIndex);
        }

        [Fact]
        public void ExistingIdTrimming()
        {
            var sa = new SupplementaryAnnotation(115256529)
            {
                AlleleSpecificAnnotations =
                    {
                        ["C"] = new SupplementaryAnnotation.AlleleSpecificAnnotation
                        {
                            DbSnp = new List<long> {11554290} // dummy rsid for testing only
                        }
                    }
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            _vcfUtilities.FieldContains("chr1\t115256529\tCanvas:LOSS:2:89432494:89444410;rs11554291\tT\tC\t1000\tPASS\t.\tGT\t0/1",
                saFilename, "Canvas:LOSS:2:89432494:89444410;rs11554290", VcfCommon.IdIndex);
        }

        [Fact]
        public void MultiAlleleTest()
        {
            var sa = new SupplementaryAnnotation(4634317)
            {
                AlleleSpecificAnnotations =
                {
                    ["A"] = new SupplementaryAnnotation.AlleleSpecificAnnotation
                    {
                        DbSnp = new List<long> {11078537},
						OneKgAllAn = 5008,
						OneKgAllAc = 2049
					},
                    ["T"] = new SupplementaryAnnotation.AlleleSpecificAnnotation
                    {
                        DbSnp = new List<long> {11078537},
						OneKgAllAn = 5008,
						OneKgAllAc = 1200
					}
                }
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr17", saFilename);

            _vcfUtilities.FieldContains("17\t4634317\trs11078537\tC\tA,T\t256\tPASS\t.\tGT\t0/1",
                saFilename, "AF1000G=0.409145,0.239617", VcfCommon.InfoIndex);
        }

        [Fact]
        public void MultipleDbSnpIds()
        {
            var sa = new SupplementaryAnnotation(115256529)
            {
                AlleleSpecificAnnotations =
                    {
                        ["C"] = new SupplementaryAnnotation.AlleleSpecificAnnotation
                        {
                            DbSnp = new List<long> {111, 222, 333}
                        }
                    }
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            _vcfUtilities.FieldContains("chr1\t115256529\tMantaFluff\tT\tC\t1000\tPASS\t.\tGT\t0/1",
                saFilename, "MantaFluff;rs111;rs222;rs333", VcfCommon.IdIndex);
        }

        [Fact]
        public void NullVcfFieldTest()
        {
            var sa = new SupplementaryAnnotation(9580071)
            {
                AlleleSpecificAnnotations =
                {
                    ["T"] = new SupplementaryAnnotation.AlleleSpecificAnnotation
                    {
                        DbSnp = null
                    }
                }
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr12", saFilename);

            var idField = _vcfUtilities.GetObservedField("chr12\t9580071\t.\tA\tC,T\t394.00\tPASS\t.\tGT\t0/1",
                saFilename, VcfCommon.IdIndex);
            Assert.NotNull(idField);
        }

        [Fact]
        public void OneAlleleFreqMissing()
        {
            var sa = new SupplementaryAnnotation(825069)
            {
	            AlleleSpecificAnnotations =
	            {
		            ["C"] = new SupplementaryAnnotation.AlleleSpecificAnnotation
		            {
			            DbSnp = new List<long> {4475692},
			            OneKgAllAn = 5008,
			            OneKgAllAc = 3392
		            }
	            }
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            _vcfUtilities.FieldContains(
                "chr1	825069	rs4475692	G	A,C	362.00	LowGQX;HighDPFRatio	SNVSB=-36.9;SNVHPOL=3	GT:GQ:GQX:DP:DPF:AD	1/2:4:0:52:38:8,11,33",
                saFilename, "AF1000G=.,0.677316", VcfCommon.InfoIndex);
        }
    }
}