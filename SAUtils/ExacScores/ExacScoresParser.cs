using System;
using System.Collections.Generic;
using System.IO;
using OptimizedCore;
using VariantAnnotation.Interface.SA;

namespace SAUtils.ExacScores
{
    public sealed class ExacScoresParser : IDisposable
    {
        private readonly StreamReader _reader;

        private const string GeneTag = "gene";
        private const string PliTag = "pLI";
        private const string PrecTag = "pRec";
        private const string PnullTag = "pNull";

        private int _geneIndex = -1;
        private int _pliIndex = -1;
        private int _precIndex = -1;
        private int _pnullIndex = -1;


        public ExacScoresParser(StreamReader reader)
        {
            _reader = reader;
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
                    geneAnnotations.Add(geneAnnotation.GeneSymbol, new List<ISuppGeneItem> {geneAnnotation});
                }

            }

            return geneAnnotations;

        }

        private ISuppGeneItem GetGeneAndScores(string line)
        {
            var cols  = line.OptimizedSplit('\t');
            var gene  = cols[_geneIndex];
            var pLi   = double.Parse(cols[_pliIndex]);
            var pRec  = double.Parse(cols[_precIndex]);
            var pNull = double.Parse(cols[_pnullIndex]);

            return new ExacScoreItem(gene, pLi, pRec, pNull);
        }

        private bool GetColumnIndices(string line)
        {
            var cols = line.OptimizedSplit('\t');

            _geneIndex = Array.IndexOf(cols, GeneTag);
            _pliIndex = Array.IndexOf(cols, PliTag);
            _pnullIndex = Array.IndexOf(cols, PnullTag);
            _precIndex = Array.IndexOf(cols, PrecTag);

            if (_geneIndex < 0)
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

            return true;
        }
    }
}