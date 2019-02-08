using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.Genes.DataStructures;
using Genome;
using Intervals;
using IO;
using OptimizedCore;

namespace CacheUtils.Genes.IO
{
    public sealed class UgaGeneReader : IDisposable
    {
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        private readonly StreamReader _reader;

        public UgaGeneReader(Stream stream, IDictionary<string, IChromosome> refNameToChromosome, bool leaveOpen = false)
        {
            _refNameToChromosome = refNameToChromosome;
            _reader = FileUtilities.GetStreamReader(stream, leaveOpen);
            _reader.ReadLine();
        }

        public void Dispose() => _reader.Dispose();

        public UgaGene[] GetGenes()
        {
            var genes = new List<UgaGene>();

            while (true)
            {
                var gene = GetNextGene();
                if (gene == null) break;
                genes.Add(gene);
            }

            return genes.ToArray();
        }

        private UgaGene GetNextGene()
        {
            string line = _reader.ReadLine();
            if (line == null) return null;

            var cols = line.OptimizedSplit('\t');
            if (cols.Length != 11) throw new InvalidDataException($"Expected 11 columns, but found {cols.Length} columns.");

            string ucscRefName   = cols[0];
            var chromosome       = ReferenceNameUtilities.GetChromosome(_refNameToChromosome, ucscRefName);
            string symbol        = cols[2];
            int start37          = int.Parse(cols[3]);
            int end37            = int.Parse(cols[4]);
            int start38          = int.Parse(cols[5]);
            int end38            = int.Parse(cols[6]);
            bool onReverseStrand = cols[7] == "R";
            int hgncId           = int.Parse(cols[8]);
            string ensemblId     = cols[9];
            string entrezGeneId  = cols[10];

            var grch37 = new Interval(start37, end37);
            var grch38 = new Interval(start38, end38);

            return new UgaGene(chromosome, grch37, grch38, onReverseStrand, entrezGeneId, ensemblId, symbol, hgncId);
        }
    }
}
