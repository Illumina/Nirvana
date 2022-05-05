using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;
using SAUtils.gnomAD;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.gnomAD;

public sealed class GnomadSvTsvParserTests
{
    private static Stream GetStreamData(string dataString)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(dataString);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public void TestGnomadSvTsvParser()
    {
        const string tsvData =
            "#variant_call_accession\tvariant_call_id\tvariant_call_type\texperiment_id\tsample_id\tsampleset_id\tassembly\tchrcontig\touter_start\tstart\tinner_start\tinner_stop\tstop\touter_stop\tinsertion_length\tvariant_region_acc\tvariant_region_id\tcopy_number\tdescription\tvalidation\tzygosity\torigin\tphenotype\thgvs_name\tplacement_method\tplacement_rank\tplacements_per_assembly\tremap_alignment\tremap_best_within_cluster\tremap_coverage\tremap_diff_chr\tremap_failure_code\tallele_count\tallele_frequency\tallele_number\n" +
            "nssv15777856\tgnomAD-SV_v2.1_CNV_10_564_alt_1\tcopy number variation\t1\t\t1\tGRCh38.p12\t10\t\t\t736806\t\t\t738184\t\t\tnsv4039284\t10__782746___784124______GRCh37.p13_copy_number_variation\t0\t\t\t\t\t\t\tRemapped\tBestAvailable\tSingle\tFirst Pass\t0\t1\t\t\tAC=21,AFR_AC=10,AMR_AC=9,EAS_AC=0,EUR_AC=2,OTH_AC=0\tAF=0.038889,AFR_AF=0.044643,AMR_AF=0.03913,EAS_AF=0,EUR_AF=0.023256,OTH_AF=0\tAN=540,AFR_AN=224,AMR_AN=230,EAS_AN=0,EUR_AN=86,OTH_AN=0\n" +
            "nssv15777857\tgnomAD-SV_v2.1_CNV_10_564_alt_10\talu insertion\t1\t\t1\tGRCh38.p12\t10\t\t\t736806\t\t\t738184\t\t\tnsv4039284\t10__782746___784124______GRCh37.p13_copy_number_variation\t9\t\t\t\t\t\t\tRemapped\tBestAvailable\tSingle\tFirst Pass\t0\t1\t\t\tAC=0,AFR_AC=0,AMR_AC=0,EAS_AC=0,EUR_AC=0,OTH_AC=0\tAF=0,AFR_AF=0,AMR_AF=0,EAS_AF=0,EUR_AF=0,OTH_AF=0\tAN=540,AFR_AN=224,AMR_AN=230,EAS_AN=0,EUR_AN=86,OTH_AN=0\n" +
            "nssv15777858\tgnomAD-SV_v2.1_CNV_10_564_alt_11\tdeletion\t1\t\t1\tGRCh38.p12\t10\t\t\t736806\t\t\t738184\t\t\tnsv4039284\t10__782746___784124______GRCh37.p13_copy_number_variation\t10\t\t\t\t\t\t\tRemapped\tBestAvailable\tSingle\tFirst Pass\t0\t1\t\t\tAC=0,AFR_AC=0,AMR_AC=0,EAS_AC=0,EUR_AC=0,OTH_AC=0\tAF=0,AFR_AF=0,AMR_AF=0,EAS_AF=0,EUR_AF=0,OTH_AF=0\tAN=540,AFR_AN=224,AMR_AN=230,EAS_AN=0,EUR_AN=86,OTH_AN=0\n" +
            "nssv15982321\tgnomAD-SV_v2.1_INS_11_75807\tinsertion\t1\t\t1\tGRCh38.p12\t11\t\t\t11946244\t\t\t11946244\t\t58\tnsv4549918\t11__11967791___11967792______GRCh37.p13_insertion\t\t\t\t\t\t\tNC_000011.10:g.11946244_11946245ins58\tRemapped\tBestAvailable\tSingle\tFirst Pass\t0\t1\t\t\tAC=1,AFR_AC=0,AMR_AC=1,EAS_AC=0,EUR_AC=0,OTH_AC=0\tAF=4.6e-05,AFR_AF=0,AMR_AF=0.000518,EAS_AF=0,EUR_AF=0,OTH_AF=0\tAN=21694,AFR_AN=9534,AMR_AN=1930,EAS_AN=2416,EUR_AN=7624,OTH_AN=190\n";
        
        using var reader         = new StreamReader(GetStreamData(tsvData));
        using var gnomadSvParser = new GnomadSvTsvParser(reader, ChromosomeUtilities.RefNameToChromosome);

        List<GnomadSvItem> svItemList = gnomadSvParser.GetItems().ToList();

        Assert.Equal(4, svItemList.Count);

        Assert.Equal(
            "\"chromosome\":\"10\",\"begin\":736807,\"end\":738184,\"variantId\":\"gnomAD-SV_v2.1_CNV_10_564_alt_1\",\"variantType\":\"copy_number_variation\",\"allAf\":0.038889,\"afrAf\":0.044643,\"amrAf\":0.03913,\"easAf\":0,\"eurAf\":0.023256,\"othAf\":0,\"allAc\":21,\"afrAc\":10,\"amrAc\":9,\"easAc\":0,\"eurAc\":2,\"othAc\":0,\"allAn\":540,\"afrAn\":224,\"amrAn\":230,\"easAn\":0,\"eurAn\":86,\"othAn\":0",
            svItemList[0].GetJsonString()
        );
        Assert.Equal(
            "\"chromosome\":\"10\",\"begin\":736807,\"end\":738184,\"variantId\":\"gnomAD-SV_v2.1_CNV_10_564_alt_10\",\"variantType\":\"mobile_element_insertion\",\"allAf\":0,\"afrAf\":0,\"amrAf\":0,\"easAf\":0,\"eurAf\":0,\"othAf\":0,\"allAc\":0,\"afrAc\":0,\"amrAc\":0,\"easAc\":0,\"eurAc\":0,\"othAc\":0,\"allAn\":540,\"afrAn\":224,\"amrAn\":230,\"easAn\":0,\"eurAn\":86,\"othAn\":0",
            svItemList[1].GetJsonString()
        );
        Assert.Equal(
            "\"chromosome\":\"10\",\"begin\":736807,\"end\":738184,\"variantId\":\"gnomAD-SV_v2.1_CNV_10_564_alt_11\",\"variantType\":\"deletion\",\"allAf\":0,\"afrAf\":0,\"amrAf\":0,\"easAf\":0,\"eurAf\":0,\"othAf\":0,\"allAc\":0,\"afrAc\":0,\"amrAc\":0,\"easAc\":0,\"eurAc\":0,\"othAc\":0,\"allAn\":540,\"afrAn\":224,\"amrAn\":230,\"easAn\":0,\"eurAn\":86,\"othAn\":0",
            svItemList[2].GetJsonString()
        );
        Assert.Equal(
            "\"chromosome\":\"11\",\"begin\":11946245,\"end\":11946244,\"variantId\":\"gnomAD-SV_v2.1_INS_11_75807\",\"variantType\":\"insertion\",\"allAf\":0.000046,\"afrAf\":0,\"amrAf\":0.000518,\"easAf\":0,\"eurAf\":0,\"othAf\":0,\"allAc\":1,\"afrAc\":0,\"amrAc\":1,\"easAc\":0,\"eurAc\":0,\"othAc\":0,\"allAn\":21694,\"afrAn\":9534,\"amrAn\":1930,\"easAn\":2416,\"eurAn\":7624,\"othAn\":190",
            svItemList[3].GetJsonString()
        );
    }

