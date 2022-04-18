using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ErrorHandling.Exceptions;
using Genome;
using OptimizedCore;

namespace SAUtils.GenericScore.GenericScoreParser
{
    public sealed class GenericScoreParser : IDisposable
    {
        private readonly ParserSettings _parserSettings;

        private readonly StreamReader                     _reader;
        private readonly Dictionary<string, Chromosome> _refNameToChromosome;
        private readonly Dictionary<string, double>      _representativeScores;

        private readonly Action<string, double, Dictionary<string, double>> _updateRepresentativeScores;

        public GenericScoreParser(
            ParserSettings parserSettings,
            StreamReader reader,
            Dictionary<string, Chromosome> refNameToChromosome
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

            ColumnIndex columnIndex = _parserSettings.ColumnIndex;

            while ((line = _reader.ReadLine()) != null)
            {
                if (line.StartsWith("#")) continue;

                string[] fields = line.OptimizedSplit('\t');

                if (!_refNameToChromosome.TryGetValue(fields[columnIndex.Chromosome], out var chromosome)) continue;
                int position = int.Parse(fields[columnIndex.Position]);

                if (chromosome != currentChromosome || position != currentPosition)
                {
                    foreach (GenericScoreItem scoreItem in GetItemsAtOnePosition(currentChromosome, currentPosition, refAllele))
                        yield return scoreItem;
                }

                currentChromosome = chromosome;
                currentPosition   = position;

                // add null checks for alleles
                refAllele = columnIndex.RefAllele == null ? null : fields[columnIndex.RefAllele.Value];
                string altAllele = columnIndex.AltAllele == null ? null : fields[columnIndex.AltAllele.Value];

                // set saItem.AltAllele to 'N' if positional
                if (_parserSettings.IsPositional) altAllele = "N";

                if (double.TryParse(fields[columnIndex.Score], NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out double score))
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
                double score = _representativeScores[altAllele];
                if (double.IsNaN(score)) continue;
                yield return new GenericScoreItem(currentChromosome, currentPosition, refAllele, altAllele, score);
                _representativeScores[altAllele] = double.NaN;
            }
        }

        public static void MaxRepresentativeScores(string altAllele, double score, Dictionary<string, double> highestScores)
        {
            if (double.IsNaN(highestScores[altAllele]) || highestScores[altAllele] < score)
                highestScores[altAllele] = score;
        }

        public static void NonConflictingScore(string altAllele, double score, Dictionary<string, double> highestScores)
        {
            if (!double.IsNaN(highestScores[altAllele]))
                throw new UserErrorException("Multiple scores oberved.");

            highestScores[altAllele] = score;
        }

        public static void MinRepresentativeScores(string altAllele, double score, Dictionary<string, double> highestScores)
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