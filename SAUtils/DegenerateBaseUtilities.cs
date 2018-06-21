using System.Collections.Generic;

namespace SAUtils
{
    public static class DegenerateBaseUtilities
    {
        private static readonly Dictionary<char, List<char>> DegenerateBaseNotation = new Dictionary<char, List<char>>
        {
            {'B', new List<char>{'C','G','T'}},
            {'D', new List<char>{'A','G','T'}},
            {'H', new List<char>{'A','C','T'}},
            {'K', new List<char>{'G','T'}},
            {'M', new List<char>{'A','C'}},
            {'R', new List<char>{'A','G'}},
            {'S', new List<char>{'C','G'}},
            {'V', new List<char>{'A','C','G'}},
            {'W', new List<char>{'A','T'}},
            {'Y', new List<char>{'C','T'}}
        };

        public static List<string> GetAllPossibleSequences(string sequenceWithDegenerateBases)
        {
            var sequences = new List<string>();
            GetSequences(sequenceWithDegenerateBases.ToUpper(), sequences, 0, "");
            return sequences;
        }

        private static void GetSequences(string inputSequence, ICollection<string> outputSequences, int index, string subSequence)
        {
            if (index == inputSequence.Length)
            {
                outputSequences.Add(subSequence);
                return;
            }
            MapBase(inputSequence[index]).ForEach(x =>
                GetSequences(inputSequence, outputSequences, index + 1, subSequence + x));

        }

        private static List<char> MapBase(char inputBase) => DegenerateBaseNotation.ContainsKey(inputBase) ? DegenerateBaseNotation[inputBase] : new List<char> {inputBase};
    }
}