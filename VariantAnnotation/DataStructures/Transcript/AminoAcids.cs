using System;
using System.Collections.Generic;
using System.Text;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures.Annotation;

namespace VariantAnnotation.DataStructures.Transcript
{
    public class AminoAcids
    {
        #region members

        public const string StopCodon   = "*";
        public const char StopCodonChar = '*';

        public CodonConversion CodonConversionScheme = CodonConversion.HumanChromosome;

        private readonly Dictionary<string, char> _aminoAcidLookupTable;
        private readonly Dictionary<string, char> _mitoDifferences;
        private readonly Dictionary<char, string> _singleToThreeAminoAcids;

        #endregion

        public enum CodonConversion : byte
        {
            HumanChromosome,
            HumanMitochondria
        }

        public AminoAcids()
        {
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

            // converts single letter amino acid ambiguity codes to three
            // letter abbreviations
            _singleToThreeAminoAcids = new Dictionary<char, string>
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
                {'J', "Xle"}
            };
        }

        /// <summary>
        /// returns the peptide sequence until the first terminal amino acid
        /// </summary>
        private static string AddAnyAminoAcid(string aminoAcids)
        {
            return aminoAcids=="*" ? aminoAcids : aminoAcids + 'X';
        }

        /// <summary>O
        /// sets the amino acids given the reference and variant codons
        /// </summary>
        public void Assign(TranscriptAnnotation transcriptAnnotation)
        {
            if (string.IsNullOrEmpty(transcriptAnnotation.ReferenceCodon) &&
                string.IsNullOrEmpty(transcriptAnnotation.AlternateCodon)) return;

            // sanity check: return null if either codon contains Ns
            if (transcriptAnnotation.ReferenceCodon.Contains("N") || 
                transcriptAnnotation.AlternateCodon.Contains("N")) return;

            transcriptAnnotation.ReferenceAminoAcids = TranslateBases(transcriptAnnotation.ReferenceCodon, false);
            transcriptAnnotation.AlternateAminoAcids = TranslateBases(transcriptAnnotation.AlternateCodon, false);
        }

        /// <summary>
        /// converts a DNA triplet to the appropriate amino acid abbreviation
        /// </summary>
        private string ConvertAminoAcidToAbbreviation(char aminoAcid)
        {
            string abbreviation;

            if (!_singleToThreeAminoAcids.TryGetValue(aminoAcid, out abbreviation))
            {
                throw new GeneralException($"Unable to convert the following string to an amino acid abbreviation: {aminoAcid}");
            }

            return abbreviation;
        }

        /// <summary>
        /// converts a DNA triplet to the appropriate amino acid abbreviation
        /// The default conversion is human chromosomes. The second parameter also allows the user to specify other codon conversions like mitochondria, etc.
        /// </summary>
        private char ConvertTripletToAminoAcid(string triplet)
        {
            var aminoAcid      = 'X';
            var foundAminoAcid = false;
            var upperTriplet = triplet.ToUpper();

            // check our exceptions first
            if (CodonConversionScheme == CodonConversion.HumanMitochondria &&
                _mitoDifferences.TryGetValue(upperTriplet, out aminoAcid))
            {
                foundAminoAcid = true;
            }

            // the default case
            if (!foundAminoAcid && _aminoAcidLookupTable.TryGetValue(upperTriplet, out aminoAcid))
            {
                foundAminoAcid = true;
            }

            return foundAminoAcid ? aminoAcid : 'X';
        }

        /// <summary>
        /// given a string of 1-letter amino acid ambiguity codes, this function
        /// returns a string of 3-letter amino acid abbreviations up until the first
        /// stop codon.
        /// </summary>
        public string GetAbbreviations(string aminoAcids)
        {
            if (string.IsNullOrEmpty(aminoAcids)) return "";
			var abbrevBuilder = new StringBuilder();

            foreach (var aminoAcid in aminoAcids)
            {
                abbrevBuilder.Append(ConvertAminoAcidToAbbreviation(aminoAcid));
            }

            return abbrevBuilder.ToString();
        }

