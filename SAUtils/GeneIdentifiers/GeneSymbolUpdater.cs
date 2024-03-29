﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using OptimizedCore;
using VariantAnnotation.IO;

namespace SAUtils.GeneIdentifiers
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
        private int _numUnresolvedGeneSymbolConflicts;

        public GeneSymbolUpdater(Dictionary<string, string> entrezGeneIdToSymbol, Dictionary<string, string> ensemblGeneIdToSymbol)
        {
            _entrezGeneIdToSymbol = entrezGeneIdToSymbol;
            _ensemblGeneIdToSymbol = ensemblGeneIdToSymbol;
            _geneSymbols = new HashSet<string>();
            _updatedGeneSymbols = new Dictionary<string, string>();
        }

        public string UpdateGeneSymbol(string oldGeneSymbol, string ensemblGeneId, string entrezGeneId)
        {
            if (ensemblGeneId == null && entrezGeneId == null)
            {
                _numGenesWhereBothIdsAreNull++;
                return null;
            }
            var ensemblSymbol = GetSymbol(ensemblGeneId, _ensemblGeneIdToSymbol);
            var entrezGeneSymbol = GetSymbol(entrezGeneId, _entrezGeneIdToSymbol);
            _geneSymbols.Clear();
            if (ensemblSymbol != null) _geneSymbols.Add(ensemblSymbol);
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
                if (newGeneSymbol == null)
                {
                    Console.WriteLine($"Unable to resolve gene symbol conflict for {oldGeneSymbol}: Ensembl: [{ensemblGeneId}]: {ensemblSymbol}, Entrez Gene: [{entrezGeneId}]: {entrezGeneSymbol}");
                    _numUnresolvedGeneSymbolConflicts++;
                    return null;
                }
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
            if (mostFrequentSymbol.Value == 1)
            {
                //Console.WriteLine($"Found unique gene symbols when trying to resolve the gene symbol conflict. Entrez Gene {entrezGeneSymbol}");
                return null;
            }

            return mostFrequentSymbol.Key;
        }

        private static void AddSymbol(Dictionary<string, int> symbolCounts, string geneSymbol)
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

            StringBuilder sb = StringBuilderPool.Get();
            var jo = new JsonObject(sb);
            sb.Append(JsonObject.OpenBrace);

            jo.AddIntValue("NumGeneSymbolsUpToDate",           _numGeneSymbolsUpToDate);
            jo.AddIntValue("NumGeneSymbolsUpdated",            _numGeneSymbolsUpdated);
            jo.AddIntValue("NumGenesWhereBothIdsAreNull",      _numGenesWhereBothIdsAreNull);
            jo.AddIntValue("NumGeneSymbolsNotInCache",         _numGeneSymbolsNotInCache);
            jo.AddIntValue("NumResolvedGeneSymbolConflicts",   _numResolvedGeneSymbolConflicts);
            jo.AddIntValue("NumUnresolvedGeneSymbolConflicts", _numUnresolvedGeneSymbolConflicts);

            sb.Append(JsonObject.CloseBrace);

            Console.WriteLine(JObject.Parse(sb.ToString())); //pretty printing json
        }
    }
}