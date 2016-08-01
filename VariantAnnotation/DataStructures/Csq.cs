using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;

namespace VariantAnnotation.DataStructures
{
    public class CsqCommon
    {
        #region members

        public const string TranscriptFeatureType = "Transcript";
        public const string RegulatoryFeatureType = "RegulatoryFeature";

        private const string CsqInfoLine = "##INFO=<ID=CSQ,Number=.,Type=String,Description=\"Consequence type as predicted by VEP. Format: Allele|Gene|Feature|Feature_type|Consequence|cDNA_position|CDS_position|Protein_position|Amino_acids|Codons|Existing_variation|DISTANCE|MOTIF_NAME|MOTIF_POS|HIGH_INF_POS|MOTIF_SCORE_CHANGE|CELL_TYPE|HGNC|CCDS|CANONICAL|PolyPhen|SIFT|ENSP|HGVSc|HGVSp|DOMAINS|EXON|INTRON\">";

        // define the column names
        //
        // The annoying bit about CSQ fields is that the order changes depending
        // on which parameters have been passed to VEP. As a result, we need to
        // store the indices of each field given the vcf info entry for the CSQ
        // field.
        private static int _alleleIndex                  = -1;
        private static int _aminoAcidsIndex              = -1;
        private static int _bioTypeIndex                 = -1;
        private static int _canonicalIndex               = -1;
        private static int _ccdsIndex                    = -1;
        private static int _cdsPositionIndex             = -1;
        private static int _cellTypeIndex                = -1;
        private static int _codingDnaPositionIndex       = -1;
        private static int _codonsIndex                  = -1;
        private static int _consequenceIndex             = -1;
        private static int _distanceIndex                = -1;
        private static int _domainsIndex                 = -1;
        private static int _ensemblProteinIdIndex        = -1;
        private static int _existingVariationIndex       = -1;
        private static int _exonIndex                    = -1;
        private static int _featureIndex                 = -1;
        private static int _featureTypeIndex             = -1;
        private static int _geneIndex                    = -1;
        private static int _hgncIdIndex                  = -1;
        private static int _hgvsCodingSequenceNameIndex  = -1;
        private static int _hgvsProteinSequenceNameIndex = -1;
        private static int _highInfPosIndex              = -1;
        private static int _impactIndex                  = -1;
        private static int _intronIndex                  = -1;
        private static int _motifNameIndex               = -1;
        private static int _motifPosIndex                = -1;
        private static int _motifScoreChangeIndex        = -1;
        private static int _polyPhenIndex                = -1;
        private static int _proteinPositionIndex         = -1;
        private static int _siftIndex                    = -1;
        private static int _strandIndex                  = -1;
        private static int _symbolIndex                  = -1; // also maps to HGNC_ID
        private static int _symbolSourceIndex            = -1;

        private const string AlleleTag                  = "Allele";
        private const string AminoAcidsTag              = "Amino_acids";
        private const string BioTypeTag                 = "BIOTYPE";
        private const string CanonicalTag               = "CANONICAL";
        private const string CcdsTag                    = "CCDS";
        private const string CdsPositionTag             = "CDS_position";
        private const string CellTypeTag                = "CELL_TYPE";
        private const string CodingDnaPositionTag       = "cDNA_position";
        private const string CodonsTag                  = "Codons";
        private const string ConsequenceTag             = "Consequence";
        private const string DistanceTag                = "DISTANCE";
        private const string DomainsTag                 = "DOMAINS";
        private const string EnsemblProteinIdTag        = "ENSP";
        private const string ExistingVariationTag       = "Existing_variation";
        private const string ExonTag                    = "EXON";
        private const string FeatureTag                 = "Feature";
        private const string FeatureTypeTag             = "Feature_type";
        private const string GeneTag                    = "Gene";
        private const string HgncIdTag                  = "HGNC_ID";
        private const string HgncTag                    = "HGNC";
        private const string HgvsCodingSequenceNameTag  = "HGVSc";
        private const string HgvsOffsetTag              = "HGVS_OFFSET";
        private const string HgvsProteinSequenceNameTag = "HGVSp";
        private const string HighInfPosTag              = "HIGH_INF_POS";
        private const string ImpactTag                  = "IMPACT";
        private const string IntronTag                  = "INTRON";
        private const string MotifNameTag               = "MOTIF_NAME";
        private const string MotifPosTag                = "MOTIF_POS";
        private const string MotifScoreChangeTag        = "MOTIF_SCORE_CHANGE";
        private const string PolyPhenTag                = "PolyPhen";
        private const string ProteinPositionTag         = "Protein_position";
        private const string SiftTag                    = "SIFT";
        private const string StrandTag                  = "STRAND";
        private const string SymbolSourceTag            = "SYMBOL_SOURCE";
        private const string SymbolTag                  = "SYMBOL";

