using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OptimizedCore;
using VariantAnnotation.ProteinConservation;

namespace SAUtils.AAConservation
{
    public sealed class ProteinConservationParser:IDisposable
    {
        private readonly Stream _stream;

        private int _ensemblIdsIndex = -1;
        private int _chromIndex = -1;
        private int _scoresIndex = -1;
        private int _proteinSeqIndex = -1;

        private const string EnsemblIdsTag = "Ensembl";
        private const string ProteinSequenceTag  = "ProteinSequence";
        private const string ChromTag = "Chromosome";
        private const string ScoresTag = "Percent Conservation at each AA residue";

        public ProteinConservationParser(Stream stream)
        {
            _stream = stream;
        }

        public IEnumerable<ProteinConservationItem> GetItems()
        {
            using (var reader = new StreamReader(_stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var columns = line.OptimizedSplit('\t');
                    if (line.StartsWith("#"))
                    {
                        ParseHeader(line);
                        continue;
                    }

                    var transcriptId    = columns[_ensemblIdsIndex];
                    var proteinSequence = columns[_proteinSeqIndex];
                    var chromosome      = columns[_chromIndex];
                    var scores = columns[_scoresIndex].OptimizedSplit(',').Select(x => (byte) int.Parse(x))
                        .ToArray();
                    
                    yield return new ProteinConservationItem(chromosome, transcriptId, proteinSequence, scores);
                }
            }

        }

        private void ParseHeader(string line)
        {
            var columnTags = line.TrimStart('#').OptimizedSplit('\t');

            _ensemblIdsIndex = Array.IndexOf(columnTags, EnsemblIdsTag);
            _chromIndex = Array.IndexOf(columnTags, ChromTag);
            _scoresIndex = Array.IndexOf(columnTags, ScoresTag);
            _proteinSeqIndex = Array.IndexOf(columnTags, ProteinSequenceTag);
        }

        public void Dispose()=>_stream?.Dispose(); 
        
    }
}