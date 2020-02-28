using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;

namespace SAUtils.Revel
{
    public sealed class RevelReader : IDisposable
    {
        private const int ChrIndex = 0;
        private const int PosIndex = 1;
        private const int RefIndex = 2;
        private const int AltIndex = 3;
        private const int ScoreIndex = 6;
        private static readonly string[] AltAlleles = {"A", "C", "G", "T"};
        private readonly StreamReader _reader;
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        private readonly IDictionary<string, string> _highestScores;

        public RevelReader(StreamReader reader, IDictionary<string, IChromosome> refNameToChromosome)
        {
            _reader = reader;
            _refNameToChromosome = refNameToChromosome;
            _highestScores = new Dictionary<string, string> {{"A", null}, {"C", null}, {"G", null}, {"T", null}};
        }
        
        public IEnumerable<ISupplementaryDataItem> GetItems()
        {
            //skip the header line
            _reader.ReadLine();
            string line;
            int currentPosition = -1;
            IChromosome currentChromosome = null;
            string refAllele = null;
            while ((line = _reader.ReadLine()) != null)
            {
                var fields = line.OptimizedSplit(',');
                
                if (!_refNameToChromosome.TryGetValue(fields[ChrIndex], out var chromosome)) continue;
                var position = int.Parse(fields[PosIndex]);
                
                if (chromosome != currentChromosome || position != currentPosition)
                {
                    foreach (var revelItem in GetItemsAtOnePosition(currentChromosome, currentPosition, refAllele, _highestScores))
                        yield return revelItem;
                }

                currentChromosome = chromosome;
                currentPosition = position;
                
                refAllele = fields[RefIndex];
                var altAllele = fields[AltIndex];
                if (refAllele.Length != 1 || altAllele.Length != 1)
                    throw new InvalidDataException($"Only SNV is expected in the input file. Exception found: {line}");

                UpdateHighestScores(altAllele, fields[ScoreIndex], _highestScores);
            }
            
            foreach (var revelItem in GetItemsAtOnePosition(currentChromosome, currentPosition, refAllele, _highestScores))
                yield return revelItem;
        }

        private static void UpdateHighestScores(string altAllele, string score, IDictionary<string, string> highestScores)
        {
            if (highestScores[altAllele] == null || 
                double.Parse(highestScores[altAllele], CultureInfo.InvariantCulture) < double.Parse(score, CultureInfo.InvariantCulture)) 
                highestScores[altAllele] = score;
        }

        private static IEnumerable<ISupplementaryDataItem> GetItemsAtOnePosition(IChromosome currentChromosome, int currentPosition, string refAllele, IDictionary<string, string> highestScores)
        {
            if (currentChromosome == null) yield break;
            foreach (var altAllele in AltAlleles)
            {
                var score = highestScores[altAllele];
                if (score == null) continue;
                yield return new RevelItem(currentChromosome, currentPosition, refAllele, altAllele, score);
                highestScores[altAllele] = null;
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}