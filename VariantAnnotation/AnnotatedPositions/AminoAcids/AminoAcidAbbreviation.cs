using System;
using OptimizedCore;

namespace VariantAnnotation.AnnotatedPositions.AminoAcids
{
    public static class AminoAcidAbbreviation
    {
        private static readonly AbbreviationEntry[] Abbreviations =
        {
            new AbbreviationEntry('*', "Ter"),
            new AbbreviationEntry('A', "Ala"),
            new AbbreviationEntry('B', "Asx"), // not expected, included for completeness
            new AbbreviationEntry('C', "Cys"),
            new AbbreviationEntry('D', "Asp"),
            new AbbreviationEntry('E', "Glu"),
            new AbbreviationEntry('F', "Phe"),
            new AbbreviationEntry('G', "Gly"),
            new AbbreviationEntry('H', "His"),
            new AbbreviationEntry('I', "Ile"),
            new AbbreviationEntry('J', "Xle"), // not expected, included for completeness
            new AbbreviationEntry('K', "Lys"),
            new AbbreviationEntry('L', "Leu"),
            new AbbreviationEntry('M', "Met"),
            new AbbreviationEntry('N', "Asn"),
            new AbbreviationEntry('O', "Pyl"), // rare - pyrrolysine
            new AbbreviationEntry('P', "Pro"),
            new AbbreviationEntry('Q', "Gln"),
            new AbbreviationEntry('R', "Arg"),
            new AbbreviationEntry('S', "Ser"),
            new AbbreviationEntry('T', "Thr"),
            new AbbreviationEntry('U', "Sec"), // rare - selenocysteine
            new AbbreviationEntry('V', "Val"),
            new AbbreviationEntry('W', "Trp"),
            new AbbreviationEntry('X', "Xaa"), // not expected, included for completeness
            new AbbreviationEntry('Y', "Tyr"),
            new AbbreviationEntry('Z', "Glx")  // not expected, included for completeness
        };

        private static readonly int EndIndex = Abbreviations.Length - 1;
        
        public static string GetThreeLetterAbbreviation(char oneLetterCode) => BinarySearch(oneLetterCode);
        
        public static string ConvertToThreeLetterAbbreviations(string aminoAcids)
        {
            if (string.IsNullOrEmpty(aminoAcids)) return "";

            var sb = StringBuilderCache.Acquire();
            foreach (char oneLetterCode in aminoAcids) sb.Append(BinarySearch(oneLetterCode));
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        private static string BinarySearch(char oneLetterCode)
        {
            var begin = 0;
            int end   = EndIndex;

            while (begin <= end)
            {
                int index = begin + (end - begin >> 1);
                var entry = Abbreviations[index];

                if (entry.OneLetterCode == oneLetterCode) return entry.ThreeLetterCode;
                if (entry.OneLetterCode < oneLetterCode) begin = index + 1;
                else end                                       = index - 1;
            }

            throw new NotSupportedException($"Unable to convert the following 1-letter code to a 3-letter amino acid abbreviation: {oneLetterCode}");
        }
        
        private readonly struct AbbreviationEntry
        {
            public readonly char  OneLetterCode;
            public readonly string ThreeLetterCode;
        
            public AbbreviationEntry(char oneLetterCode, string threeLetterCode)
            {
                OneLetterCode   = oneLetterCode;
                ThreeLetterCode = threeLetterCode;
            }
        }
    }
}