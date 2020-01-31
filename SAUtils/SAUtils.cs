using System.Collections.Generic;
using CommandLine.Builders;
using SAUtils.ClinGen;
using SAUtils.DbSnpRemapper;
using SAUtils.dbVar;
using SAUtils.ExtractCosmicSvs;
using SAUtils.ExtractMiniSa;
using SAUtils.ExtractMiniXml;
using SAUtils.GnomadGeneScores;
using SAUtils.MitoHeteroplasmy;
using SAUtils.MitoMap;
using SAUtils.NsaIndexUpdater;
using SAUtils.PrimateAi;
using SAUtils.ProcessSpliceNetTsv;
using SAUtils.SpliceAi;
using VariantAnnotation.Interface;

namespace SAUtils
{
    public static class SaUtils
    {
        public static int Main(string[] args)
        {
            var ops                    = new Dictionary<string, TopLevelOption>
            {
                ["clinvar"]               = new TopLevelOption("create ClinVar database", CreateClinvarDb.Main.Run),
                ["cosmic"]                = new TopLevelOption("create COSMIC database", CreateCosmicDb.Main.Run),
                ["ClinGen"]               = new TopLevelOption("create ClinGen database", MakeClinGenDb.Main.Run),
                ["CustomVar"]             = new TopLevelOption("create custom variant annotation database", Custom.VariantMain.Run),
                ["CustomGene"]            = new TopLevelOption("create custom gene annotation database", Custom.GeneMain.Run),
                ["OneKGenSvVcfToBed"]     = new TopLevelOption("convert 1000 Genomes structural variants VCF file into a BED-like file", OneKGenSvDb.VcfToBed.Run),
                ["OneKGenSv"]             = new TopLevelOption("create 1000 Genomes structural variants database", OneKGenSvDb.Create.Run),
                ["OneKGen"]               = new TopLevelOption("create 1000 Genome small variants database", CreateOneKgDb.Main.Run),
                ["RefMinor"]              = new TopLevelOption("create Reference Minor database from 1000 Genome ", RefMinorDb.Main.Run),
                ["ancestralAllele"]       = new TopLevelOption("create Ancestral allele database from 1000Genomes data", MakeAaDb.Main.Run),
                ["Dbsnp"]                 = new TopLevelOption("create dbSNP database", CreateDbsnpDb.Main.Run),
                ["globalMinor"]           = new TopLevelOption("create global minor allele database", CreateGlobalAllelesDb.Main.Run),  
                ["Dgv"]                   = new TopLevelOption("create DGV database", makeDgvDb.Main.Run),
                ["Omim"]                  = new TopLevelOption("create OMIM database", Omim.Main.Run),
                ["downloadOmim"]          = new TopLevelOption("download OMIM database", Omim.Downloader.Run),
                ["ExacScores"]            = new TopLevelOption("create ExAC gene scores database", ExacScores.Main.Run),
                ["extractMiniSA"]         = new TopLevelOption("extracts mini SA", ExtractMiniSaMain.Run),
                ["extractMiniXml"]        = new TopLevelOption("extracts mini XML (ClinVar)", ExtractMiniXmlMain.Run),
                ["Gnomad"]                = new TopLevelOption("create gnomAD database", CreateGnomadDb.GnomadMain.Run),
                ["GnomadGeneScores"]      = new TopLevelOption("create gnomAD gene scores database", GnomadGenesMain.Run),
                ["TopMed"]                = new TopLevelOption("create TOPMed database", CreateTopMedDb.Main.Run),
                ["PhyloP"]                = new TopLevelOption("create PhyloP database", PhyloP.Main.Run),
                ["CosmicSv"]              = new TopLevelOption("create COSMIC SV tsv files", ExtractCosmicSvsMain.Run),
                ["remapWithDbsnp"]        = new TopLevelOption("remap a VCF file given source and destination rsID mappings", DbSnpRemapperMain.Run),
                ["filterSpliceNetTsv"]    = new TopLevelOption("filter SpliceNet predictions", SpliceNetPredictionFilterMain.Run),
                ["mitomapVarDb"]          = new TopLevelOption("create MITOMAP small variants database", SmallVarDb.Run),
                ["mitomapSvDb"]           = new TopLevelOption("create MITOMAP structural variants database", StructVarDb.Run),
                ["spliceAi"]              = new TopLevelOption("create SpliceAI database", SpliceAiDb.Run),
                ["primateAi"]             = new TopLevelOption("create PrimateAI database", PrimateAiDb.Run),
                ["dosageSensitivity"]     = new TopLevelOption("create dosage sensitivity database", DosageSensitivity.Run),
                ["diseaseValidity"]       = new TopLevelOption("create disease validity database", GeneDiseaseValidity.Run),
                ["MitoHet"]               = new TopLevelOption("create mitochondrial Heteroplasmy database", MitoHeteroplasmyDb.Run),
                ["Revel"]                 = new TopLevelOption("create REVEL database", Revel.Create.Run),
                ["index"]                 = new TopLevelOption("edit an index file", UpdateIndex.Run),
                ["concat"]                = new TopLevelOption("merge multiple NSA files for the same data source having non-overlapping regions", NsaConcatenator.NsaConcatenator.Run)
            };

            var exitCode = new TopLevelAppBuilder(args, ops)
                .Parse()
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Utilities focused on supplementary annotation")
                .ShowErrors()
                .Execute();

            return (int)exitCode;
        }
    }
}