        /// <summary>
        /// given a common amino acid prefix, remove the common amino acids (insertion)
        /// returns true if the alleles were modified, false otherwise
        /// </summary>
        internal static void RemovePrefixAndSuffix(HgvsProteinNomenclature.HgvsNotation hn)
        {
            // nothing to do if we have a pure insertion or deletion
            if (hn.ReferenceAminoAcids == null || hn.AlternateAminoAcids == null) return;

            // skip this if the amino acids are already the same
            if (hn.ReferenceAminoAcids == hn.AlternateAminoAcids) return;

            // calculate how many shared amino acids we have from the beginning of each amino acid
            var numSharedPrefixPos = 0;
            var isClipped         = false;
            var refLen             = hn.ReferenceAminoAcids.Length;
            var altLen             = hn.AlternateAminoAcids.Length;
            var minLength          = Math.Min(refLen, altLen);

            for (var pos = 0; pos < minLength; pos++, numSharedPrefixPos++, hn.Start++)
            {
                if (hn.ReferenceAminoAcids[pos] != hn.AlternateAminoAcids[pos]) break;
                refLen--;
                altLen--;
                isClipped = true;
            }

            // calculate how many shared amino acids we have from the end of each amino acid
            minLength = Math.Min(refLen, altLen);

            for (var pos = 0; pos < minLength; pos++, hn.End--)
            {
                var refPos = hn.ReferenceAminoAcids.Length - pos - 1;
                var altPos = hn.AlternateAminoAcids.Length - pos - 1;
                if (hn.ReferenceAminoAcids[refPos] != hn.AlternateAminoAcids[altPos]) break;
                refLen--;
                altLen--;
                isClipped = true;
            }

            // clip the amino acid alleles
            if (isClipped)
            {
                hn.SetReferenceAminoAcids(refLen == 0 ? null : hn.ReferenceAminoAcids.Substring(numSharedPrefixPos, refLen));
                hn.SetAlternateAminoAcids(altLen == 0 ? null : hn.AlternateAminoAcids.Substring(numSharedPrefixPos, altLen));
            }
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

            // sanity check: make sure the length is a multiple of 3
            var nonTriplet = !forceNonTriplet && numAminoAcids * 3 != bases.Length;

            // special case: single amino acid
            string aminoAcidString;
            if (numAminoAcids == 1)
            {
                aminoAcidString =
                    ConvertTripletToAminoAcid(bases.Substring(0, 3 * numAminoAcids))
                        .ToString();
                return nonTriplet ? AddAnyAminoAcid(aminoAcidString) : aminoAcidString;
            }

            // multiple amino acid case
            var aminoAcids = new char[numAminoAcids];
            for (var i = 0; i < numAminoAcids; i++)
            {
                aminoAcids[i] = ConvertTripletToAminoAcid(bases.Substring(i * 3, 3));
            }

            aminoAcidString = new string(aminoAcids);
            return nonTriplet ? AddAnyAminoAcid(aminoAcidString) : aminoAcidString;
        }

		internal static void Rotate3Prime(HgvsProteinNomenclature.HgvsNotation hn, string peptides)
		{
			if (hn.Type != ProteinChange.Deletion
				&& hn.Type != ProteinChange.Duplication
				&& hn.Type != ProteinChange.Insertion
				)
				return;

			// for insertion, the reference bases will be empty string. The shift should happen on the alternate allele
			var rotatingPeptides = hn.Type == ProteinChange.Insertion ? hn.AlternateAminoAcids : hn.ReferenceAminoAcids;
			var numBases = rotatingPeptides.Length;

			var downstreamPeptides = peptides.Length>= hn.End ? peptides.Substring(hn.End): null;

			if (downstreamPeptides == null) return;

            var combinedSequence = rotatingPeptides + downstreamPeptides;

			int shiftStart, shiftEnd;
			var hasShifted = false;
			for (shiftStart = 0, shiftEnd = numBases; shiftEnd < combinedSequence.Length ; shiftStart++, shiftEnd++)
			{
				if (combinedSequence[shiftStart] != combinedSequence[shiftEnd]) break;
				hn.Start++;
				hasShifted = true;

			}
			if (hasShifted) rotatingPeptides = combinedSequence.Substring(shiftStart, numBases);

			if (hn.Type == ProteinChange.Insertion)
				hn.AlternateAminoAcids = rotatingPeptides;
			else hn.ReferenceAminoAcids = rotatingPeptides;

			hn.End = hn.Type == ProteinChange.Insertion ? hn.Start-1 : hn.Start + numBases - 1;

			if (hn.Type != ProteinChange.Insertion || !hasShifted) return;

			var newUpstreamSeq = combinedSequence.Substring(0, shiftStart);

			if (newUpstreamSeq.EndsWith(rotatingPeptides))
			{
				hn.Type = ProteinChange.Duplication;
				hn.End = hn.Start + numBases - 1;

				hn.ReferenceAminoAcids= hn.AlternateAminoAcids;
			}
		}
	}
}