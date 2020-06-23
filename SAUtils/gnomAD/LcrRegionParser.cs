using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Providers;

namespace SAUtils.gnomAD
{
    public sealed class LcrRegionParser:IDisposable
    {
        private readonly StreamReader _reader;
        private readonly ISequenceProvider _refProvider;

        private int _nRegionSize;

        public LcrRegionParser(StreamReader reader, ISequenceProvider refProvider)
        {
            _reader = reader;
            _refProvider = refProvider;
        }

        public void Dispose() => _reader?.Dispose();
        public IEnumerable<ISuppIntervalItem> GetItems()
        {
            using (var reader = _reader)
            {
                string line;
                
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == string.Empty || line.StartsWith("#")) continue;

                    ISuppIntervalItem region;
                    try
                    {
                        region = GetLcrRegion(line);
                        if(region ==null) continue;

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        e.Data["Line"] = line;
                        throw;
                    }
                    yield return region;
                }
            }

            Console.WriteLine($"Total size of N-regions:{_nRegionSize}");
        }

        private ISuppIntervalItem GetLcrRegion(string line)
        {
            (string chromName, int start, int end) = ParsePosition(line);
            if (chromName==null) return null; //unknown chromosome
            
            var chromosome = _refProvider.RefNameToChromosome[chromName];
            if (chromosome.IsEmpty()) return null;

            if (_refProvider.Assembly == GenomeAssembly.GRCh38) start++;
            
            return IsNRegion(chromosome, start, end) ? null : new LcrInterval(chromosome, start, end);
        }

        private bool IsNRegion(IChromosome chrom, int start, int end)
        {
            if (_refProvider == null) return false;
            
            _refProvider.LoadChromosome(chrom);
            var sequence = _refProvider.Sequence.Substring(start - 1, end - start + 1);

            if (sequence == null) return false;
            
            foreach (char c in sequence)
            {
                if (c != 'N' && c != 'n') return false;
            }

            _nRegionSize+=end-start+1;
            return true;
        }

        private (string ChromName, int Start, int End) ParsePosition(string line)
        {
            var splits = line.Split(':', '-', '\t');
            var chrom = splits[0];
            if (!_refProvider.RefNameToChromosome.ContainsKey(chrom)) return (null, 0, 0);
            
            var start = int.Parse(splits[1]);
            var end = int.Parse(splits[2]);

            return (chrom, start, end);
        }
    }
}