    [Fact]
    public void TestUnknownChromosome()
    {
        const string tsvData =
            "#variant_call_accession\tvariant_call_id\tvariant_call_type\texperiment_id\tsample_id\tsampleset_id\tassembly\tchrcontig\touter_start\tstart\tinner_start\tinner_stop\tstop\touter_stop\tinsertion_length\tvariant_region_acc\tvariant_region_id\tcopy_number\tdescription\tvalidation\tzygosity\torigin\tphenotype\thgvs_name\tplacement_method\tplacement_rank\tplacements_per_assembly\tremap_alignment\tremap_best_within_cluster\tremap_coverage\tremap_diff_chr\tremap_failure_code\tallele_count\tallele_frequency\tallele_number\n" +
            "nssv15777856\tgnomAD-SV_v2.1_CNV_10_564_alt_1\tcopy number variation\t1\t\t1\tGRCh38.p12\tINVALID-1\t\t\t736806\t\t\t738184\t\t\tnsv4039284\t10__782746___784124______GRCh37.p13_copy_number_variation\t0\t\t\t\t\t\t\tRemapped\tBestAvailable\tSingle\tFirst Pass\t0\t1\t\t\tAC=21,AFR_AC=10,AMR_AC=9,EAS_AC=0,EUR_AC=2,OTH_AC=0\tAF=0.038889,AFR_AF=0.044643,AMR_AF=0.03913,EAS_AF=0,EUR_AF=0.023256,OTH_AF=0\tAN=540,AFR_AN=224,AMR_AN=230,EAS_AN=0,EUR_AN=86,OTH_AN=0\n" +
            "nssv15777857\tgnomAD-SV_v2.1_CNV_10_564_alt_10\tduplication\t1\t\t1\tGRCh38.p12\t10\t\t\t736806\t\t\t738184\t\t\tnsv4039284\t10__782746___784124______GRCh37.p13_copy_number_variation\t9\t\t\t\t\t\t\tRemapped\tBestAvailable\tSingle\tFirst Pass\t0\t1\t\t\tAC=0,AFR_AC=0,AMR_AC=0,EAS_AC=0,EUR_AC=0,OTH_AC=0\tAF=0,AFR_AF=0,AMR_AF=0,EAS_AF=0,EUR_AF=0,OTH_AF=0\tAN=540,AFR_AN=224,AMR_AN=230,EAS_AN=0,EUR_AN=86,OTH_AN=0\n";

        using var reader         = new StreamReader(GetStreamData(tsvData));
        using var gnomadSvParser = new GnomadSvTsvParser(reader, ChromosomeUtilities.RefNameToChromosome);

        List<GnomadSvItem> svItemList = gnomadSvParser.GetItems().ToList();

        Assert.Single(svItemList);


        Assert.Equal(
            "\"chromosome\":\"10\",\"begin\":736807,\"end\":738184,\"variantId\":\"gnomAD-SV_v2.1_CNV_10_564_alt_10\",\"variantType\":\"duplication\",\"allAf\":0,\"afrAf\":0,\"amrAf\":0,\"easAf\":0,\"eurAf\":0,\"othAf\":0,\"allAc\":0,\"afrAc\":0,\"amrAc\":0,\"easAc\":0,\"eurAc\":0,\"othAc\":0,\"allAn\":540,\"afrAn\":224,\"amrAn\":230,\"easAn\":0,\"eurAn\":86,\"othAn\":0",
            svItemList[0].GetJsonString()
        );
    }

