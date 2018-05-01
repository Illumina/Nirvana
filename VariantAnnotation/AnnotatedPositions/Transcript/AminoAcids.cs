using System;
using System.Collections.Generic;
using OptimizedCore;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class AminoAcids
    {
        public const string StopCodon   = "*";
        public const char StopCodonChar = '*';

        private readonly CodonConversion _codonConversionScheme = CodonConversion.HumanChromosome;

        private readonly Dictionary<string, char> _aminoAcidLookupTable;
        private readonly Dictionary<string, char> _mitoDifferences;

        // converts single letter amino acid ambiguity codes to three
        // letter abbreviations
        private static readonly Dictionary<char, string> SingleToThreeAminoAcids = new Dictionary<char, string>
        {
            {'A', "Ala"},
            {'B', "Asx"},
            {'C', "Cys"},
            {'D', "Asp"},
            {'E', "Glu"},
            {'F', "Phe"},
            {'G', "Gly"},
            {'H', "His"},
            {'I', "Ile"},
            {'K', "Lys"},
            {'L', "Leu"},
            {'M', "Met"},
            {'N', "Asn"},
            {'P', "Pro"},
            {'Q', "Gln"},
            {'R', "Arg"},
            {'S', "Ser"},
            {'T', "Thr"},
            {'V', "Val"},
            {'W', "Trp"},
            {'Y', "Tyr"},
            {'Z', "Glx"},
            {'X', "Ter"}, // Ter now recommended in HGVS
		    {'*', "Ter"},
            {'U', "Sec"},
            {'O', "Pyl"},
            {'J', "Xle"},
            {'?', "_?_"} //deletion at the end of incomplete transcript results in unknown change
        };

        private enum CodonConversion : byte
        {
            HumanChromosome,
            HumanMitochondrion
        }

        public AminoAcids(bool isMitochondrial)
        {
            if (isMitochondrial) _codonConversionScheme = CodonConversion.HumanMitochondrion;

            _aminoAcidLookupTable = new Dictionary<string, char>
            {
                // 2nd base: T
                {"TTT", 'F'},
                {"TTC", 'F'},
                {"TTA", 'L'},
                {"TTG", 'L'},
                {"CTT", 'L'},
                {"CTC", 'L'},
                {"CTA", 'L'},
                {"CTG", 'L'},
                {"ATT", 'I'},
                {"ATC", 'I'},
                {"ATA", 'I'},
                {"ATG", 'M'},
                {"GTT", 'V'},
                {"GTC", 'V'},
                {"GTA", 'V'},
                {"GTG", 'V'},

                // 2nd base: C
                {"TCT", 'S'},
                {"TCC", 'S'},
                {"TCA", 'S'},
                {"TCG", 'S'},
                {"CCT", 'P'},
                {"CCC", 'P'},
                {"CCA", 'P'},
                {"CCG", 'P'},
                {"ACT", 'T'},
                {"ACC", 'T'},
                {"ACA", 'T'},
                {"ACG", 'T'},
                {"GCT", 'A'},
                {"GCC", 'A'},
                {"GCA", 'A'},
                {"GCG", 'A'},

                // 2nd base: A
                {"TAT", 'Y'},
                {"TAC", 'Y'},
                {"TAA", '*'},
                {"TAG", '*'},
                {"CAT", 'H'},
                {"CAC", 'H'},
                {"CAA", 'Q'},
                {"CAG", 'Q'},
                {"AAT", 'N'},
                {"AAC", 'N'},
                {"AAA", 'K'},
                {"AAG", 'K'},
                {"GAT", 'D'},
                {"GAC", 'D'},
                {"GAA", 'E'},
                {"GAG", 'E'},

                // 2nd base: G
                {"TGT", 'C'},
                {"TGC", 'C'},
                {"TGA", '*'},
                {"TGG", 'W'},
                {"CGT", 'R'},
                {"CGC", 'R'},
                {"CGA", 'R'},
                {"CGG", 'R'},
                {"AGT", 'S'},
                {"AGC", 'S'},
                {"AGA", 'R'},
                {"AGG", 'R'},
                {"GGT", 'G'},
                {"GGC", 'G'},
                {"GGA", 'G'},
                {"GGG", 'G'}
            };

            _mitoDifferences = new Dictionary<string, char>
            {
                {"ATA", 'M'},
                {"TGA", 'W'},
                {"AGA", '*'},
                {"AGG", '*'}
            };
        }

        internal static string AddUnknownAminoAcid(string aminoAcids) => aminoAcids == StopCodon ? aminoAcids : aminoAcids + 'X';

        public (string Reference, string Alternate) Translate(string referenceCodons,
            string alternateCodons)
        {
            if (string.IsNullOrEmpty(referenceCodons) && string.IsNullOrEmpty(alternateCodons)) return ("", "");
            if (referenceCodons != null && (referenceCodons.Contains("N") || alternateCodons.Contains("N"))) return ("", "");

            var referenceAminoAcids = TranslateBases(referenceCodons, false);
            var alternateAminoAcids = TranslateBases(alternateCodons, false);
            return (referenceAminoAcids, alternateAminoAcids);
        }

        /// <summary>
        /// converts a DNA triplet to the appropriate amino acid abbreviation
        /// </summary>
        public static string ConvertAminoAcidToAbbreviation(char aminoAcid)
        {
            if (!SingleToThreeAminoAcids.TryGetValue(aminoAcid, out var abbreviation))
            {
                throw new NotSupportedException($"Unable to convert the following string to an amino acid abbreviation: {aminoAcid}");
            }

            return abbreviation;
        }

        /// <summary>
        /// converts a DNA triplet to the appropriate amino acid abbreviation
        /// The default conversion is human chromosomes. The second parameter also allows the user to specify other codon conversions like mitochondria, etc.
        /// </summary>
        internal char ConvertTripletToAminoAcid(string triplet)
        {
            var upperTriplet = triplet.ToUpper();

            // check our exceptions first
            if (_codonConversionScheme == CodonConversion.HumanMitochondrion &&
                _mitoDifferences.TryGetValue(upperTriplet, out var mitoAminoAcid)) return mitoAminoAcid;

            // the default case
            return _aminoAcidLookupTable.TryGetValue(upperTriplet, out var aminoAcid) ? aminoAcid : 'X';
        }


        /// <summary>
        /// given a string of 1-letter amino acid ambiguity codes, this function
        /// returns a string of 3-letter amino acid abbreviations up until the first
        /// stop codon.
        /// </summary>
        public static string GetAbbreviations(string aminoAcids)
        {
            if (string.IsNullOrEmpty(aminoAcids)) return "";
            if (aminoAcids.Length == 1) return ConvertAminoAcidToAbbreviation(aminoAcids[0]);

            var sb = StringBuilderCache.Acquire();

            foreach (var aminoAcid in aminoAcids)
            {
                sb.Append(ConvertAminoAcidToAbbreviation(aminoAcid));
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        /// <summary>
        /// returns a string of single-letter amino acids translated from a string of bases. 
        /// The bases must already be grouped by triplets (i.e. len must be a multiple of 3)
        /// </summary>
        public string TranslateBases(string bases, bool forceNonTriplet)
        {
            // sanity check: handle the empty case
            if (bases == null) return null;

            var numAminoAcids = bases.Length / 3;

            // check if we have a non triplet case
            var nonTriplet = !forceNonTriplet && numAminoAcids * 3 != bases.Length;

            // special case: single amino acid
            string aminoAcidString;
            if (numAminoAcids == 1)
            {
                aminoAcidString =
                    ConvertTripletToAminoAcid(bases.Substring(0, 3 * numAminoAcids))
                        .ToString();
                return nonTriplet ? AddUnknownAminoAcid(aminoAcidString) : aminoAcidString;
            }

            // multiple amino acid case
            var aminoAcids = new char[numAminoAcids];
            for (var i = 0; i < numAminoAcids; i++)
            {
                aminoAcids[i] = ConvertTripletToAminoAcid(bases.Substring(i * 3, 3));
            }

            aminoAcidString = new string(aminoAcids);
            return nonTriplet ? AddUnknownAminoAcid(aminoAcidString) : aminoAcidString;
        }
    }
}

