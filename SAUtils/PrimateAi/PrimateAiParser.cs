using System;
using System.Collections.Generic;
using System.IO;
using IO;
using OptimizedCore;
using VariantAnnotation.Interface.Providers;

namespace SAUtils.PrimateAi
{
    public sealed class PrimateAiParser : IDisposable
    {
        private readonly Stream _stream;
        private readonly ISequenceProvider _referenceProvider;
        private readonly Dictionary<string, string> _entrezToHgnc;
        private readonly Dictionary<string, string> _ensemblToHgnc;

        public PrimateAiParser(Stream stream, ISequenceProvider referenceProvider, Dictionary<string, string> entrezToHgnc, Dictionary<string, string> ensemblToHgnc)
        {
            _stream            = stream;
            _entrezToHgnc      = entrezToHgnc;
            _ensemblToHgnc     = ensemblToHgnc;
            _referenceProvider = referenceProvider;
        }


        public IEnumerable<PrimateAiItem> GetItems()
        {

            using (var reader = FileUtilities.GetStreamReader(_stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Skip comments.
                    if (line.OptimizedStartsWith('#')) continue;

                    var item = ExtractItem(line);
                    if (item == null) continue;
                    yield return item;
                }
            }

            Console.WriteLine($"Number of entries:{_count}. Entries without hgnc:{_nullGeneCount} ({100.0*_nullGeneCount/_count} %)");
        }
        //#CHROM  POS     REF     ALT     GeneId  ScorePercentile
        //1       69094   G A       79501   0.79
        private int _nullGeneCount;
        private int _count;
        private PrimateAiItem ExtractItem(string line)
        {
            var splits = line.Split('\t');
            var chromosomeName = splits[0];
            if (!_referenceProvider.RefNameToChromosome.ContainsKey(chromosomeName)) return null;

            var chromosome = _referenceProvider.RefNameToChromosome[chromosomeName];
            var position   = int.Parse(splits[1]);
            var refAllele  = splits[2];
            var altAllele  = splits[3];
            var geneId     = splits[4];
            var percentile = double.Parse(splits[5]);

            string hgnc=null;
            if (_entrezToHgnc.ContainsKey(geneId))  hgnc = _entrezToHgnc[geneId];
            if (_ensemblToHgnc.ContainsKey(geneId)) hgnc = _ensemblToHgnc[geneId];

            if (string.IsNullOrEmpty(hgnc))
            {
                _nullGeneCount++;
                return null;
            }

            _count++;
            return new PrimateAiItem(chromosome, position, refAllele, altAllele, hgnc, percentile);
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _referenceProvider?.Dispose();
        }
    }
}