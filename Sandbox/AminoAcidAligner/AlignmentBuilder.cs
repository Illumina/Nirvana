using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AminoAcidAligner
{
    public class AlignmentBuilder
    {
        public readonly string TranscriptId;
        public string Chromosome;
        private readonly Dictionary<string, StringBuilder> _speciesAlignment;

        public AlignmentBuilder(string id)
        {
            TranscriptId = id;
            _speciesAlignment = new Dictionary<string, StringBuilder>(100);//since we are doing 100 way alignment
        }

        public void Add(string transcriptId, string species, string sequence)
        {
            if (TranscriptId != transcriptId) return;
            if (_speciesAlignment.TryGetValue(species, out var sb))
            {
                sb.Append(sequence);
            }
            else
            {
                _speciesAlignment[species] = new StringBuilder();
                _speciesAlignment[species].Append(sequence);
            }
        }

        public override string ToString()
        {
            if(!CheckAlignments()) throw new DataMisalignedException($"Alignment issues found for {TranscriptId}");

            var sb = new StringBuilder();
            foreach (var (species, sequence) in _speciesAlignment)
            {
                sb.Append($"{species}\t{sequence}\n");
            }

            return sb.ToString();
        }

        private bool CheckAlignments()
        {
            var length = -1;
            //checking if all the alignments have same length
            foreach (var sequence in _speciesAlignment.Values)
            {
                if (length == -1) length = sequence.Length;
                if (length != sequence.Length) return false;
            }

            //check if there are any '-' es in Human
            StringBuilder humanSb;
            if (!_speciesAlignment.TryGetValue("hg38", out humanSb) && !_speciesAlignment.TryGetValue("hg19", out humanSb)) return true;
            var hg38Sequence = humanSb.ToString();
            if (hg38Sequence.Contains('-'))
                Console.WriteLine($"Human sequence contains - in {TranscriptId}");

            return true;
        }

        public string GetScoresLine()
        {
            var sb = new StringBuilder();
            
            StringBuilder humanSb;
            string humanSequence=null;
            if (_speciesAlignment.TryGetValue("hg38", out humanSb) ||
                _speciesAlignment.TryGetValue("hg19", out humanSb))
            {
                humanSequence = humanSb.ToString();
            }

            if(humanSequence == null) throw new InvalidDataException($"No human sequence available for {TranscriptId}");

            sb.Append($"{TranscriptId}\t{Chromosome}\t{humanSequence}");

            var residueCount = new int[humanSequence.Length];
            Array.Fill(residueCount, 0);

            foreach (var alignment in _speciesAlignment.Values)
            {
                for (int i = 0; i < humanSequence.Length; i++)
                {
                    if (humanSequence[i] == alignment[i]) residueCount[i]++;
                }
            }

            sb.Append('\t');
            sb.Append(string.Join(',', residueCount.Select(x => 100 * x / _speciesAlignment.Count)));
            
            return sb.ToString();
        }
    }
}