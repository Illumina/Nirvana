namespace VariantAnnotation.GeneFusions.SA
{
    public static class GeneFusionSourceUtilities
    {
        public static string Convert(GeneFusionSource source)
        {
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return source switch
            {
                GeneFusionSource.Alaei_Mahabadi_18_Cancers   => "Alaei-Mahabadi 18 cancers",
                GeneFusionSource.Babiceanu_NonCancerTissues  => "Babiceanu non-cancer tissues",
                GeneFusionSource.Bailey_pancreatic_cancers   => "Bailey pancreatic cancers",
                GeneFusionSource.Bao_gliomas                 => "Bao gliomas",
                GeneFusionSource.CACG                        => "CACG",
                GeneFusionSource.Cancer_Genome_Project       => "Cancer Genome Project",
                GeneFusionSource.CCLE                        => "DepMap CCLE",
                GeneFusionSource.CCLE_Klign                  => "CCLE Klijn",
                GeneFusionSource.CCLE_Vellichirammal         => "CCLE Vellichirammal",
                GeneFusionSource.ChimerKB_4                  => "ChimerKB 4.0",
                GeneFusionSource.ChimerPub_4                 => "ChimerPub 4.0",
                GeneFusionSource.ChimerSeq_4                 => "ChimerSeq 4.0",
                GeneFusionSource.ConjoinG                    => "ConjoinG",
                GeneFusionSource.COSMIC                      => "COSMIC",
                GeneFusionSource.Duplicated_Genes_Database   => "Duplicated Genes Database",
                GeneFusionSource.GTEx_healthy_tissues        => "GTEx healthy tissues",
                GeneFusionSource.Healthy                     => "Healthy",
                GeneFusionSource.Healthy_prefrontal_cortex   => "Healthy prefrontal cortex",
                GeneFusionSource.Healthy_strong_support      => "Healthy (strong support)",
                GeneFusionSource.Human_Protein_Atlas         => "Human Protein Atlas",
                GeneFusionSource.Illumina_BodyMap2           => "Illumina Body Map 2.0",
                GeneFusionSource.Known                       => "Known",
                GeneFusionSource.Mitelman_DB                 => "Mitelman DB",
                GeneFusionSource.NonTumorCellLines           => "non-tumor cell lines",
                GeneFusionSource.OncoKB                      => "OncoKB",
                GeneFusionSource.OneK_Genomes_Project        => "1000 Genomes Project",
                GeneFusionSource.PCAWG                       => "PCAWG",
                GeneFusionSource.Robinson_prostate_cancers   => "Robinson prostate cancers",
                GeneFusionSource.TCGA                        => "TCGA",
                GeneFusionSource.TCGA_Gao                    => "TCGA Gao",
                GeneFusionSource.TCGA_oesophageal_carcinomas => "TCGA oesophageal carcinomas",
                GeneFusionSource.TCGA_Tumor                  => "TCGA tumor",
                GeneFusionSource.TCGA_Vellichirammal         => "TCGA Vellichirammal",
                GeneFusionSource.TICdb                       => "TICdb",
                GeneFusionSource.TumorFusions_tumor          => "TumorFusions tumor",
                GeneFusionSource.TumorFusions_normal         => "TumorFusions normal",
                _                                            => null
            };
        }
    }
}