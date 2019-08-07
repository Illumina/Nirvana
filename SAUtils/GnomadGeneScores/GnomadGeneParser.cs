using System;
using System.Collections.Generic;
using System.IO;
using OptimizedCore;
using VariantAnnotation.Interface.SA;

namespace SAUtils.GnomadGeneScores
{
    public sealed class GnomadGeneParser : IDisposable
    {
        private readonly StreamReader _reader;

        private const string GeneTag       = "gene";
        private const string GeneIdTag     = "gene_id";
        private const string PliTag        = "pLI";
        private const string PrecTag       = "pRec";
        private const string PnullTag      = "pNull";
        private const string SynZTag       = "syn_z";
        private const string MisZTag       = "mis_z";
        private const string LoeufTag      = "oe_lof_upper";

        private int _geneIndex   = -1;
        private int _geneIdIndex = -1;
        private int _pliIndex    = -1;
        private int _precIndex   = -1;
        private int _pnullIndex  = -1;
        private int _synZIndex   = -1;
        private int _misZIndex   = -1;
        private int _loeufIndex  = -1;

        private readonly Dictionary<string, string> _geneIdToSymbols;
        public GnomadGeneParser(StreamReader reader, Dictionary<string, string> geneIdToSymbols)
        {
            _reader = reader;
            _geneIdToSymbols = geneIdToSymbols;
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }

        public Dictionary<string, List<ISuppGeneItem>> GetItems()
        {
            bool isFirstLine = true;

            var geneAnnotations = new Dictionary<string, List<ISuppGeneItem>>();
            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                if (isFirstLine)
                {
                    if (!GetColumnIndices(line)) return null;
                    isFirstLine = false;
                }
                else
                {
                    var geneAnnotation = GetGeneAndScores(line);
                    if(geneAnnotation == null) continue;
                    if (geneAnnotations.TryAdd(geneAnnotation.GeneSymbol, new List<ISuppGeneItem> {geneAnnotation}))
                        continue;

                    var existingEntry = (GnomadGeneItem) geneAnnotations[geneAnnotation.GeneSymbol][0];
                    var newEntry = (GnomadGeneItem) geneAnnotation;
                    // in case of a conflict we keep the item with minimal loeuf
                    if (existingEntry.CompareTo(newEntry) > 0)
                        geneAnnotations[geneAnnotation.GeneSymbol][0] = geneAnnotation;
                    
                }

            }
            return geneAnnotations;

        }

        private ISuppGeneItem GetGeneAndScores(string line)
        {
            var cols = line.OptimizedSplit('\t');
            var geneId = cols[_geneIdIndex];
            if (!_geneIdToSymbols.TryGetValue(geneId, out var gene))
            {
                gene = cols[_geneIndex];
                Console.WriteLine($"GeneId to symbol not found in cache for: {geneId}, using provided name in file: {gene}");
            }

            var pLi   = GetScore(cols[_pliIndex]);
            var pRec  = GetScore(cols[_precIndex]);
            var pNull = GetScore(cols[_pnullIndex]);
            var synZ  = GetScore(cols[_synZIndex]);
            var misZ  = GetScore(cols[_misZIndex]);
            var loeuf = GetScore(cols[_loeufIndex]);

            return new GnomadGeneItem(gene, pLi, pRec, pNull, synZ, misZ, loeuf);
        }

        private double? GetScore(string score)
        {
            if (score == "NA" || score == "NaN") return null;
            return double.Parse(score);
        }
        private bool GetColumnIndices(string line)
        {
            var cols = line.OptimizedSplit('\t');

            _geneIndex   = Array.IndexOf(cols, GeneTag);
            _geneIdIndex = Array.IndexOf(cols, GeneIdTag);
            _pliIndex    = Array.IndexOf(cols, PliTag);
            _pnullIndex  = Array.IndexOf(cols, PnullTag);
            _precIndex   = Array.IndexOf(cols, PrecTag);
            _synZIndex   = Array.IndexOf(cols, SynZTag);
            _misZIndex   = Array.IndexOf(cols, MisZTag);
            _loeufIndex  = Array.IndexOf(cols, LoeufTag);

            if (_geneIdIndex < 0)
            {
                Console.WriteLine("gene column not found");
                return false;
            }
            if (_pliIndex < 0)
            {
                Console.WriteLine("pLI column not found");
                return false;
            }
            if (_precIndex < 0)
            {
                Console.WriteLine("pRec column not found");
                return false;
            }
            if (_pnullIndex < 0)
            {
                Console.WriteLine("pNull column not found");
                return false;
            }
            if (_synZIndex < 0)
            {
                Console.WriteLine("synZ column not found");
                return false;
            }
            if (_misZIndex < 0)
            {
                Console.WriteLine("misZ column not found");
                return false;
            }
            if (_loeufIndex < 0)
            {
                Console.WriteLine("loeuf column not found");
                return false;
            }

            return true;
        }
    }
}