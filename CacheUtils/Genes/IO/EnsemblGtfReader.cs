using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.Genes.DataStructures;
using ErrorHandling.Exceptions;
using Genome;
using OptimizedCore;

namespace CacheUtils.Genes.IO
{
    public sealed class EnsemblGtfReader : IDisposable
    {
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        private readonly StreamReader _reader;

        private const int ChromosomeIndex  = 0;
        private const int FeatureTypeIndex = 2;
        private const int StartIndex       = 3;
        private const int EndIndex         = 4;
        private const int InfoIndex        = 8;

        public EnsemblGtfReader(StreamReader reader, IDictionary<string, IChromosome> refNameToChromosome)
        {
            _refNameToChromosome = refNameToChromosome;
            _reader = reader;
            _reader.ReadLine();
        }

        public EnsemblGene[] GetGenes()
        {
            var genes = new List<EnsemblGene>();

            while (true)
            {
                string line = _reader.ReadLine();
                if (line == null) break;

                if (line.OptimizedStartsWith('#')) continue;

                var cols = line.OptimizedSplit('\t');
                if (cols.Length != 9) throw new InvalidDataException($"Expected 9 columns but found {cols.Length} when parsing the GFF entry.");

                string featureType = cols[FeatureTypeIndex];
                if (featureType != "gene") continue;

                AddGene(cols, genes);
            }

            return genes.ToArray();
        }

        private void AddGene(string[] cols, ICollection<EnsemblGene> genes)
        {
            var chromosome = RefSeqGffReader.GetChromosome(cols[ChromosomeIndex], _refNameToChromosome);
            if (chromosome == null) return;

            try
            {
                int start    = int.Parse(cols[StartIndex]);
                int end      = int.Parse(cols[EndIndex]);
                var infoCols = cols[InfoIndex].Split(';', StringSplitOptions.RemoveEmptyEntries);
                var info     = GetGffFields(infoCols);

                var gene = new EnsemblGene(chromosome, start, end, info.EnsemblGeneId, info.Name);
                genes.Add(gene);
            }
            catch (Exception)
            {
                Console.WriteLine();
                Console.WriteLine("Offending line: {0}", string.Join('\t', cols));
                for (var i = 0; i < cols.Length; i++) Console.WriteLine("- col {0}: [{1}]", i, cols[i]);
                throw;
            }
        }

        private static (string EnsemblGeneId, string Name) GetGffFields(string[] cols)
        {
            string ensemblId = null;
            string symbol    = null;

            foreach (string col in cols)
            {
                var kvp      = col.Trim().OptimizedSplit(' ');
                string key   = kvp[0];
                string value = kvp[1].Trim('\"');

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (key)
                {
                    case "gene_id":
                        ensemblId = value;
                        break;
                    case "gene_name":
                        symbol = value;
                        break;
                }
            }

            if (string.IsNullOrEmpty(ensemblId) || string.IsNullOrEmpty(symbol))
            {
                throw new UserErrorException(string.Join('\t', cols));
            }

            return (ensemblId, symbol);
        }

        public void Dispose() => _reader.Dispose();
    }
}
