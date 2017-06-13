using Xunit;

namespace UnitTests.Output
{
    public sealed class VcfOutputTests
    {
        [Fact(Skip = "class change")]
        public void AlleleFrequency1KgOutputTest()
        {
            //var sa = new SupplementaryAnnotationPosition(115256529);

            //var oneKg = new OneKGenAnnotation
            //{
            //    OneKgAllAn = 5008,
            //    OneKgAllAc = 2130,
            //    OneKgAmrAn = 694,
            //    OneKgAmrAc = 250
            //};

            //var dbSnp = new DbSnpAnnotation
            //{
            //    DbSnp = new List<long> { 11554290 }
            //};

            //var saCreator = new SupplementaryPositionCreator(sa);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.OneKg, "C", oneKg);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, "C", dbSnp);

            //var saReader = new MockSupplementaryAnnotationReader(sa);
            //VcfUtilities.FieldContains(saReader,
            //    "chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1", "AF1000G=0.425319", VcfCommon.InfoIndex);
        }

        [Fact(Skip = "class change")]
        public void AllSuppAnnotOutputTest()
        {
            //const string altAllele = "C";

            //var sa = new SupplementaryAnnotationPosition(115256529);

            //var oneKg = new OneKGenAnnotation
            //{
            //    OneKgAllAn = 5008,
            //    OneKgAllAc = 2130,
            //    OneKgAmrAn = 694,
            //    OneKgAmrAc = 250
            //};

            //var dbSnp = new DbSnpAnnotation
            //{
            //    DbSnp = new List<long> { 11554290 }
            //};

            //var saCreator = new SupplementaryPositionCreator(sa);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.OneKg, "C", oneKg);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, "C", dbSnp);

            //var cosmicItem1 = new CosmicItem("chr1", 115256529, "COSM1000", "T", altAllele, "TP53",
            //    new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy("", "carcinoma", "oesophagus") }, 1,
            //    altAllele);

            //var cosmicItem2 = new CosmicItem("chr1", 115256529, "COSM1001", "T", altAllele, "TP53",
            //    new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy("01", "carcinoma", "large_intestine") }, 1,
            //    altAllele);

            //cosmicItem1.AddCosmicToSa(saCreator);
            //cosmicItem2.AddCosmicToSa(saCreator);

            //var clinvarItem1 = new ClinVarItem(null, 0, null, altAllele, null, "RCV001",
            //    null, null, new List<string> { "ORPHA2462" }, null, null, "other");

            //sa.ClinVarItems.Add(clinvarItem1);

            //var saReader = new MockSupplementaryAnnotationReader(sa);
            //var infoColumn = VcfUtilities.GetVcfColumn(saReader,
            //    "chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1", VcfCommon.InfoIndex);

            //Assert.Contains("AF1000G=0.425319", infoColumn);
            //Assert.Contains("cosmic=1|COSM1000,1|COSM1001", infoColumn);
            //Assert.Contains("clinvar=1|other", infoColumn);
        }

        [Fact(Skip = "class change")]
        public void ClinVarOutputTest()
        {
            //var sa = new SupplementaryAnnotationPosition(115256529);

            //var clinvarItem1 = new ClinVarItem(null, 0, null, "C", null, "RCV001",
            //    null, null, new List<string> { "ORPHA2462" }, null, null, "other");

            //sa.ClinVarItems.Add(clinvarItem1);

            //var saReader = new MockSupplementaryAnnotationReader(sa);
            //VcfUtilities.FieldContains(saReader,
            //    "chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1", "clinvar=1|other", VcfCommon.InfoIndex);
        }

        [Fact(Skip = "class change")]
        public void CosmicOutputTest()
        {
            //var sa = new SupplementaryAnnotationPosition(115256529);
            //var saCreator = new SupplementaryPositionCreator(sa);
            //var altAllele = "C";

            //var cosmicItem1 = new CosmicItem("chr1", 115256529, "COSM1000", "T", altAllele, "TP53",
            //                new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy("", "carcinoma", "oesophagus") }, 1,
            //                altAllele);

            //var cosmicItem2 = new CosmicItem("chr1", 115256529, "COSM1001", "T", altAllele, "TP53",
            //    new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy("01", "carcinoma", "large_intestine") }, 1,
            //    altAllele);

            //cosmicItem1.AddCosmicToSa(saCreator);
            //cosmicItem2.AddCosmicToSa(saCreator);

            //var saReader = new MockSupplementaryAnnotationReader(sa);
            //VcfUtilities.FieldContains(saReader,
            //    "chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1", "cosmic=1|COSM1000", VcfCommon.InfoIndex);
        }

        [Fact(Skip = "class change")]
        public void DbSnpOutputTest()
        {
            //var sa = new SupplementaryAnnotationPosition(115256529);

            //var dbSnp = new DbSnpAnnotation
            //{
            //    DbSnp = new List<long> { 11554290 }
            //};

            //var saCreator = new SupplementaryPositionCreator(sa);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, "C", dbSnp);

            //var saReader = new MockSupplementaryAnnotationReader(sa);
            //VcfUtilities.FieldContains(saReader,
            //    "chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1", "rs11554290", VcfCommon.IdIndex);
        }

