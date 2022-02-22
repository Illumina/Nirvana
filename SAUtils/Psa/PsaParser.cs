using System;
using System.Collections.Generic;
using System.IO;
using OptimizedCore;
using VariantAnnotation.PSA;

namespace SAUtils.Psa
{
    public sealed class PsaParser : IDisposable
    {
        private readonly StreamReader _reader;

        private const int ChromosomeIndex   = 0;
        private const int TranscriptIdIndex = 1;
        private const int AaPositionIndex   = 2;
        private const int RefAaIndex        = 3;
        private const int AltAaIndex        = 4;
        private const int ScoreIndex        = 5;
        private const int PredictionIndex   = 6;


        public PsaParser(StreamReader reader)
        {
            _reader = reader;
        }

        public IEnumerable<PsaDataItem> GetItems()
        {
            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                var item = GetNextItem(line);
                if (item == null) continue;
                yield return item;
            }
        }

        private PsaDataItem GetNextItem(string line)
        {
            if (string.IsNullOrEmpty(line) || line.StartsWith('#')) return null;

            var words = line.OptimizedSplit('\t');

            if (words[AaPositionIndex] == "NA" || words[ScoreIndex] == "NA") return null;

            var    chromName    = words[ChromosomeIndex];
            var    position     = int.Parse(words[AaPositionIndex]);
            var    transcriptId = words[TranscriptIdIndex];
            var    refAa        = words[RefAaIndex][0];
            var    altAa        = words[AltAaIndex][0];
            double score        = double.Parse(words[ScoreIndex]);
            var    shortScore   = PsaUtilities.GetUshortScore(score);
            var    prediction   = words[PredictionIndex];

            return new PsaDataItem(chromName, transcriptId, position, refAa, altAa, shortScore, prediction);
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}