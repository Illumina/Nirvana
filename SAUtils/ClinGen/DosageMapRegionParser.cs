using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using OptimizedCore;

namespace SAUtils.ClinGen
{
    public sealed class DosageMapRegionParser : IDisposable
    {
        private readonly Stream _stream;
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;

        private const string GenomicLocation = "Genomic Location";
        private const string HaploInsufficiencyScoreTag = "Haploinsufficiency Score";
        private const string TriploSensitivityScoreTag  = "Triplosensitivity Score";

        private int _genomicLocationIndex = -1;
        private int _haploInsufficiencyScoreIndex = -1;
        private int _triploSensitivityScoreIndex  = -1;
        public DosageMapRegionParser(Stream stream, IDictionary<string, IChromosome> refNameToChromosome)
        {
            _stream = stream;
            _refNameToChromosome = refNameToChromosome;
        }
        
        public void Dispose()
        {
            _stream?.Dispose();
        }
        
        public IEnumerable<DosageMapRegionItem> GetItems()
        {
            using (var reader = new StreamReader(_stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                    {
                        ParseHeaderLine(line);
                    }
                    else
                    {
                        var item = GetDosageMapRegionItem(line, _refNameToChromosome);
                        if (item != null) yield return item;
                    }
                }
            }
        }

        private DosageMapRegionItem GetDosageMapRegionItem(string line, IDictionary<string, IChromosome> refNameToChromosome)
        {
            var fields = line.OptimizedSplit('\t');
            string genomicLocation = fields[_genomicLocationIndex];
            (string chromName, int start, int end) = ParseGenomeLocation(genomicLocation);
            if (chromName == null) return null;
            if (!refNameToChromosome.TryGetValue(chromName, out var chrom)) return null;
            
            string haploInsufficiencyScore = fields[_haploInsufficiencyScoreIndex];
            string triploSensitivityScore = fields[_triploSensitivityScoreIndex];
            
            if (!int.TryParse(haploInsufficiencyScore, out int hiScore)) hiScore = -1;
            if (!int.TryParse(triploSensitivityScore, out int tsScore)) tsScore  = -1;
            
            return new DosageMapRegionItem(chrom, start, end, hiScore, tsScore);
        }


        private void ParseHeaderLine(string line)
        {
            if (line.StartsWith("#ISCA ID")) GetColumnIndices(line);
        }
        
        private void GetColumnIndices(string line)
        {
            var cols = line.OptimizedSplit('\t');

            _genomicLocationIndex = Array.IndexOf(cols, GenomicLocation);
            _haploInsufficiencyScoreIndex = Array.IndexOf(cols, HaploInsufficiencyScoreTag);
            _triploSensitivityScoreIndex  = Array.IndexOf(cols, TriploSensitivityScoreTag);
            
            if (_genomicLocationIndex == -1 || _haploInsufficiencyScoreIndex == -1 || _triploSensitivityScoreIndex == -1)
                throw new InvalidDataException("Column indices not set!!");
        }

        private static (string chromName, int Start, int End) ParseGenomeLocation(string genomeLocation)
        {
            int index1 = genomeLocation.IndexOf(':');
            int index2 = genomeLocation.IndexOf('-');
            if (index1 < 0 || index2 < 0)
            {
                Console.WriteLine($"Not able to parse {genomeLocation}");
                return (null, -1, -1);
            }
            string chromName = genomeLocation.Substring(0, index1);
            int start = int.Parse(genomeLocation.Substring(index1 + 1, index2 - index1 - 1));
            int end = int.Parse(genomeLocation.Substring(index2 + 1));
            return (chromName, start, end);
        }
    }
}