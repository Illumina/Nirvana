using Jasix;
using Xunit;

namespace UnitTests.Jasix
{
    public sealed class JasixFunctionalityTests
    {
        [Fact]
        public void ParsingDeletionJsonLine()
        {
            const string jsonLine =
                "{\"chromosome\":\"chr1\",\"refAllele\":\"GT\",\"position\":2337967,\"altAlleles\":[\"G\"],\"cyt\r\nogeneticBand\":\"1p36.32\",\"variants\":[{\"altAllele\":\"C\",\"refAllele\":\"-\",\"begin\":2337968,\"chromosome\":\"chr1\",\"dbsnp\":[\"rs797044762\"],\"end\":2337967,\"variantType\":\"insertion\",\"vid\":\"1:2337968:2337967:C\",\"regulatoryRegions\":[{\"id\":\"ENSR00001576444\",\"consequence\":[\"regulatory_region_variant\"]}],\"transcripts\":{\"refSeq\":[{\"transcript\":\"XM_005244712.1\",\"bioType\":\"protein_coding\",\"geneId\":\"11079\",\"hgnc\":\"RER1\",\"consequence\":[\"downstream_gene_variant\"],\"proteinId\":\"XP_005244769.1\"},{\"transcript\":\"NM_007033.4\",\"bioType\":\"protein_coding\",\"geneId\":\"11079\",\"hgnc\":\"RER1\",\"consequence\":[\"downstream_gene_variant\"],\"isCanonical\":true,\"proteinId\":\"NP_008964.3\"},{\"transcript\":\"XM_005244713.1\",\"bioType\":\"protein_coding\",\"geneId\":\"11079\",\"hgnc\":\"RER1\",\"consequence\":[\"downstream_gene_variant\"],\"proteinId\":\"XP_005244770.1\"},{\"transcript\":\"NM_002617.3\",\"bioType\":\"protein_coding\",\"aminoAcids\":\"-/X\",\"cDnaPos\":\"936-937\",\"codons\":\"-/G\",\"cdsPos\":\"867-868\",\"exons\":\"5/6\",\"geneId\":\"5192\",\"hgnc\":\"PEX10\",\"consequence\":[\"frameshift_variant\"],\"hgvsc\":\"NM_002617.3:c.867_868insG\",\"hgvsp\":\"NP_002608.1:p.His290AlafsTer49\",\"proteinId\":\"NP_002608.1\",\"proteinPos\":\"289-290\"},{\"transcript\":\"NM_153818.1\",\"bioType\":\"protein_coding\",\"aminoAcids\":\"-/X\",\"cDnaPos\":\"996-997\",\"codons\":\"-/G\",\"cdsPos\":\"927-928\",\"exons\":\"5/6\",\"geneId\":\"5192\",\"hgnc\":\"PEX10\",\"consequence\":[\"frameshift_variant\"],\"hgvsc\":\"NM_153818.1:c.927_928insG\",\"hgvsp\":\"NP_722540.1:p.His310AlafsTer49\",\"isCanonical\":true,\"proteinId\":\"NP_722540.1\",\"proteinPos\":\"309-310\"}]}}]}";

            var chrPos = IndexCreator.GetChromPosition(jsonLine);
            Assert.Equal("chr1", chrPos.Item1);
            Assert.Equal(2337967, chrPos.Item2);
            Assert.Equal(2337968, chrPos.Item3);
        }

