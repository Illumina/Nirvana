using System;
using System.Collections.Generic;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using CacheUtils.Helpers;
using Compression.Utilities;

namespace SAUtils.CreateOmimTsv
{
    public static class CreateOmimTsvMain
    {
        private static string _inputGeneMap2Path;
        private static string _inputReferencePath;
        private static string _outputTsvDirectory;
        private static string _universalGeneArchivePath;
        private static string _mim2GenePath;

        private static ExitCodes ProgramExecution()
        {
            var (ensemblGeneIdToSymbol, entrezGeneIdToSymbol) = ParseUniversalGeneArchive();
            var geneSymbolUpdater = new GeneSymbolUpdater(ensemblGeneIdToSymbol, entrezGeneIdToSymbol);
            var omimTsvCreator    = new OmimTsvCreator(_inputGeneMap2Path, _mim2GenePath, geneSymbolUpdater, _outputTsvDirectory);

            return omimTsvCreator.Create();
        }

        private static (Dictionary<string, string> EntrezGeneIdToSymbol, Dictionary<string, string>
            EnsemblIdToSymbol) ParseUniversalGeneArchive()
        {
            var (_, refNameToChromosome, _) = SequenceHelper.GetDictionaries(_inputReferencePath);

            UgaGene[] genes;

            using (var reader = new UgaGeneReader(GZipUtilities.GetAppropriateReadStream(_universalGeneArchivePath),
                refNameToChromosome))
            {
                genes = reader.GetGenes();
            }

            var entrezGeneIdToSymbol = genes.GetGeneIdToSymbol(x => x.EntrezGeneId);
            var ensemblIdToSymbol    = genes.GetGeneIdToSymbol(x => x.EnsemblId);
            return (entrezGeneIdToSymbol, ensemblIdToSymbol);
        }

        private static Dictionary<string, string> GetGeneIdToSymbol(this UgaGene[] genes,
            Func<UgaGene, string> geneIdFunc)
        {
            var dict = new Dictionary<string, string>();
            foreach (var gene in genes)
            {
                var key = geneIdFunc(gene);
                var symbol = gene.Symbol;
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(symbol)) continue;
                dict[key] = symbol;
            }
            return dict;
        }

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input genemap2 {path}",
                    v => _inputGeneMap2Path = v
                },
                {
                    "mim|m=",
                    "mim2gene {path}",
                    v => _mim2GenePath = v
                },
                {
                    "out|o=",
                    "output TSV {directory}",
                    v => _outputTsvDirectory = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => _inputReferencePath = v
                },
                {
                    "uga|u=",
                    "universal gene archive {path}",
                    v => _universalGeneArchivePath = v
                },
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_inputGeneMap2Path, "genemap2", "--in")
                .CheckInputFilenameExists(_mim2GenePath, "mim2gene", "--mim")
                .CheckInputFilenameExists(_universalGeneArchivePath, "universal gene archive", "--uga")
                .CheckInputFilenameExists(_inputReferencePath, "compressed reference", "--ref")
                .CheckDirectoryExists(_outputTsvDirectory, "output TSV", "--out")
                .SkipBanner()
                .ShowHelpMenu("Reads provided OMIM data files and populates tsv file", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }
    }
}
