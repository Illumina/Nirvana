using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.GnomadGeneScores;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class GnomadGeneScoreTests
    {
        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("gene\ttranscript\tobs_mis\texp_mis\toe_mis\tmu_mis\tpossible_mis\tobs_mis_pphen\texp_mis_pphen\toe_mis_pphen\tpossible_mis_pphen\tobs_syn\texp_syn\toe_syn\tmu_syn\tpossible_syn\tobs_lof\tmu_lof\tpossible_lof\texp_lof\tpLI\tpNull\tpRec\toe_lof\toe_syn_lower\toe_syn_upper\toe_mis_lower\toe_mis_upper\toe_lof_lower\toe_lof_upper\tconstraint_flag\tsyn_z\tmis_z\tlof_z\toe_lof_upper_rank\toe_lof_upper_bin\toe_lof_upper_bin_6\tn_sites\tclassic_caf\tmax_af\tno_lofs\tobs_het_lof\tobs_hom_lof\tdefined\tp\texp_hom_lof\tclassic_caf_afr\tclassic_caf_amr\tclassic_caf_asj\tclassic_caf_eas\tclassic_caf_fin\tclassic_caf_nfe\tclassic_caf_oth\tclassic_caf_sas\tp_afr\tp_amr\tp_asj\tp_eas\tp_fin\tp_nfe\tp_oth\tp_sas\ttranscript_type\tgene_id\ttranscript_level\tcds_length\tnum_coding_exons\tgene_type\tgene_length\texac_pLI\texac_obs_lof\texac_exp_lof\texac_oe_lof\tbrain_expression\tchromosome\tstart_position\tend_position");
            writer.WriteLine("MED13\tENST00000397786\t871\t1.1178e+03\t7.7921e-01\t5.5598e-05\t14195\t314\t5.2975e+02\t5.9273e-01\t6708\t422\t3.8753e+02\t1.0890e+00\t1.9097e-05\t4248\t0\t4.9203e-06\t1257\t9.8429e+01\t1.0000e+00\t8.9436e-40\t1.8383e-16\t0.0000e+00\t1.0050e+00\t1.1800e+00\t7.3600e-01\t8.2400e-01\t0.0000e+00\t3.0000e-02\t\t-1.3765e+00\t2.6232e+00\t9.1935e+00\t0\t0\t0\t2\t1.2058e-05\t8.0492e-06\t124782\t3\t0\t124785\t1.2021e-05\t1.8031e-05\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t9.2812e-05\t8.8571e-06\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t9.2760e-05\t8.8276e-06\t0.0000e+00\t0.0000e+00\tprotein_coding\tENSG00000108510\t2\t6522\t30\tprotein_coding\t122678\t1.0000e+00\t0\t6.4393e+01\t0.0000e+00\tNA\t17\t60019966\t60142643");
            writer.WriteLine("NIPBL\tENST00000282516\t846\t1.4415e+03\t5.8688e-01\t7.3808e-05\t18540\t158\t5.4310e+02\t2.9092e-01\t7135\t496\t4.9501e+02\t1.0020e+00\t2.4942e-05\t5211\t1\t9.4214e-06\t1781\t1.5032e+02\t1.0000e+00\t2.9773e-59\t3.5724e-24\t6.6527e-03\t9.3000e-01\t1.0790e+00\t5.5400e-01\t6.2100e-01\t1.0000e-03\t3.2000e-02\t\t-3.5119e-02\t5.5737e+00\t1.1286e+01\t1\t0\t0\t2\t1.1943e-05\t7.9636e-06\t125693\t3\t0\t125696\t1.1934e-05\t1.7901e-05\t0.0000e+00\t0.0000e+00\t9.9246e-05\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t6.5338e-05\t0.0000e+00\t0.0000e+00\t9.9231e-05\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t6.5327e-05\tprotein_coding\tENSG00000164190\t2\t8412\t46\tprotein_coding\t189655\t1.0000e+00\t1\t1.1057e+02\t9.0443e-03\tNA\t5\t36876861\t37066515");
            writer.WriteLine("RPS17\tENST00000330339\t0\t0.0000e+00\tNaN\t0.0000e+00\t879\t0\t3.4388e-02\t0.0000e+00\t75\t0\t0.0000e+00\tNaN\t0.0000e+00\t254\t0\t1.7630e-08\t85\t4.1103e-01\t3.2566e-01\t1.7556e-01\t4.9878e-01\t0.0000e+00\t0.0000e+00\t1.9000e+00\t0.0000e+00\t1.9000e+00\t0.0000e+00\t1.8490e+00\tno_exp_mis|no_exp_syn|no_variants\tNaN\tNaN\t5.9410e-01\t17933\t9\t5\t0\t0.0000e+00\t0.0000e+00\t0\t0\t0\t0\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\t0.0000e+00\tprotein_coding\tENSG00000184779\t2\t405\t5\tprotein_coding\t3815\tNA\tNA\tNA\tNA\tNA\t15\t82821158\t82824972");
            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetItems()
        {
            var geneIdToSymbols = new Dictionary<string, string>
            {
                {"ENSG00000108510", "MED13"},
                {"ENSG00000164190", "NIPBL"},
                {"tENSG00000184779", "RPS17" }
            };
            using (var reader = new GnomadGeneParser(new StreamReader(GetStream()), geneIdToSymbols))
            {
                var items = reader.GetItems().ToList();

                Assert.Equal(3, items.Count);
                Assert.Equal("{\"pLi\":1.00e0,\"pRec\":1.84e-16,\"pNull\":8.94e-40,\"synZ\":-1.38e0,\"misZ\":2.62e0,\"loeuf\":3.00e-2}", items[0].Value[0].GetJsonString());
                Assert.Equal("{\"pLi\":3.26e-1,\"pRec\":4.99e-1,\"pNull\":1.76e-1,\"loeuf\":1.85e0}", items[2].Value[0].GetJsonString());
            }
        }

        private static Stream GetStream_with_duplicate_gene_entries()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("gene\ttranscript\tobs_mis\texp_mis\toe_mis\tmu_mis\tpossible_mis\tobs_mis_pphen\texp_mis_pphen\toe_mis_pphen\tpossible_mis_pphen\tobs_syn\texp_syn\toe_syn\tmu_syn\tpossible_syn\tobs_lof\tmu_lof\tpossible_lof\texp_lof\tpLI\tpNull\tpRec\toe_lof\toe_syn_lower\toe_syn_upper\toe_mis_lower\toe_mis_upper\toe_lof_lower\toe_lof_upper\tconstraint_flag\tsyn_z\tmis_z\tlof_z\toe_lof_upper_rank\toe_lof_upper_bin\toe_lof_upper_bin_6\tn_sites\tclassic_caf\tmax_af\tno_lofs\tobs_het_lof\tobs_hom_lof\tdefined\tp\texp_hom_lof\tclassic_caf_afr\tclassic_caf_amr\tclassic_caf_asj\tclassic_caf_eas\tclassic_caf_fin\tclassic_caf_nfe\tclassic_caf_oth\tclassic_caf_sas\tp_afr\tp_amr\tp_asj\tp_eas\tp_fin\tp_nfe\tp_oth\tp_sas\ttranscript_type\tgene_id\ttranscript_level\tcds_length\tnum_coding_exons\tgene_type\tgene_length\texac_pLI\texac_obs_lof\texac_exp_lof\texac_oe_lof\tbrain_expression\tchromosome\tstart_position\tend_position");
            writer.WriteLine("MDGA2\tENST00000426342\t306\t4.0043e+02\t7.6419e-01\t2.1096e-05\t4724\t78\t1.6525e+02\t4.7202e-01\t1923\t125\t1.3737e+02\t9.0993e-01\t7.1973e-06\t1413\t4\t2.0926e-06\t453\t3.8316e+01\t9.9922e-01\t8.6490e-12\t7.8128e-04\t1.0440e-01\t7.8600e-01\t1.0560e+00\t6.9500e-01\t8.4000e-01\t5.0000e-02\t2.3900e-01\t\t8.2988e-01\t1.6769e+00\t5.1372e+00\t1529\t0\t0\t7\t2.8103e-05\t4.0317e-06\t124784\t7\t0\t124791\t2.8047e-05\t9.8167e-05\t0.0000e+00\t2.8962e-05\t0.0000e+00\t0.0000e+00\t0.0000e+00\t3.5391e-05\t1.6672e-04\t3.2680e-05\t0.0000e+00\t2.8962e-05\t0.0000e+00\t0.0000e+00\t0.0000e+00\t3.5308e-05\t1.6492e-04\t3.2678e-05\tprotein_coding\tENSG00000139915\t2\t2181\t13\tprotein_coding\t835332\t9.9322e-01\t3\t2.7833e+01\t1.0779e-01\tNA\t14\t47308826\t48144157");
            writer.WriteLine("MDGA2\tENST00000439988\t438\t5.5311e+02\t7.9189e-01\t2.9490e-05\t6608\t105\t2.0496e+02\t5.1228e-01\t2386\t180\t1.9491e+02\t9.2351e-01\t9.8371e-06\t2048\t11\t2.8074e-06\t627\t5.1882e+01\t6.6457e-01\t5.5841e-10\t3.3543e-01\t2.1202e-01\t8.1700e-01\t1.0450e+00\t7.3100e-01\t8.5700e-01\t1.3200e-01\t3.5100e-01\t\t8.3940e-01\t1.7393e+00\t5.2595e+00\t2989\t1\t0\t9\t3.6173e-05\t4.0463e-06\t124782\t9\t0\t124791\t3.6061e-05\t1.6228e-04\t6.4986e-05\t2.8962e-05\t0.0000e+00\t0.0000e+00\t0.0000e+00\t4.4275e-05\t1.6672e-04\t3.2680e-05\t6.4577e-05\t2.8962e-05\t0.0000e+00\t0.0000e+00\t0.0000e+00\t4.4135e-05\t1.6492e-04\t3.2678e-05\tprotein_coding\tENSG00000272781\t3\t3075\t17\tprotein_coding\t832866\tNA\tNA\tNA\tNA\tNA\t14\t47311134\t48143999");
            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetNonDuplicateItems()
        {
            var geneIdToSymbols = new Dictionary<string, string>
            {
                {"ENST00000426342", "MDGA2"},
                {"ENST00000439988", "MDGA2"}
            };
            using (var reader = new GnomadGeneParser(new StreamReader(GetStream_with_duplicate_gene_entries()), geneIdToSymbols))
            {
                var items = reader.GetItems().ToList();

                Assert.Single(items);
                Assert.Equal("{\"pLi\":9.99e-1,\"pRec\":7.81e-4,\"pNull\":8.65e-12,\"synZ\":8.30e-1,\"misZ\":1.68e0,\"loeuf\":2.39e-1}", items[0].Value[0].GetJsonString());
            }
        }
        private static Stream GetStream_resolve_without_loeuf()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("gene\ttranscript\tobs_mis\texp_mis\toe_mis\tmu_mis\tpossible_mis\tobs_mis_pphen\texp_mis_pphen\toe_mis_pphen\tpossible_mis_pphen\tobs_syn\texp_syn\toe_syn\tmu_syn\tpossible_syn\tobs_lof\tmu_lof\tpossible_lof\texp_lof\tpLI\tpNull\tpRec\toe_lof\toe_syn_lower\toe_syn_upper\toe_mis_lower\toe_mis_upper\toe_lof_lower\toe_lof_upper\tconstraint_flag\tsyn_z\tmis_z\tlof_z\toe_lof_upper_rank\toe_lof_upper_bin\toe_lof_upper_bin_6\tn_sites\tclassic_caf\tmax_af\tno_lofs\tobs_het_lof\tobs_hom_lof\tdefined\tp\texp_hom_lof\tclassic_caf_afr\tclassic_caf_amr\tclassic_caf_asj\tclassic_caf_eas\tclassic_caf_fin\tclassic_caf_nfe\tclassic_caf_oth\tclassic_caf_sas\tp_afr\tp_amr\tp_asj\tp_eas\tp_fin\tp_nfe\tp_oth\tp_sas\ttranscript_type\tgene_id\ttranscript_level\tcds_length\tnum_coding_exons\tgene_type\tgene_length\texac_pLI\texac_obs_lof\texac_exp_lof\texac_oe_lof\tbrain_expression\tchromosome\tstart_position\tend_position");
            writer.WriteLine("NBPF8\tENST00000369365\t75\t3.0353e+01\t2.4709e+00\t1.4000e-06\t24299\t60\t2.5873e+01\t2.3190e+00\t14469\t22\t1.3347e+01\t1.6483e+00\t5.9757e-07\t6838\t8\t1.5346e-07\t3145\t3.7051e+00\t1.4181e-07\t9.6601e-01\t3.3989e-02\t2.1592e+00\t1.1460e+00\t1.9490e+00\t1.7400e+00\t1.9940e+00\t1.0280e+00\t1.9670e+00\tmis_too_many\t-1.8618e+00\t-2.8797e+00\t-2.0676e+00\t19130\t9\t5\t11\t8.5551e-05\t2.3640e-05\t120533\t19\t0\t120552\t7.8807e-05\t7.4870e-04\t2.1519e-04\t3.0820e-05\t0.0000e+00\t1.6820e-04\t4.8035e-05\t1.1691e-04\t0.0000e+00\t0.0000e+00\t2.1446e-04\t2.9290e-05\t0.0000e+00\t1.6411e-04\t4.7779e-05\t1.0166e-04\t0.0000e+00\t0.0000e+00\tprotein_coding\tENSG00000162825\t2\t11420\t91\tprotein_coding\t77674\tNA\tNA\tNA\tNA\tNA\t1\t144146808\t144224481");
            writer.WriteLine("NBPF20\tENST00000369202\t450\t1.6927e+02\t2.6584e+00\t8.8521e-06\t29674\t110\t2.3246e+01\t4.7320e+00\t17017\t169\t6.7362e+01\t2.5089e+00\t3.9243e-06\t8313\t26\t6.7292e-07\t3810\t1.3886e+01\t1.9221e-22\t9.9999e-01\t7.9649e-06\t1.8724e+00\t1.8630e+00\t1.9970e+00\t1.9490e+00\t1.9990e+00\t1.2990e+00\t1.9710e+00\tmis_too_many|syn_outlier\t-9.7346e+00\t-7.6675e+00\t-3.0124e+00\t19150\t9\t5\t22\t3.0519e-04\t1.0143e-04\t125629\t75\t0\t125704\t2.9836e-04\t1.1190e-02\t1.5454e-04\t2.3190e-04\t1.0022e-04\t5.6199e-05\t7.5773e-04\t3.3476e-04\t3.2954e-04\t2.6141e-04\t1.2342e-04\t2.3129e-04\t9.9211e-05\t5.4367e-05\t7.3937e-04\t3.2537e-04\t3.2600e-04\t2.6134e-04\tprotein_coding\tENSG00000203832\t2\t13863\t110\tprotein_coding\t97258\t4.4592e-04\t7\t1.1898e+01\t5.8834e-01\tNA\t1\t148250249\t148347506");
            writer.WriteLine("FAM231B\tENST00000601199\t100\t6.6880e+01\t1.4952e+00\t3.5319e-06\t1044\t29\t1.9078e+01\t1.5201e+00\t297\t40\t2.6953e+01\t1.4840e+00\t1.4161e-06\t357\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\t1.1490e+00\t1.8690e+00\t1.2710e+00\t1.7630e+00\tNA\tNA\tno_exp_lof\t-1.9754e+00\t-1.4391e+00\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tprotein_coding\tENSG00000268674\t3\t507\t1\tprotein_coding\t510\tNA\tNA\tNA\tNA\tNA\t1\t16865561\t16866070");
            writer.WriteLine("FAM231D\tENST00000369173\t67\t7.4600e+01\t8.9812e-01\t3.7640e-06\t963\t18\t1.9611e+01\t9.1786e-01\t240\t20\t2.7120e+01\t7.3747e-01\t1.1812e-06\t318\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\t5.1800e-01\t1.0720e+00\t7.3700e-01\t1.1010e+00\tNA\tNA\tno_exp_lof\t1.0747e+00\t3.1268e-01\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tprotein_coding\tENSG00000203815\t3\t507\t1\tprotein_coding\t632\tNA\tNA\tNA\tNA\tNA\t1\t149675978\t149676609");
            writer.Flush();

            stream.Position = 0;
            return stream;
        }
        [Fact]
        public void ResolveDuplicateWithout_loeuf()
        {
            var geneIdToSymbols = new Dictionary<string, string>
            {
                {"ENSG00000162825", "NBPF20"},
                {"ENSG00000203832", "NBPF20"},
                {"ENSG00000268674", "FAM231D"},
                {"ENSG00000203815", "FAM231D"},
            };
            using (var reader = new GnomadGeneParser(new StreamReader(GetStream_resolve_without_loeuf()), geneIdToSymbols))
            {
                var items = reader.GetItems().ToList();

                Assert.Equal(2, items.Count);
                Assert.Equal("{\"pLi\":1.42e-7,\"pRec\":3.40e-2,\"pNull\":9.66e-1,\"synZ\":-1.86e0,\"misZ\":-2.88e0,\"loeuf\":1.97e0}", items[0].Value[0].GetJsonString());
                Assert.Equal("{\"synZ\":-1.98e0,\"misZ\":-1.44e0}", items[1].Value[0].GetJsonString());
            }
        }
    }
}