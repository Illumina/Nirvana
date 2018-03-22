using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAUtils.CreateOmimTsv
{
    public sealed class GeneSymbolUpdater
    {
        private readonly Dictionary<string, string> _entrezGeneIdToSymbol;
        private readonly Dictionary<string, string> _ensemblGeneIdToSymbol;
        private readonly HashSet<string> _geneSymbols;

        private readonly Dictionary<string, string> _updatedGeneSymbols;

        private int _numGenesWhereBothIdsAreNull;
        private int _numGeneSymbolsUpToDate;
        private int _numGeneSymbolsUpdated;
        private int _numGeneSymbolsNotInCache;
        private int _numResolvedGeneSymbolConflicts;

        public GeneSymbolUpdater(Dictionary<string, string> entrezGeneIdToSymbol,
            Dictionary<string, string> ensemblGeneIdToSymbol)
        {
            _entrezGeneIdToSymbol  = entrezGeneIdToSymbol;
            _ensemblGeneIdToSymbol = ensemblGeneIdToSymbol;
            _geneSymbols           = new HashSet<string>();
            _updatedGeneSymbols    = new Dictionary<string, string>();
        }

        public string UpdateGeneSymbol(string oldGeneSymbol, string ensemblGeneId, string entrezGeneId)
        {
            if (ensemblGeneId == null && entrezGeneId == null)
            {
                _numGenesWhereBothIdsAreNull++;
                return null;
            }

            var ensemblSymbol    = GetSymbol(ensemblGeneId, _ensemblGeneIdToSymbol);
            var entrezGeneSymbol = GetSymbol(entrezGeneId, _entrezGeneIdToSymbol);

            _geneSymbols.Clear();
            if (ensemblSymbol    != null) _geneSymbols.Add(ensemblSymbol);
            if (entrezGeneSymbol != null) _geneSymbols.Add(entrezGeneSymbol);

            if (_geneSymbols.Count == 0)
            {
                _numGeneSymbolsNotInCache++;
                return oldGeneSymbol;
            }

            var newGeneSymbol = _geneSymbols.First();

            if (_geneSymbols.Count > 1)
            {
                newGeneSymbol = ResolveGeneSymbolConflict(oldGeneSymbol, ensemblSymbol, entrezGeneSymbol);
                if (newGeneSymbol == null) throw new InvalidDataException($"Unable to resolve gene symbol conflict for {oldGeneSymbol}: Ensembl: [{ensemblGeneId}]: {ensemblSymbol}, Entrez Gene: [{entrezGeneId}]: {entrezGeneSymbol}");
                _numResolvedGeneSymbolConflicts++;
            }

            if (newGeneSymbol == oldGeneSymbol) _numGeneSymbolsUpToDate++;
            else
            {
                _updatedGeneSymbols[oldGeneSymbol] = newGeneSymbol;
                _numGeneSymbolsUpdated++;
            }

            return newGeneSymbol;
        }

        private static string ResolveGeneSymbolConflict(string oldGeneSymbol, string ensemblSymbol, string entrezGeneSymbol)
        {
            var symbolCounts = new Dictionary<string, int>();
            AddSymbol(symbolCounts, oldGeneSymbol);
            AddSymbol(symbolCounts, ensemblSymbol);
            AddSymbol(symbolCounts, entrezGeneSymbol);

            var mostFrequentSymbol = symbolCounts.OrderByDescending(x => x.Value).First();
            if (mostFrequentSymbol.Value == 1) throw new InvalidDataException("Found unique gene symbols when trying to resolve the gene symbol conflict.");

            return mostFrequentSymbol.Key;
        }

        private static void AddSymbol(IDictionary<string, int> symbolCounts, string geneSymbol)
        {
            if (symbolCounts.TryGetValue(geneSymbol, out int counts)) symbolCounts[geneSymbol] = counts + 1;
            else symbolCounts[geneSymbol] = 1;
        }

        private static string GetSymbol(string geneId, IReadOnlyDictionary<string, string> geneIdToSymbol)
        {
            if (geneId == null) return null;
            return geneIdToSymbol.TryGetValue(geneId, out var symbol) ? symbol : null;
        }

        public void DisplayStatistics()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Gene Symbol Update Statistics");
            Console.ResetColor();

            Console.WriteLine("============================================");
            Console.WriteLine($"# of gene symbols already up-to-date: {_numGeneSymbolsUpToDate:N0}");
            Console.WriteLine($"# of gene symbols updated:            {_numGeneSymbolsUpdated:N0}");
            Console.WriteLine($"# of genes where both IDs are null:   {_numGenesWhereBothIdsAreNull:N0}");
            Console.WriteLine($"# of gene symbols not in cache:       {_numGeneSymbolsNotInCache:N0}");
            Console.WriteLine($"# of resolved gene symbol conflicts:  {_numResolvedGeneSymbolConflicts:N0}");
        }

        public void WriteUpdatedGeneSymbols(StreamWriter writer)
        {
            writer.WriteLine("original\tupdated");
            foreach (var kvp in _updatedGeneSymbols) writer.WriteLine($"{kvp.Key}\t{kvp.Value}");
        }
    }
}

