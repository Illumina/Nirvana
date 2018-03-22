using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CacheUtils.Genes.DataStructures;
using CommonUtilities;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;

namespace CacheUtils.Genes.IO
{
    public sealed class UgaGeneReader : IDisposable
    {
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        private readonly StreamReader _reader;

        public UgaGeneReader(Stream stream, IDictionary<string, IChromosome> refNameToChromosome, bool leaveOpen = false)
        {
            _refNameToChromosome = refNameToChromosome;
            _reader = new StreamReader(stream, Encoding.ASCII, leaveOpen);
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
            var line = _reader.ReadLine();
            if (line == null) return null;

            var cols = line.Split('\t');
            if (cols.Length != 11) throw new InvalidDataException($"Expected 11 columns, but found {cols.Length} columns.");

            var ucscRefName     = cols[0];
            var chromosome      = ReferenceNameUtilities.GetChromosome(_refNameToChromosome, ucscRefName);
            var symbol          = cols[2];
            var start37         = int.Parse(cols[3]);
            var end37           = int.Parse(cols[4]);
            var start38         = int.Parse(cols[5]);
            var end38           = int.Parse(cols[6]);
            var onReverseStrand = cols[7] == "R";
            var hgncId          = int.Parse(cols[8]);
            var ensemblId       = cols[9];
            var entrezGeneId    = cols[10];

            var grch37 = new Interval(start37, end37);
            var grch38 = new Interval(start38, end38);
            return new UgaGene(chromosome, grch37, grch38, onReverseStrand, entrezGeneId, ensemblId, symbol, hgncId);
        }
    }
}