        #endregion

        /// <summary>
        /// static constructor: set the default CSQ field order
        /// </summary>
        static CsqCommon()
        {
            SetCsqFieldOrder(CsqInfoLine);
        }

        /// <summary>
        /// returns the current known CSQ tags
        /// </summary>
        private static HashSet<string> GetKnownCsqTags()
        {
            return new HashSet<string>
            {
                AlleleTag,
                AminoAcidsTag,
                BioTypeTag,
                CanonicalTag,
                CcdsTag,
                CdsPositionTag,
                CellTypeTag,
                CodingDnaPositionTag,
                CodonsTag,
                ConsequenceTag,
                DistanceTag,
                DomainsTag,
                EnsemblProteinIdTag,
                ExistingVariationTag,
                ExonTag,
                FeatureTag,
                FeatureTypeTag,
                GeneTag,
                HgncIdTag,
                HgncTag,
                HgvsCodingSequenceNameTag,
                HgvsOffsetTag,
                HgvsProteinSequenceNameTag,
                HighInfPosTag,
                ImpactTag,
                IntronTag,
                MotifNameTag,
                MotifPosTag,
                MotifScoreChangeTag,
                PolyPhenTag,
                ProteinPositionTag,
                SiftTag,
                StrandTag,
                SymbolSourceTag,
                SymbolTag
            };
        }

        /// <summary>
        /// creates a dictionary of tag name keys to column indexes
        /// </summary>
        private static Dictionary<string, int> GetTagToColumnIndex(string[] cols, HashSet<string> knownTags)
        {
            var dict = new Dictionary<string, int>();

            for (int colIndex = 0; colIndex < cols.Length; colIndex++)
            {
                var tagName = cols[colIndex];

                if (!knownTags.Contains(tagName))
                {
                    throw new GeneralException($"Found unknown CSQ tag: {tagName}");
                }

                dict[tagName] = colIndex;
            }

            return dict;
        }

        /// <summary>
        /// sets the index for the associated tag if present in the dictionary
        /// </summary>
        private static void SetIndex(ref int index, string tag, Dictionary<string, int> dict)
        {
            int colIndex;
            if (dict.TryGetValue(tag, out colIndex)) index = colIndex;
        }

        /// <summary>
        /// clears all of the public static integers in this class
        /// </summary>
        private static void ClearIndices()
        {
            var type   = typeof(CsqCommon);
            var fields = type.GetTypeInfo().GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                // ignore constants
                if (field.IsLiteral) continue;

                var temp = field.GetValue(null);
                if (temp is int) field.SetValue(temp, -1);
            }
        }

