using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VariantAnnotation.Algorithms;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.DataStructures
{
    public sealed class CsqCommon
    {
        #region members

        private const string CsqInfoTag            = "CSQ=";
        private const int NumColumns               = 28;
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
        /// returns the CSQ value if the colIndex is non-negative, returns null otherwise
        /// </summary>
        private static string GetCsqValue(string[] cols, int colIndex)
        {
            if (colIndex == -1 || colIndex >= cols.Length) return null;
            return cols[colIndex];
        }

        /// <summary>
        /// populates a list containing the CSQ entries found in the info field
        /// </summary>
        public static void GetCsqEntries(string vcfInfoField, List<CsqEntry> csqEntries)
        {
            csqEntries.Clear();

            // grab the CSQ field
            var cols   = vcfInfoField.Split(';');
            var foundCsq   = false;
            var csqField = string.Empty;

            foreach (var col in cols)
            {
                if (col.StartsWith(CsqInfoTag))
                {
                    foundCsq = true;
                    csqField = col;
                }
            }

            if (!foundCsq) return;

            var csqEntryCols = csqField.Substring(CsqInfoTag.Length).Split(',');

            // populate each CSQ entry
            foreach (var s in csqEntryCols)
            {
                var csqCols = s.Split('|');

                // sanity check: check the column count
                if (csqCols.Length < NumColumns)
                {
                    throw new GeneralException($"Expected at least {NumColumns} columns in each CSQ field entry, but {csqCols.Length} columns were found.");
                }

                var newEntry = new CsqEntry
                {
                    Allele                   = GetCsqValue(csqCols, _alleleIndex),
                    AminoAcids               = GetCsqValue(csqCols, _aminoAcidsIndex),
                    BioType                  = GetCsqValue(csqCols, _bioTypeIndex),
                    Canonical                = GetCsqValue(csqCols, _canonicalIndex),
                    Ccds                     = GetCsqValue(csqCols, _ccdsIndex),
                    CdsPosition              = GetCsqValue(csqCols, _cdsPositionIndex),
                    CellType                 = GetCsqValue(csqCols, _cellTypeIndex),
                    Codons                   = GetCsqValue(csqCols, _codonsIndex),
                    ComplementaryDnaPosition = GetCsqValue(csqCols, _codingDnaPositionIndex),
                    Consequence              = GetCsqValue(csqCols, _consequenceIndex),
                    Distance                 = GetCsqValue(csqCols, _distanceIndex),
                    Domains                  = GetCsqValue(csqCols, _domainsIndex),
                    EnsemblProteinId         = GetCsqValue(csqCols, _ensemblProteinIdIndex),
                    ExistingVariation        = GetCsqValue(csqCols, _existingVariationIndex),
                    Exon                     = GetCsqValue(csqCols, _exonIndex),
                    Feature                  = GetCsqValue(csqCols, _featureIndex),
                    FeatureType              = GetCsqValue(csqCols, _featureTypeIndex),
                    Gene                     = GetCsqValue(csqCols, _geneIndex),
                    HgncId                   = GetCsqValue(csqCols, _hgncIdIndex),
                    HgvsCodingSequenceName   = GetCsqValue(csqCols, _hgvsCodingSequenceNameIndex),
                    HgvsProteinSequenceName  = GetCsqValue(csqCols, _hgvsProteinSequenceNameIndex),
                    HighInfPos               = GetCsqValue(csqCols, _highInfPosIndex),
                    Intron                   = GetCsqValue(csqCols, _intronIndex),
                    MotifName                = GetCsqValue(csqCols, _motifNameIndex),
                    MotifPos                 = GetCsqValue(csqCols, _motifPosIndex),
                    MotifScoreChange         = GetCsqValue(csqCols, _motifScoreChangeIndex),
                    PolyPhen                 = GetCsqValue(csqCols, _polyPhenIndex),
                    ProteinPosition          = GetCsqValue(csqCols, _proteinPositionIndex),
                    Sift                     = GetCsqValue(csqCols, _siftIndex),
                    Strand                   = GetCsqValue(csqCols, _strandIndex),
                    Symbol                   = GetCsqValue(csqCols, _symbolIndex),
                    SymbolSource             = GetCsqValue(csqCols, _symbolSourceIndex)
                };

                csqEntries.Add(newEntry);
            }
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

            for (var colIndex = 0; colIndex < cols.Length; colIndex++)
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
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

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

            var formatIndex = csqVcfInfoLine.LastIndexOf(formatTag, StringComparison.Ordinal);

            if (formatIndex == -1)
            {
                throw new GeneralException(
                    $"Could not find the format tag ({formatTag}) in the following string: {csqVcfInfoLine}");
            }

            var csqFieldOrder = csqVcfInfoLine.Substring(formatIndex + formatTag.Length);
            if (csqFieldOrder.EndsWith(endTag)) csqFieldOrder = csqFieldOrder.Substring(0, csqFieldOrder.Length - endTag.Length);

            return csqFieldOrder.Split('|');
        }

        /// <summary>
        /// returns the csq entry specified by the feature ID and the allele. Returns null
        /// if the entry could not be found.
        /// </summary>
        public static CsqEntry GetEntry(List<CsqEntry> csqEntries, string featureId, string allele)
        {
            return csqEntries.FirstOrDefault(csqEntry => csqEntry.Feature == featureId && csqEntry.Allele == allele);
        }
    }

    /// <summary>
    /// The annoying bit about CSQ fields is that the order changes depending on which
    /// parameters have been passed to VEP. As a result, we need to keep all of the key
    /// value pairs in a dictionary.
    /// </summary>
    public sealed class CsqEntry
    {
        #region members

        public string Allele;
        public string AminoAcids;
        public string BioType;
        public string Ccds;
        public string CdsPosition;
        public string Codons;
        public string ComplementaryDnaPosition;
        public string Consequence;
        public string Distance;
        public string Domains;
        public string EnsemblProteinId;
        public string ExistingVariation;
        public string Exon;
        public string Feature;
        public string FeatureType;
        public string Gene;
        public string HgncId;
        public string HgvsCodingSequenceName;
        public string HgvsProteinSequenceName;
        public string HighInfPos;
        public string Intron;
        public string MotifName;
        public string MotifPos;
        public string MotifScoreChange;
        public string PolyPhen;
        public string ProteinPosition;
        public string Sift;
        public string Strand;
        public string Symbol;
        public string SymbolSource;

        // For human, the canonical transcript for a gene is set according to the following
        // hierarchy: 1. Longest CCDS translation with no stop codons. 2. If no (1), choose
        // the longest Ensembl/Havana merged translation with no stop codons. 3. If no (2),
        // choose the longest translation with no stop codons. 4. If no translation, choose
        // the longest non-protein-coding transcript.
        public string Canonical;

        public string CellType;

        #endregion

        /// <summary>
        /// returns a hashcode that can be used to uniquely identify this CSQ tag.
        /// </summary>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            var hashCode = FowlerNollVoPrimeHash.ComputeHash(Allele, Feature);
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
        /// TODO: CC 29
        public override string ToString()
        {
            var sb = new StringBuilder();

            AddPresentString(sb, "Allele", Allele);
            AddPresentString(sb, "AminoAcids", AminoAcids);
            AddPresentString(sb, "BioType", BioType);
            AddPresentString(sb, "Canonical", Canonical);
            AddPresentString(sb, "Ccds", Ccds);
            AddPresentString(sb, "CdsPosition", CdsPosition);
            AddPresentString(sb, "CellType", CellType);
            AddPresentString(sb, "Codons", Codons);
            AddPresentString(sb, "ComplementaryDnaPosition", ComplementaryDnaPosition);
            AddPresentString(sb, "Consequence", Consequence);
            AddPresentString(sb, "Distance", Distance);
            AddPresentString(sb, "Domains", Domains);
            AddPresentString(sb, "EnsemblProteinId", EnsemblProteinId);
            AddPresentString(sb, "ExistingVariation", ExistingVariation);
            AddPresentString(sb, "Exon", Exon);
            AddPresentString(sb, "Feature", Feature);
            AddPresentString(sb, "FeatureType", FeatureType);
            AddPresentString(sb, "Gene", Gene);
            AddPresentString(sb, "HgncId", HgncId);
            AddPresentString(sb, "HgvsCodingSequenceName", HgvsCodingSequenceName);
            AddPresentString(sb, "HgvsProteinSequenceName", HgvsProteinSequenceName);
            AddPresentString(sb, "HighInfPos", HighInfPos);
            AddPresentString(sb, "Intron", Intron);
            AddPresentString(sb, "MotifName", MotifName);
            AddPresentString(sb, "MotifPos", MotifPos);
            AddPresentString(sb, "MotifScoreChange", MotifScoreChange);
            AddPresentString(sb, "PolyPhen", PolyPhen);
            AddPresentString(sb, "ProteinPosition", ProteinPosition);
            AddPresentString(sb, "Sift", Sift);
            AddPresentString(sb, "Strand", Strand);
            AddPresentString(sb, "Symbol", Symbol);
            AddPresentString(sb, "SymbolSource", SymbolSource);
			
            return sb.ToString();
        }

        /// <summary>
        /// adds a string to the string builder if it's not empty or null
        /// </summary>
        private static void AddPresentString(StringBuilder sb, string description, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            const int fieldWidth = 28;
            var numSpaces = fieldWidth - description.Length - 1;
            sb.AppendFormat("{0}:{1}{2}\n", description, new string(' ', numSpaces), value);
        }
    }
}
