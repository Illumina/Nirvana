using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Genome;
using OptimizedCore;

namespace SAUtils.GenericScore.GenericScoreParser
{
    public sealed class GenericScoreParser : IDisposable
    {
        private readonly ParserSettings _parserSettings;

        private readonly StreamReader                     _reader;
        private readonly IDictionary<string, Chromosome> _refNameToChromosome;
        private readonly IDictionary<string, double>      _representativeScores;

        private readonly Action<string, double, IDictionary<string, double>> _updateRepresentativeScores;

        public GenericScoreParser(
            ParserSettings parserSettings,
            StreamReader reader,
            IDictionary<string, Chromosome> refNameToChromosome
        )
        {
            _reader               = reader;
            _refNameToChromosome  = refNameToChromosome;
            _parserSettings       = parserSettings;
            _representativeScores = new Dictionary<string, double>();
            foreach (string allele in _parserSettings.PossibleAlleles)
            {
                _representativeScores[allele] = double.NaN;
            }

            _updateRepresentativeScores = _parserSettings.ConflictResolutionFunction;
        }

        public IEnumerable<GenericScoreItem> GetItems()
        {
            string      line;
            int         currentPosition   = -1;
            Chromosome currentChromosome = null;
            string      refAllele         = null;

            ColumnPositions columnPositions = _parserSettings.ColumnPositions;

            while ((line = _reader.ReadLine()) != null)
            {
                if (line.StartsWith("#")) continue;

                string[] fields = line.OptimizedSplit('\t');

                if (!_refNameToChromosome.TryGetValue(fields[columnPositions.Chromosome], out var chromosome)) continue;
                int position = int.Parse(fields[columnPositions.Position]);

                if (chromosome != currentChromosome || position != currentPosition)
                {
                    foreach (GenericScoreItem scoreItem in GetItemsAtOnePosition(currentChromosome, currentPosition, refAllele))
                        yield return scoreItem;
                }

                currentChromosome = chromosome;
                currentPosition   = position;

                refAllele = fields[columnPositions.RefAllele];
                string altAllele = fields[columnPositions.AltAllele];
                if (refAllele.Length != 1 || altAllele.Length != 1)
                    throw new InvalidDataException($"Only SNV is expected in the input file. Exception found: {line}");

                if (double.TryParse(fields[columnPositions.Score], NumberStyles.Number, CultureInfo.InvariantCulture, out double score))
                {
                    _updateRepresentativeScores(altAllele, score, _representativeScores);
                }
            }

            foreach (var scoreItem in GetItemsAtOnePosition(currentChromosome, currentPosition, refAllele))
                yield return scoreItem;
        }

        private IEnumerable<GenericScoreItem> GetItemsAtOnePosition(Chromosome currentChromosome, int currentPosition, string refAllele)
        {
            if (currentChromosome == null) yield break;
            foreach (string altAllele in _parserSettings.PossibleAlleles)
            {
                var score = _representativeScores[altAllele];
                if (double.IsNaN(score)) continue;
                yield return new GenericScoreItem(currentChromosome, currentPosition, refAllele, altAllele, score);
                _representativeScores[altAllele] = double.NaN;
            }
        }

        public static void MaxRepresentativeScores(string altAllele, double score, IDictionary<string, double> highestScores)
        {
            if (double.IsNaN(highestScores[altAllele]) || highestScores[altAllele] < score)
                highestScores[altAllele] = score;
        }

        public static void MinRepresentativeScores(string altAllele, double score, IDictionary<string, double> highestScores)
        {
            if (double.IsNaN(highestScores[altAllele]) || highestScores[altAllele] > score)
                highestScores[altAllele] = score;
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}