        [Fact]
        public void ParsingSnvJsonLine()
        {
            const string jsonLine =
                "{\"chromosome\":\"chr1\",\"refAllele\":\"G\",\"position\":2337967,\"altAlleles\":[\"C\",\"T\"],\"cyt\r\nogeneticBand\":\"1p36.32\",\"variants\":[{\"altAllele\":\"C\",\"refAllele\":\"-\",\"begin\":2337968,\"chromosome\":\"chr1\",\"dbsnp\":[\"rs797044762\"],\"end\":2337967,\"variantType\":\"insertion\",\"vid\":\"1:2337968:2337967:C\",\"regulatoryRegions\":[{\"id\":\"ENSR00001576444\",\"consequence\":[\"regulatory_region_variant\"]}],\"transcripts\":{\"refSeq\":[{\"transcript\":\"XM_005244712.1\",\"bioType\":\"protein_coding\",\"geneId\":\"11079\",\"hgnc\":\"RER1\",\"consequence\":[\"downstream_gene_variant\"],\"proteinId\":\"XP_005244769.1\"},{\"transcript\":\"NM_007033.4\",\"bioType\":\"protein_coding\",\"geneId\":\"11079\",\"hgnc\":\"RER1\",\"consequence\":[\"downstream_gene_variant\"],\"isCanonical\":true,\"proteinId\":\"NP_008964.3\"},{\"transcript\":\"XM_005244713.1\",\"bioType\":\"protein_coding\",\"geneId\":\"11079\",\"hgnc\":\"RER1\",\"consequence\":[\"downstream_gene_variant\"],\"proteinId\":\"XP_005244770.1\"},{\"transcript\":\"NM_002617.3\",\"bioType\":\"protein_coding\",\"aminoAcids\":\"-/X\",\"cDnaPos\":\"936-937\",\"codons\":\"-/G\",\"cdsPos\":\"867-868\",\"exons\":\"5/6\",\"geneId\":\"5192\",\"hgnc\":\"PEX10\",\"consequence\":[\"frameshift_variant\"],\"hgvsc\":\"NM_002617.3:c.867_868insG\",\"hgvsp\":\"NP_002608.1:p.His290AlafsTer49\",\"proteinId\":\"NP_002608.1\",\"proteinPos\":\"289-290\"},{\"transcript\":\"NM_153818.1\",\"bioType\":\"protein_coding\",\"aminoAcids\":\"-/X\",\"cDnaPos\":\"996-997\",\"codons\":\"-/G\",\"cdsPos\":\"927-928\",\"exons\":\"5/6\",\"geneId\":\"5192\",\"hgnc\":\"PEX10\",\"consequence\":[\"frameshift_variant\"],\"hgvsc\":\"NM_153818.1:c.927_928insG\",\"hgvsp\":\"NP_722540.1:p.His310AlafsTer49\",\"isCanonical\":true,\"proteinId\":\"NP_722540.1\",\"proteinPos\":\"309-310\"}]}}]}";

            var chrPos = IndexCreator.GetChromPosition(jsonLine);
            Assert.Equal("chr1", chrPos.Item1);
            Assert.Equal(2337967, chrPos.Item2);
            Assert.Equal(2337967, chrPos.Item3);
        }

        [Fact]
        public void ParsingJsonInsertionLine()
        {
            const string jsonLine =
                "{\"chromosome\":\"chr1\",\"refAllele\":\"G\",\"position\":2337967,\"altAlleles\":[\"GCC\"],\"cyt\r\nogeneticBand\":\"1p36.32\",\"variants\":[{\"altAllele\":\"C\",\"refAllele\":\"-\",\"begin\":2337968,\"chromosome\":\"chr1\",\"dbsnp\":[\"rs797044762\"],\"end\":2337967,\"variantType\":\"insertion\",\"vid\":\"1:2337968:2337967:C\",\"regulatoryRegions\":[{\"id\":\"ENSR00001576444\",\"consequence\":[\"regulatory_region_variant\"]}],\"transcripts\":{\"refSeq\":[{\"transcript\":\"XM_005244712.1\",\"bioType\":\"protein_coding\",\"geneId\":\"11079\",\"hgnc\":\"RER1\",\"consequence\":[\"downstream_gene_variant\"],\"proteinId\":\"XP_005244769.1\"},{\"transcript\":\"NM_007033.4\",\"bioType\":\"protein_coding\",\"geneId\":\"11079\",\"hgnc\":\"RER1\",\"consequence\":[\"downstream_gene_variant\"],\"isCanonical\":true,\"proteinId\":\"NP_008964.3\"},{\"transcript\":\"XM_005244713.1\",\"bioType\":\"protein_coding\",\"geneId\":\"11079\",\"hgnc\":\"RER1\",\"consequence\":[\"downstream_gene_variant\"],\"proteinId\":\"XP_005244770.1\"},{\"transcript\":\"NM_002617.3\",\"bioType\":\"protein_coding\",\"aminoAcids\":\"-/X\",\"cDnaPos\":\"936-937\",\"codons\":\"-/G\",\"cdsPos\":\"867-868\",\"exons\":\"5/6\",\"geneId\":\"5192\",\"hgnc\":\"PEX10\",\"consequence\":[\"frameshift_variant\"],\"hgvsc\":\"NM_002617.3:c.867_868insG\",\"hgvsp\":\"NP_002608.1:p.His290AlafsTer49\",\"proteinId\":\"NP_002608.1\",\"proteinPos\":\"289-290\"},{\"transcript\":\"NM_153818.1\",\"bioType\":\"protein_coding\",\"aminoAcids\":\"-/X\",\"cDnaPos\":\"996-997\",\"codons\":\"-/G\",\"cdsPos\":\"927-928\",\"exons\":\"5/6\",\"geneId\":\"5192\",\"hgnc\":\"PEX10\",\"consequence\":[\"frameshift_variant\"],\"hgvsc\":\"NM_153818.1:c.927_928insG\",\"hgvsp\":\"NP_722540.1:p.His310AlafsTer49\",\"isCanonical\":true,\"proteinId\":\"NP_722540.1\",\"proteinPos\":\"309-310\"}]}}]}";

            var chrPos = IndexCreator.GetChromPosition(jsonLine);
            Assert.Equal("chr1", chrPos.Item1);
            Assert.Equal(2337967, chrPos.Item2);
            Assert.Equal(2337968, chrPos.Item3);
        }

