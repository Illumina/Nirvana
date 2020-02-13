using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.ProteinConservation;

namespace VariantAnnotation.Providers
{
    public sealed class ProteinConservationProvider:IDisposable
    {
        private readonly ProteinConservationReader _reader;
        public string Name => "Amino acid conservation score provider";
        public IDataSourceVersion Version => _reader.Version;
        private readonly Dictionary<string, byte[]> _conservationScores;

        public ProteinConservationProvider(Stream stream)
        {
            _reader = new ProteinConservationReader(stream);
            _conservationScores = new Dictionary<string, byte[]>(100_000);
        }

        public void Load()
        {
            foreach (var item in _reader.GetItems())
            {
                _conservationScores.Add(item.TranscriptId, item.ConservationScores);
            }
        }

        
        public int GetConservationScore(String transcriptId, int position)
        {
            if (_conservationScores.TryGetValue(transcriptId, out var scores))
                return position < scores.Length ? scores[position - 1] : -1;
            return -1;
        }
        
        public void Dispose() =>_reader?.Dispose();
        

    }
}