        [Fact(Skip = "class change")]
        public void EvsOutputTest()
        {
            //var sa = new SupplementaryAnnotationPosition(115256529);

            //var dbSnp = new DbSnpAnnotation
            //{
            //    DbSnp = new List<long> { 121913237 }
            //};
            //var evs = new EvsAnnotation
            //{
            //    EvsAll = "0.0001",
            //    EvsCoverage = "102",
            //    NumEvsSamples = "3456"
            //};

            //var saCreator = new SupplementaryPositionCreator(sa);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.Evs, "C", evs);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, "C", dbSnp);

            //var saReader = new MockSupplementaryAnnotationReader(sa);
            //VcfUtilities.FieldContains(saReader,
            //    "chr1\t115256529\t.\tT\tC\t1000\tPASS\t.\tGT\t0/1", "EVS=0.0001|102|3456", VcfCommon.InfoIndex);
        }

        [Fact(Skip = "class change")]
        public void ExistingIdTrimming()
        {
            //var sa = new SupplementaryAnnotationPosition(115256529);

            //var dbSnp = new DbSnpAnnotation
            //{
            //    DbSnp = new List<long> { 11554290 }
            //};

            //var saCreator = new SupplementaryPositionCreator(sa);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, "C", dbSnp);

            //var saReader = new MockSupplementaryAnnotationReader(sa);
            //VcfUtilities.FieldContains(saReader,
            //    "chr1\t115256529\tCanvas:LOSS:2:89432494:89444410;rs11554291\tT\tC\t1000\tPASS\t.\tGT\t0/1",
            //    "Canvas:LOSS:2:89432494:89444410;rs11554290", VcfCommon.IdIndex);
        }

        [Fact(Skip = "class change")]
        public void MultiAlleleTest()
        {
            //var sa = new SupplementaryAnnotationPosition(4634317);

            //var oneKg1 = new OneKGenAnnotation
            //{
            //    AncestralAllele = "C",
            //    OneKgAllAn = 5008,
            //    OneKgAllAc = 2049
            //};

            //var oneKg2 = new OneKGenAnnotation
            //{
            //    AncestralAllele = "C",
            //    OneKgAllAn = 5008,
            //    OneKgAllAc = 1200
            //};
            //var dbSnp = new DbSnpAnnotation
            //{
            //    DbSnp = new List<long> { 11078537 }
            //};

            //var saCreator = new SupplementaryPositionCreator(sa);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, "A", dbSnp);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, "T", dbSnp);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.OneKg, "A", oneKg1);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.OneKg, "T", oneKg2);

            //var saReader = new MockSupplementaryAnnotationReader(sa);
            //VcfUtilities.FieldContains(saReader,
            //    "17\t4634317\trs11078537\tC\tA,T\t256\tPASS\t.\tGT\t0/1", "AF1000G=0.409145,0.239617", VcfCommon.InfoIndex);
        }

        [Fact(Skip = "class change")]
        public void MultipleDbSnpIds()
        {
            //var sa = new SupplementaryAnnotationPosition(115256529);

            //var dbSnp = new DbSnpAnnotation
            //{
            //    DbSnp = new List<long> { 111, 222, 333 }
            //};

            //var saCreator = new SupplementaryPositionCreator(sa);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, "C", dbSnp);

            //var saReader = new MockSupplementaryAnnotationReader(sa);
            //VcfUtilities.FieldContains(saReader,
            //    "chr1\t115256529\tMantaFluff\tT\tC\t1000\tPASS\t.\tGT\t0/1", "MantaFluff;rs111;rs222;rs333",
            //    VcfCommon.IdIndex);
        }

        [Fact(Skip = "class change")]
        public void NullVcfFieldTest()
        {
            //var sa = new SupplementaryAnnotationPosition(9580071);

            //var dbSnp = new DbSnpAnnotation
            //{
            //    DbSnp = null
            //};

            //var saCreator = new SupplementaryPositionCreator(sa);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, "T", dbSnp);

            //var saReader = new MockSupplementaryAnnotationReader(sa);
            //var idColumn = VcfUtilities.GetVcfColumn(saReader,
            //    "chr12\t9580071\t.\tA\tC,T\t394.00\tPASS\t.\tGT\t0/1", VcfCommon.IdIndex);
            //Assert.NotNull(idColumn);
        }

        [Fact(Skip = "class change")]
        public void OneAlleleFreqMissing()
        {
            //var sa = new SupplementaryAnnotationPosition(825069);

            //var saCreator = new SupplementaryPositionCreator(sa);

            //var dbSnp = new DbSnpAnnotation
            //{
            //    DbSnp = new List<long> { 4475692 }
            //};

            //var oneKg = new OneKGenAnnotation
            //{
            //    OneKgAllAn = 5008,
            //    OneKgAllAc = 3392
            //};

            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, "C", dbSnp);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.OneKg, "C", oneKg);

            //var saReader = new MockSupplementaryAnnotationReader(sa);
            //VcfUtilities.FieldContains(saReader,
            //    "chr1	825069	rs4475692	G	A,C	362.00	LowGQX;HighDPFRatio	SNVSB=-36.9;SNVHPOL=3	GT:GQ:GQX:DP:DPF:AD	1/2:4:0:52:38:8,11,33",
            //    "AF1000G=.,0.677316", VcfCommon.InfoIndex);
        }
    }
}