    [Fact]
    public void TestInvalidStart()
    {
        const string tsvData =
            "#variant_call_accession\tvariant_call_id\tvariant_call_type\texperiment_id\tsample_id\tsampleset_id\tassembly\tchrcontig\touter_start\tstart\tinner_start\tinner_stop\tstop\touter_stop\tinsertion_length\tvariant_region_acc\tvariant_region_id\tcopy_number\tdescription\tvalidation\tzygosity\torigin\tphenotype\thgvs_name\tplacement_method\tplacement_rank\tplacements_per_assembly\tremap_alignment\tremap_best_within_cluster\tremap_coverage\tremap_diff_chr\tremap_failure_code\tallele_count\tallele_frequency\tallele_number\n" +            "nssv15777856\tgnomAD-SV_v2.1_CNV_10_564_alt_1\tcopy number variation\t1\t\t1\tGRCh38.p12\t10\t\t\tInvalid-736806\t\t\t738184\t\t\tnsv4039284\t10__782746___784124______GRCh37.p13_copy_number_variation\t0\t\t\t\t\t\t\tRemapped\tBestAvailable\tSingle\tFirst Pass\t0\t1\t\t\tAC=21,AFR_AC=10,AMR_AC=9,EAS_AC=0,EUR_AC=2,OTH_AC=0\tAF=0.038889,AFR_AF=0.044643,AMR_AF=0.03913,EAS_AF=0,EUR_AF=0.023256,OTH_AF=0\tAN=540,AFR_AN=224,AMR_AN=230,EAS_AN=0,EUR_AN=86,OTH_AN=0\n";

        using var reader         = new StreamReader(GetStreamData(tsvData));
        using var gnomadSvParser = new GnomadSvTsvParser(reader, ChromosomeUtilities.RefNameToChromosome);

        Assert.Throws<InvalidDataException>(() => gnomadSvParser.GetItems().ToList());
    }

