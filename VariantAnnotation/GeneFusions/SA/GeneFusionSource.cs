// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace VariantAnnotation.GeneFusions.SA
{
    public enum GeneFusionSource : byte
    {
        None = 0,
        Alaei_Mahabadi_18_Cancers, // 18cancer
        Babiceanu_NonCancerTissues,
        Bailey_pancreatic_cancers,
        Bao_gliomas,
        CACG,
        Cancer_Genome_Project,
        CCLE,
        CCLE_Vellichirammal, // ccle3
        ConjoinG,
        COSMIC,
        Duplicated_Genes_Database,
        GTEx_healthy_tissues,
        Healthy,
        Healthy_prefrontal_cortex,
        Healthy_strong_support, // banned
        Human_Protein_Atlas,
        Illumina_BodyMap2,
        NonTumorCellLines,
        OneK_Genomes_Project,
        Paralog,
        Pseudogene,
        Readthrough,
        Robinson_prostate_cancers,
        TumorFusions_normal,
        TCGA_oesophageal_carcinomas,
        TCGA_Tumor,

        // additional data sources (2021-05-25)
        CCLE_Klign, // ccle2.txt
        ChimerKB_4,
        ChimerPub_4,
        ChimerSeq_4,
        Known,
        Mitelman_DB,
        OncoKB,
        PCAWG,
        TCGA,                // tcga.txt
        TumorFusions_tumor,  // tcga-cancer.txt
        TCGA_Gao,            // tcga2.txt
        TCGA_Vellichirammal, // tcga3.txt
        TICdb
    }
}