using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.Genes.DataStructures;
using Genome;
using IO;
using OptimizedCore;

namespace CacheUtils.Genes.IO
{
    public sealed class HgncReader : IDisposable
    {
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        private readonly StreamReader _reader;

        private const int HgncIdIndex    = 0;
        private const int SymbolIndex    = 1;
        private const int LocationIndex  = 6;
        private const int EntrezIdIndex  = 18;
        private const int EnsemblIdIndex = 19;

        public HgncReader(Stream stream, IDictionary<string, IChromosome> refNameToChromosome)
        {
            _refNameToChromosome = refNameToChromosome;
            _reader = FileUtilities.GetStreamReader(stream);
            _reader.ReadLine();
        }

        /// <summary>
        /// retrieves the next gene. Returns false if there are no more genes available
        /// </summary>
        private HgncGene Next()
        {
            string line = _reader.ReadLine();
            if (line == null) return null;

            var cols = line.OptimizedSplit('\t');
            if (cols.Length != 49) throw new InvalidDataException($"Expected 48 columns but found {cols.Length} when parsing the gene entry:[{line}]");

            try
            {
                int hgncId             = int.Parse(cols[HgncIdIndex].Substring(5));
                string symbol          = cols[SymbolIndex];
                IChromosome chromosome = GetChromosome(cols[LocationIndex]);
                string entrezGeneId    = GetId(cols[EntrezIdIndex]);
                string ensemblId       = GetId(cols[EnsemblIdIndex]);

                return new HgncGene(chromosome, -1, -1, symbol, entrezGeneId, ensemblId, hgncId);
            }
            catch (Exception)
            {
                Console.WriteLine("Offending line: {0}", line);
                for (var i = 0; i < cols.Length; i++) Console.WriteLine("- col {0}: [{1}]", i, cols[i]);
                throw;
            }
        }

        public HgncGene[] GetGenes()
        {
            var list = new List<HgncGene>();

            while (true)
            {
                var gene = Next();
                if (gene == null) break;
                list.Add(gene);
            }

            return list.ToArray();
        }

        private IChromosome GetChromosome(string cytogeneticBand)
        {
            int armPos = GetArmPos(cytogeneticBand);
            if (armPos == -1) return new EmptyChromosome(cytogeneticBand);

            string chrName = cytogeneticBand.Substring(0, armPos);
            return ReferenceNameUtilities.GetChromosome(_refNameToChromosome, chrName);
        }

        private static int GetArmPos(string cytogeneticBand)
        {
            int pos = cytogeneticBand.IndexOf('p');
            if (pos != -1) return pos;

            pos = cytogeneticBand.IndexOf('q');
            return pos;
        }

        private static string GetId(string s) => string.IsNullOrEmpty(s) ? null : s;

        public void Dispose() => _reader.Dispose();
    }
}