        /// <summary>
        /// sets the column indices according to the order found in the vcf info
        /// line for CSQ.
        /// </summary>
        private static void SetCsqFieldOrder(string csqVcfInfoLine)
        {
            ClearIndices();

            var cols             = ExtractCsqColumnOrder(csqVcfInfoLine);
            var knownTags        = GetKnownCsqTags();
            var tagToColumnIndex = GetTagToColumnIndex(cols, knownTags);

            SetIndex(ref _alleleIndex, AlleleTag, tagToColumnIndex);
            SetIndex(ref _aminoAcidsIndex, AminoAcidsTag, tagToColumnIndex);
            SetIndex(ref _bioTypeIndex, BioTypeTag, tagToColumnIndex);
            SetIndex(ref _canonicalIndex, CanonicalTag, tagToColumnIndex);
            SetIndex(ref _ccdsIndex, CcdsTag, tagToColumnIndex);
            SetIndex(ref _cdsPositionIndex, CdsPositionTag, tagToColumnIndex);
            SetIndex(ref _cellTypeIndex, CellTypeTag, tagToColumnIndex);
            SetIndex(ref _codingDnaPositionIndex, CodingDnaPositionTag, tagToColumnIndex);
            SetIndex(ref _codonsIndex, CodonsTag, tagToColumnIndex);
            SetIndex(ref _consequenceIndex, ConsequenceTag, tagToColumnIndex);
            SetIndex(ref _distanceIndex, DistanceTag, tagToColumnIndex);
            SetIndex(ref _domainsIndex, DomainsTag, tagToColumnIndex);
            SetIndex(ref _ensemblProteinIdIndex, EnsemblProteinIdTag, tagToColumnIndex);
            SetIndex(ref _existingVariationIndex, ExistingVariationTag, tagToColumnIndex);
            SetIndex(ref _exonIndex, ExonTag, tagToColumnIndex);
            SetIndex(ref _featureIndex, FeatureTag, tagToColumnIndex);
            SetIndex(ref _featureTypeIndex, FeatureTypeTag, tagToColumnIndex);
            SetIndex(ref _geneIndex, GeneTag, tagToColumnIndex);
            SetIndex(ref _hgncIdIndex, HgncIdTag, tagToColumnIndex);
            SetIndex(ref _symbolIndex, HgncTag, tagToColumnIndex);
            SetIndex(ref _hgvsCodingSequenceNameIndex, HgvsCodingSequenceNameTag, tagToColumnIndex);
            SetIndex(ref _hgvsProteinSequenceNameIndex, HgvsProteinSequenceNameTag, tagToColumnIndex);
            SetIndex(ref _highInfPosIndex, HighInfPosTag, tagToColumnIndex);
            SetIndex(ref _impactIndex, ImpactTag, tagToColumnIndex);
            SetIndex(ref _intronIndex, IntronTag, tagToColumnIndex);
            SetIndex(ref _motifNameIndex, MotifNameTag, tagToColumnIndex);
            SetIndex(ref _motifPosIndex, MotifPosTag, tagToColumnIndex);
            SetIndex(ref _motifScoreChangeIndex, MotifScoreChangeTag, tagToColumnIndex);
            SetIndex(ref _polyPhenIndex, PolyPhenTag, tagToColumnIndex);
            SetIndex(ref _proteinPositionIndex, ProteinPositionTag, tagToColumnIndex);
            SetIndex(ref _siftIndex, SiftTag, tagToColumnIndex);
            SetIndex(ref _strandIndex, StrandTag, tagToColumnIndex);
            SetIndex(ref _symbolSourceIndex, SymbolSourceTag, tagToColumnIndex);
            SetIndex(ref _symbolIndex, SymbolTag, tagToColumnIndex);
        }

        /// <summary>
        /// returns an array of format columns from the vcf header line for CSQ entries
        /// </summary>
        private static string[] ExtractCsqColumnOrder(string csqVcfInfoLine)
        {
            const string formatTag = "Format: ";
            const string endTag    = "\">";

            int formatIndex = csqVcfInfoLine.LastIndexOf(formatTag, StringComparison.Ordinal);

            if (formatIndex == -1)
            {
                throw new GeneralException(
                    $"Could not find the format tag ({formatTag}) in the following string: {csqVcfInfoLine}");
            }

            string csqFieldOrder = csqVcfInfoLine.Substring(formatIndex + formatTag.Length);
            if (csqFieldOrder.EndsWith(endTag)) csqFieldOrder = csqFieldOrder.Substring(0, csqFieldOrder.Length - endTag.Length);

            return csqFieldOrder.Split('|');
        }
    }

    /// <summary>
    /// The annoying bit about CSQ fields is that the order changes depending on which
    /// parameters have been passed to VEP. As a result, we need to keep all of the key
    /// value pairs in a dictionary.
    /// </summary>
    public class CsqEntry
    {
        #region members

        public string Allele;
        public string Consequence;
        public string Feature;
        public string FeatureType;
        public string Symbol;
        public string Canonical;

        #endregion

        /// <summary>
        /// returns a hashcode that can be used to uniquely identify this CSQ tag.
        /// </summary>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            int hashCode = FowlerNollVoPrimeHash.ComputeHash(Allele, Feature);
            // ReSharper restore NonReadonlyFieldInGetHashCode
            return hashCode;
        }

        /// <summary>
        /// returns true if this entry has the same data as another entry
        /// </summary>
        // ReSharper disable once CSharpWarnings::CS0659
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            var other = (CsqEntry)obj;

            return string.Equals(Allele, other.Allele) && string.Equals(Feature, other.Feature);
        }

        /// <summary>
        /// returns a string representation of our CSQ entry
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            AddString(sb, "Allele", Allele);
            AddString(sb, "Canonical", Canonical);
            AddString(sb, "Consequence", Consequence);
            AddString(sb, "Feature", Feature);
            AddString(sb, "FeatureType", FeatureType);
            AddString(sb, "Symbol", Symbol);
			
            return sb.ToString();
        }

        /// <summary>
        /// adds a string to the string builder if it's not empty or null
        /// </summary>
        private static void AddString(StringBuilder sb, string description, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            const int fieldWidth = 28;
            int numSpaces = fieldWidth - description.Length - 1;
            sb.AppendFormat("{0}:{1}{2}\n", description, new string(' ', numSpaces), value);
        }
    }
}
