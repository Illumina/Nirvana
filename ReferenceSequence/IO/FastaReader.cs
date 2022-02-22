using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using Genome;
using ReferenceSequence.Creation;

namespace ReferenceSequence.IO
{
    internal static class FastaReader
    {
        private static readonly Regex NameRegex = new Regex("^>(\\S+)", RegexOptions.Compiled);

        // >gi|224589823|ref|NC_000024.9|
        private static readonly Regex NcbiRegex = new Regex("^>gi\\|\\d+\\|ref\\|([^|]+)\\|", RegexOptions.Compiled);

        // >ref|NC_000013.11| Homo sapiens chromosome 13, GRCh38.p12 Primary Assembly
        private static readonly Regex NcbiRegex2 = new Regex("^>ref\\|([^|]+)\\|", RegexOptions.Compiled);

        internal static void AddReferenceSequences(Stream stream, Dictionary<string, Chromosome> refNameToChromosome, List<FastaSequence> references)
        {
            var sb = new StringBuilder();

            using (var reader = new StreamReader(stream))
            {
                var queue = new Queue<string>();

                while (true)
                {
                    string input = queue.Count > 0 ? queue.Dequeue() : reader.ReadLine();
                    if (input == null) break;

                    if (!input.StartsWith(">")) throw new UserErrorException($"Encountered a FASTA header that did not start with '>': {input}");

                    string name       = GetName(input);
                    var    chromosome = GetChromosome(refNameToChromosome, name);
                    string bases      = GetBases(sb, reader, queue);

                    references.Add(new FastaSequence(chromosome, bases));
                }
            }
        }

        private static string GetBases(StringBuilder sb, StreamReader reader, Queue<string> queue)
        {
            sb.Clear();

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null) break;

                if (line.StartsWith('>'))
                {
                    queue.Enqueue(line);
                    break;
                }

                sb.Append(line);
            }

            return sb.ToString();
        }

        private static Chromosome GetChromosome(Dictionary<string, Chromosome> refNameToChromosome, string name)
        {
            var chromosome = ReferenceNameUtilities.GetChromosome(refNameToChromosome, name);

            if (chromosome.IsEmpty)
            {
                throw new InvalidDataException($"Could not find the chromosome ({name}) in the reference name dictionary.");
            }

            return chromosome;
        }

        private static string GetName(string s)
        {
            var match = NcbiRegex2.Match(s);
            if (match.Success) return match.Groups[1].Value;

            match = NcbiRegex.Match(s);
            if (match.Success) return match.Groups[1].Value;

            match = NameRegex.Match(s);
            if (match.Success) return match.Groups[1].Value;

            throw new InvalidDataException($"Unable to match the regex to the chromosome name ({s})");
        }
    }
}