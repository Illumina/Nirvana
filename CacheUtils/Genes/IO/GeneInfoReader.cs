using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.Genes.DataStructures;
using OptimizedCore;

namespace CacheUtils.Genes.IO
{
    public sealed class GeneInfoReader : IDisposable
    {
        private readonly StreamReader _reader;

        private int _entrezGeneIndex = -1;
        private int _symbolIndex     = -1;
        private int _dbXrefsIndex    = -1;

        public GeneInfoReader(StreamReader reader)
        {
            _reader = reader;
            string headerLine = _reader.ReadLine();
            SetColumnIndices(headerLine);
        }

        private void SetColumnIndices(string line)
        {
            if (line.StartsWith("#Format: "))  line = line.Substring(9);
            if (line.OptimizedStartsWith('#')) line = line.Substring(1);

            var cols = line.OptimizedSplit('\t');
            if (cols.Length == 1) cols = line.OptimizedSplit(' ');

            for (var index = 0; index < cols.Length; index++)
            {
                string header = cols[index];

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (header)
                {
                    case "dbXrefs":
                        _dbXrefsIndex = index;
                        break;
                    case "GeneID":
                        _entrezGeneIndex = index;
                        break;
                    case "Symbol":
                        _symbolIndex = index;
                        break;
                }
            }

            // ReSharper disable once InvertIf
            if (_entrezGeneIndex == -1 || _symbolIndex == -1) {
                Console.WriteLine("_dbXrefsIndex:    {0}", _dbXrefsIndex);
                Console.WriteLine("_entrezGeneIndex: {0}", _entrezGeneIndex);
                Console.WriteLine("_symbolIndex:     {0}", _symbolIndex);

                throw new InvalidDataException("Not all of the indices were set.");
            }
        }

        /// <summary>
        /// retrieves the next gene. Returns false if there are no more genes available
        /// </summary>
        private GeneInfo Next()
        {
            string line = _reader.ReadLine();
            if (line == null) return null;

            if (!line.StartsWith("9606")) return null;

            var cols = line.OptimizedSplit('\t');
            if (cols.Length != 16) throw new InvalidDataException($"Expected 16 columns but found {cols.Length} when parsing the gene entry:\n[{line}]");

            try
            {
                string entrezGeneId = cols[_entrezGeneIndex];
                string symbol       = cols[_symbolIndex];

                return new GeneInfo(symbol, entrezGeneId);
            }
            catch (Exception)
            {
                Console.WriteLine("Offending line: {0}", line);
                for (var i = 0; i < cols.Length; i++) Console.WriteLine("- col {0}: [{1}]", i, cols[i]);
                throw;
            }
        }

        public GeneInfo[] GetGenes()
        {
            var list = new List<GeneInfo>();

            while (true)
            {
                var gene = Next();
                if (gene == null) break;
                list.Add(gene);
            }

            return list.ToArray();
        }

        public void Dispose() => _reader.Dispose();
    }
}