        [Fact]
        public void ParseJsonStructuralVariant()
        {
            const string jsonLine =
                "{\"chromosome\":\"chr3\",\"refAllele\":\"A\",\"position\":62431401,\"svEnd\":62431801,\"altAlleles\":[\"<DEL>\"],\"cytogeneticBand\":\"3p14.2\",\"variants\":[{\"altAllele\":\"<DEL>\",\"refAllele\":\"A\",\"begin\":62431402,\"chromosome\":\"chr3\",\"end\":62431801,\"variantType\":\"unknown\",\"vid\":\"3:62431402:62431401\",\"globalAllele\":{\"globalMajorAllele\":\"C\",\"globalMajorAlleleFrequency\":0.9856,\"globalMinorAllele\":\"A\",\"globalMinorAlleleFrequency\":0.01438}}]}";

            var chrPos = IndexCreator.GetChromPosition(jsonLine);
            Assert.Equal("chr3", chrPos.Item1);
            Assert.Equal(62431401, chrPos.Item2);
            Assert.Equal(62431801, chrPos.Item3);
        }

        [Fact]
        public void ParseJsonBreakEnd()
        {
            const string jsonLine =
                "{\"chromosome\":\"2\",\"refAllele\":\"G\",\"position\":321681,\"quality\":6,\"filters\":[\"PASS\"],\"altAlleles\":[\"G]2:421681]\"],\"cytogeneticBand\":\"2p25.3\",\"oneKg\":[{\"chromosome\":\"2\",\"begin\":314969,\"end\":694521,\"variantType\":\"copy_number_gain\",\"variantFreqAll\":0.0008,\"variantFreqEas\":0.00397,\"id\":\"esv3589600\",\"sampleSize\":2504,\"sampleSizeAfr\":661,\"sampleSizeAmr\":347,\"sampleSizeEas\":504,\"sampleSizeEur\":503,\"sampleSizeSas\":489,\"observedGains\":2}],\"variants\":[{\"altAllele\":\"G]2:421681]\",\"refAllele\":\"G\",\"begin\":321681,\"chromosome\":\"2\",\"end\":321686,\"variantType\":\"translocation_breakend\",\"vid\":\"2:321681:+:2:421681:-\",\"overlappingGenes\":[\"AC079779.6\"]}]}";

            var chrPos = IndexCreator.GetChromPosition(jsonLine);
            Assert.Equal("2", chrPos.Item1);
            Assert.Equal(321681, chrPos.Item2);
            Assert.Equal(321681, chrPos.Item3);
        }
    }
}
