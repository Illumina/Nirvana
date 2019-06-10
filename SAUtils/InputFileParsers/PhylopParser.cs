using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using OptimizedCore;
using SAUtils.DataStructures;

namespace SAUtils.InputFileParsers
{
    public sealed class PhylopParser : IDisposable
    {
        private readonly Stream _stream;
        private readonly GenomeAssembly _assembly;
        private readonly IDictionary<string, IChromosome> _refChromDict;

        public PhylopParser(Stream stream, GenomeAssembly assembly, IDictionary<string, IChromosome> refChromDict)
        {
            _stream = stream;
            _assembly = assembly;
            _refChromDict = refChromDict;
        }

        public IEnumerable<PhylopItem> GetItems()
        {
            using (var reader = FileUtilities.GetStreamReader(_stream))
            {
                IChromosome chrom = null;
                int position = 0;
                int step = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (double.TryParse(line, out double score))
                    {
                        // the chrom is unrecognized, so we skip
                        if (chrom ==null || chrom.Index==ushort.MaxValue) continue;
                        // since phylop used hg19, we skip entries for chrM
                        if (_assembly == GenomeAssembly.GRCh37 && chrom.UcscName == "chrM") continue;
                        // this is a phylop score
                        yield return new PhylopItem(chrom, position, score);
                        position += step;
                    }
                    else
                    {
                        (chrom, position, step) = StartNewInterval(line);
                    }

                }
            }
        }

        private (IChromosome chrom, int position, int step) StartNewInterval(string line)
        {
            var words = line.Split();
            string chromName = words[1].OptimizedKeyValue().Value;

            var chrom = _refChromDict.TryGetValue(chromName, out var chromosome)? chromosome: new EmptyChromosome(chromName);
            if (chrom.Index == ushort.MaxValue) return (chrom, 0, 0);

            int position = int.Parse(words[2].OptimizedKeyValue().Value);
            int step = short.Parse(words[3].OptimizedKeyValue().Value);

            return (chrom, position, step);
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}