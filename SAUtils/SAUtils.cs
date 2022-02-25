using System.Collections.Generic;
using CommandLine.Builders;
using ErrorHandling;
using SAUtils.ClinGen;
using SAUtils.CosmicGeneFusions;
using SAUtils.CreateClinvarDb;
using SAUtils.DbSnpRemapper;
using SAUtils.ExtractCosmicSvs;
using SAUtils.ExtractMiniSa;
using SAUtils.ExtractMiniXml;
using SAUtils.FusionCatcher;
using SAUtils.gnomAD;
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
            var ops = new Dictionary<string, TopLevelOption>
            {
                ["AaCon"]           = new("create AA conservation database", AAConservation.AaConservationMain.Run),
                ["ancestralAllele"] = new("create Ancestral allele database from 1000Genomes data", MakeAaDb.Main.Run),
                ["ClinGen"]         = new("create ClinGen database", MakeClinGenDb.Main.Run),
                ["clinvar"]         = new("create ClinVar database", ClinVarMain.Run),
                ["concat"] = new("merge multiple NSA files for the same data source having non-overlapping regions",
                    NsaConcatenator.NsaConcatenator.Run),
                ["Cosmic"]             = new("create COSMIC database", CreateCosmicDb.Main.Run),
                ["CosmicSv"]           = new("create COSMIC SV database", ExtractCosmicSvsMain.Run),
                ["CosmicFusion"]       = new("create COSMIC gene fusion database", CreateCosmicGeneFusions.Run),
                ["CustomGene"]         = new("create custom gene annotation database", Custom.GeneMain.Run),
                ["CustomVar"]          = new("create custom variant annotation database", Custom.VariantMain.Run),
                ["Dbsnp"]              = new("create dbSNP database", CreateDbsnpDb.Main.Run),
                ["Dgv"]                = new("create DGV database", makeDgvDb.Main.Run),
                ["DiseaseValidity"]    = new("create disease validity database", GeneDiseaseValidity.Run),
                ["DosageMapRegions"]   = new("create dosage map regions", DosageMapRegions.Run),
                ["DosageSensitivity"]  = new("create dosage sensitivity database", DosageSensitivity.Run),
                ["DownloadOmim"]       = new("download OMIM database", Omim.Downloader.Run),
                ["ExacScores"]         = new("create ExAC gene scores database", ExacScores.Main.Run),
                ["ExtractMiniSA"]      = new("extracts mini SA", ExtractMiniSaMain.Run),
                ["ExtractMiniXml"]     = new("extracts mini XML (ClinVar)", ExtractMiniXmlMain.Run),
                ["FilterSpliceNetTsv"] = new("filter SpliceNet predictions", SpliceNetPredictionFilterMain.Run),
                ["FusionCatcher"]      = new("create FusionCatcher database", CreateFusionCatcher.Run),
                ["GlobalMinor"]        = new("create global minor allele database", CreateGlobalAllelesDb.Main.Run),
                ["Gnomad"]             = new("create gnomAD database", GnomadSnvMain.Run),
                ["Gnomad-lcr"]         = new("create gnomAD low complexity region database", LcrRegionsMain.Run),
                ["GnomadGeneScores"]   = new("create gnomAD gene scores database", GnomadGenesMain.Run),
                ["Index"]              = new("edit an index file", UpdateIndex.Run),
                ["MitoHet"]            = new("create mitochondrial Heteroplasmy database", MitoHeteroplasmyDb.Run),
                ["MitomapSvDb"]        = new("create MITOMAP structural variants database", StructVarDb.Run),
                ["MitomapVarDb"]       = new("create MITOMAP small variants database", SmallVarDb.Run),
                ["Omim"]               = new("create OMIM database", Omim.Main.Run),
                ["OneKGen"]            = new("create 1000 Genome small variants database", CreateOneKgDb.Main.Run),
                ["OneKGenSv"]          = new("create 1000 Genomes structural variants database", OneKGenSvDb.Create.Run),
                ["OneKGenSvVcfToBed"] = new("convert 1000 Genomes structural variants VCF file into a BED-like file",
                    OneKGenSvDb.VcfToBed.Run),
                ["PhyloP"]         = new("create PhyloP database", PhyloP.Main.Run),
                ["PrimateAi"]      = new("create PrimateAI database", PrimateAiDb.Run),
                ["RefMinor"]       = new("create Reference Minor database from 1000 Genome ", RefMinorDb.Main.Run),
                ["RemapWithDbsnp"] = new("remap a VCF file given source and destination rsID mappings", DbSnpRemapperMain.Run),
                ["Revel"]          = new("create REVEL database", Revel.Create.Run),
                ["Dann"]           = new("create DANN database", Dann.Create.Run),
                ["SpliceAi"]       = new("create SpliceAI database", SpliceAiDb.Run),
                ["TopMed"]         = new("create TOPMed database", CreateTopMedDb.Main.Run)
            };

            ExitCodes exitCode = new TopLevelAppBuilder(args, ops)
                .Parse()
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Utilities focused on supplementary annotation")
                .ShowErrors()
                .Execute();

            return (int) exitCode;
        }
    }
}