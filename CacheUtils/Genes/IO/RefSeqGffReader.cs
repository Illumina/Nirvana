using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.Genes.DataStructures;
using Genome;
using OptimizedCore;

namespace CacheUtils.Genes.IO
{
    public sealed class RefSeqGffReader : IDisposable
    {
        private readonly IDictionary<string, IChromosome> _accessionIdToChromosome;
        private readonly StreamReader _reader;

        private const int AccessionIndex   = 0;
        private const int FeatureTypeIndex = 2;
        private const int StartIndex       = 3;
        private const int EndIndex         = 4;
        private const int StrandIndex      = 6;
        private const int InfoIndex        = 8;

        public RefSeqGffReader(StreamReader reader, IDictionary<string, IChromosome> accessionIdToChromosome)
        {
            _accessionIdToChromosome = accessionIdToChromosome;
            _reader = reader;
            _reader.ReadLine();
        }

        public void AddGenes(List<RefSeqGene> refSeqGenes)
        {
            while (true)
            {
                string line = _reader.ReadLine();
                if (line == null) break;

                if (line.OptimizedStartsWith('#')) continue;

                var cols = line.OptimizedSplit('\t');
                if (cols.Length != 9) throw new InvalidDataException($"Expected 9 columns but found {cols.Length} when parsing the GFF entry.");

                string featureType = cols[FeatureTypeIndex];
                if (featureType == "gene") AddGene(cols, refSeqGenes);
            }
        }

        private void AddGene(string[] cols, ICollection<RefSeqGene> refSeqGenes)
        {
            var chromosome = GetChromosome(cols[AccessionIndex], _accessionIdToChromosome);
            if (chromosome == null) return;

            try
            {
                int start            = int.Parse(cols[StartIndex]);
                int end              = int.Parse(cols[EndIndex]);
                bool onReverseStrand = cols[StrandIndex] == "-";
                var infoCols         = cols[InfoIndex].OptimizedSplit(';');
                var info             = GetGffFields(infoCols);

                var gene = new RefSeqGene(chromosome, start, end, onReverseStrand, info.EntrezGeneId, info.Name, info.HgncId);
                refSeqGenes.Add(gene);
            }
            catch (Exception)
            {
                Console.WriteLine();
                Console.WriteLine("Offending line: {0}", string.Join('\t', cols));
                for (var i = 0; i < cols.Length; i++) Console.WriteLine("- col {0}: [{1}]", i, cols[i]);
                throw;
            }
        }

        internal static IChromosome GetChromosome(string referenceName, IDictionary<string, IChromosome> refNameToChromosome)
        {
            refNameToChromosome.TryGetValue(referenceName, out var chromosome);
            return chromosome;
        }

        private static (string Name, string EntrezGeneId, int HgncId)
            GetGffFields(IEnumerable<string> cols)
        {
            string entrezGeneId = null;
            string name         = null;
            int hgncId          = -1;

            foreach (string col in cols)
            {
                (string key, string value) = col.OptimizedKeyValue();

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (key)
                {
                    case "Dbxref":
                        var ids = value.OptimizedSplit(',');
                        (entrezGeneId, hgncId) = GetIds(ids);
                        break;
                    case "Name":
                        name = value;
                        break;
                }
            }

            return (name, entrezGeneId, hgncId);
        }

        private static (string EntrezGeneId, int HgncId) GetIds(IEnumerable<string> ids)
        {
            string entrezGeneId = null;
            int hgncId          = -1;

            foreach (string idPair in ids)
            {
                var cols = idPair.OptimizedSplit(':');

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (cols[0])
                {
                    case "HGNC":
                        int lastIndex = cols.Length - 1;
                        if (cols[lastIndex] != "HGNC") hgncId = int.Parse(cols[lastIndex]);
                        break;
                    case "GeneID":
                        entrezGeneId = cols[1];
                        break;
                }
            }

            return (entrezGeneId, hgncId);
        }

        public void Dispose() => _reader.Dispose();
    }
}