    [Fact]
    public void TestInvalidEnd()
    {
        const string tsvData =
            "#variant_call_accession\tvariant_call_id\tvariant_call_type\texperiment_id\tsample_id\tsampleset_id\tassembly\tchrcontig\touter_start\tstart\tinner_start\tinner_stop\tstop\touter_stop\tinsertion_length\tvariant_region_acc\tvariant_region_id\tcopy_number\tdescription\tvalidation\tzygosity\torigin\tphenotype\thgvs_name\tplacement_method\tplacement_rank\tplacements_per_assembly\tremap_alignment\tremap_best_within_cluster\tremap_coverage\tremap_diff_chr\tremap_failure_code\tallele_count\tallele_frequency\tallele_number\n" +            "nssv15777856\tgnomAD-SV_v2.1_CNV_10_564_alt_1\tcopy number variation\t1\t\t1\tGRCh38.p12\t10\t\t\t736806\t\t\tInvalid-738184\t\t\tnsv4039284\t10__782746___784124______GRCh37.p13_copy_number_variation\t0\t\t\t\t\t\t\tRemapped\tBestAvailable\tSingle\tFirst Pass\t0\t1\t\t\tAC=21,AFR_AC=10,AMR_AC=9,EAS_AC=0,EUR_AC=2,OTH_AC=0\tAF=0.038889,AFR_AF=0.044643,AMR_AF=0.03913,EAS_AF=0,EUR_AF=0.023256,OTH_AF=0\tAN=540,AFR_AN=224,AMR_AN=230,EAS_AN=0,EUR_AN=86,OTH_AN=0\n";

        using var reader         = new StreamReader(GetStreamData(tsvData));
        using var gnomadSvParser = new GnomadSvTsvParser(reader, ChromosomeUtilities.RefNameToChromosome);

        Assert.Throws<InvalidDataException>(() => gnomadSvParser.GetItems().ToList());
    }

    [Fact]
    public void TestInvalidSvType()
    {
        const string tsvData =
            "#variant_call_accession\tvariant_call_id\tvariant_call_type\texperiment_id\tsample_id\tsampleset_id\tassembly\tchrcontig\touter_start\tstart\tinner_start\tinner_stop\tstop\touter_stop\tinsertion_length\tvariant_region_acc\tvariant_region_id\tcopy_number\tdescription\tvalidation\tzygosity\torigin\tphenotype\thgvs_name\tplacement_method\tplacement_rank\tplacements_per_assembly\tremap_alignment\tremap_best_within_cluster\tremap_coverage\tremap_diff_chr\tremap_failure_code\tallele_count\tallele_frequency\tallele_number\n" +
            "nssv15777856\tgnomAD-SV_v2.1_CNV_10_564_alt_1\tINVALID copy number variation\t1\t\t1\tGRCh38.p12\t10\t\t\t736806\t\t\t738184\t\t\tnsv4039284\t10__782746___784124______GRCh37.p13_copy_number_variation\t0\t\t\t\t\t\t\tRemapped\tBestAvailable\tSingle\tFirst Pass\t0\t1\t\t\tAC=21,AFR_AC=10,AMR_AC=9,EAS_AC=0,EUR_AC=2,OTH_AC=0\tAF=0.038889,AFR_AF=0.044643,AMR_AF=0.03913,EAS_AF=0,EUR_AF=0.023256,OTH_AF=0\tAN=540,AFR_AN=224,AMR_AN=230,EAS_AN=0,EUR_AN=86,OTH_AN=0\n";

        using var reader         = new StreamReader(GetStreamData(tsvData));
        using var gnomadSvParser = new GnomadSvTsvParser(reader, ChromosomeUtilities.RefNameToChromosome);

        Assert.Throws<InvalidDataException>(() => gnomadSvParser.GetItems().ToList());
    }
}