using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public class SupplementaryAnnotation
    {
        #region members

        public int ReferencePosition { get; }
		public string RefSeqName { get; set; }
		public string RefAllele { private get; set; }
		public double RefAlleleFreq = double.MinValue;
		public const int PrecisionConst = 1000000000;//want to have 9 digit precision for floats
                                                     // positional
        public string GlobalMinorAllele { get; internal set; }
        public string GlobalMinorAlleleFrequency { get; internal set; }
        public string GlobalMajorAllele { get; internal set; }
        internal string GlobalMajorAlleleFrequency { get; set; }

        private double TotalMinorAlleleFreq { get; set; }

        public bool IsRefMinorAllele { get; set; }
        public const double RmaFreqThreshold = 0.95;

        public Dictionary<string, AlleleSpecificAnnotation> AlleleSpecificAnnotations { get; }
        public List<CosmicItem> CosmicItems { get; }
        public List<ClinVarItem> ClinVarItems { get; }
	    

	    public List<CustomItem> CustomItems;

        // MD5 support
        private static readonly StringBuilder MD5Builder = new StringBuilder();
        private static readonly MD5 MD5Hash = MD5.Create();

        public static long TotalAlleleSpecificEntryCount;
        public static long ConflictingAlleleSpecificEntryCount;

        #endregion

        
        public static string ConvertMixedFormatString(string s)
        {
            if (s == null) return null;

            // no hex characters to convert
            if (!s.Contains(@"\x")) return s;

            var sb = new StringBuilder();

            for (int i = 0; i < s.Length - 1; i++)
            {
                if (s[i] == '\\' && s[i + 1] == 'x')
                {
                    var hexString = s.Substring(i + 2, 2);
                    var value = Convert.ToInt32(hexString, 16);
                    sb.Append(char.ConvertFromUtf32(value));
                    i += 3;
                }
                else sb.Append(s[i]);
            }

            // the last char has to be added
            sb.Append(s[s.Length - 1]);
            return sb.ToString();
        }

        public void AddCosmic(CosmicItem other)
        {
            var index = CosmicItems.IndexOf(other);

            if (index == -1)
            {
                CosmicItems.Add(other);
                return;
            }

            //get the existing cosmic item in dictionary
            if (CosmicItems[index].Studies == null)
            {
                CosmicItems[index].Studies = other.Studies;
                return;
            }

            foreach (var study in other.Studies)
            {
                CosmicItems[index].Studies.Add(study);
            }

        }

        
        // allele-specific
        public class AlleleSpecificAnnotation
        {
            #region members

            public List<long> DbSnp = new List<long>();
            public string AncestralAllele;
            public bool HasMultipleOneKgenEntries;
            public bool HasMultipleEvsEntries;
            public bool HasMultipleExacEntries;

            public int ExacCoverage;

            public int? ExacAllAn;
            public int? ExacAfrAn;
            public int? ExacAmrAn;
            public int? ExacEasAn;
            public int? ExacFinAn;
            public int? ExacNfeAn;
            public int? ExacOthAn;
            public int? ExacSasAn;

            public int? ExacAllAc;
            public int? ExacAfrAc;
            public int? ExacAmrAc;
            public int? ExacEasAc;
            public int? ExacFinAc;
            public int? ExacNfeAc;
            public int? ExacOthAc;
            public int? ExacSasAc;

            public string NumEvsSamples;
            public string EvsCoverage;
            public string EvsAfr;
            public string EvsAll;
            public string EvsEur;

            public int? OneKgAllAn;
            public int? OneKgAfrAn;
            public int? OneKgAmrAn;
            public int? OneKgEasAn;
            public int? OneKgEurAn;
            public int? OneKgSasAn;

            public int? OneKgAllAc;
            public int? OneKgAfrAc;
            public int? OneKgAmrAc;
            public int? OneKgEasAc;
            public int? OneKgEurAc;
            public int? OneKgSasAc;
	        public double AltAlleleFreq = double.MinValue;

            #endregion

	        public void MergeExacAnnotations(AlleleSpecificAnnotation otherAsa)
            {
                // the new strategy is to remove an entry that has conflicting frequency information

                if (otherAsa.ExacAllAn == null || otherAsa.ExacAllAn.Value == 0)
                    return;

                if (ExacAllAn == null || ExacAllAn.Value == 0)
                {
                    ExacCoverage = otherAsa.ExacCoverage;

                    ExacAllAn = otherAsa.ExacAllAn;
                    ExacAfrAn = otherAsa.ExacAfrAn;
                    ExacAmrAn = otherAsa.ExacAmrAn;
                    ExacEasAn = otherAsa.ExacEasAn;
                    ExacFinAn = otherAsa.ExacFinAn;
                    ExacNfeAn = otherAsa.ExacNfeAn;
                    ExacOthAn = otherAsa.ExacOthAn;
                    ExacSasAn = otherAsa.ExacSasAn;

                    ExacAllAc = otherAsa.ExacAllAc;
                    ExacAfrAc = otherAsa.ExacAfrAc;
                    ExacAmrAc = otherAsa.ExacAmrAc;
                    ExacEasAc = otherAsa.ExacEasAc;
                    ExacFinAc = otherAsa.ExacFinAc;
                    ExacNfeAc = otherAsa.ExacNfeAc;
                    ExacOthAc = otherAsa.ExacOthAc;
                    ExacSasAc = otherAsa.ExacSasAc;
                }
                else
                {
                    // this is a conflict
                    HasMultipleExacEntries = true;
                    ClearExacData();
                }


            }

            public void ClearExacData()
            {
                ExacCoverage = 0;

                ExacAllAn = null;
                ExacAfrAn = null;
                ExacAmrAn = null;
                ExacEasAn = null;
                ExacFinAn = null;
                ExacNfeAn = null;
                ExacOthAn = null;
                ExacSasAn = null;

                ExacAllAc = null;
                ExacAfrAc = null;
                ExacAmrAc = null;
                ExacEasAc = null;
                ExacFinAc = null;
                ExacNfeAc = null;
                ExacOthAc = null;
                ExacSasAc = null;

            }

            public void MergeOneKGenAnnotations(AlleleSpecificAnnotation otherAsa)
            {
                // the new strategy is to remove an entry that has conflicting frequency information

                if (otherAsa.OneKgAllAc == null)
                    return;
                if (OneKgAllAc == null)
                {
                    OneKgAllAc = otherAsa.OneKgAllAc;
                    AncestralAllele = otherAsa.AncestralAllele;

                    OneKgAllAn = otherAsa.OneKgAllAn;
                    OneKgAfrAn = otherAsa.OneKgAfrAn;
                    OneKgAmrAn = otherAsa.OneKgAmrAn;
                    OneKgEurAn = otherAsa.OneKgEurAn;
                    OneKgEasAn = otherAsa.OneKgEasAn;
                    OneKgSasAn = otherAsa.OneKgSasAn;

                    OneKgAllAc = otherAsa.OneKgAllAc;
                    OneKgAfrAc = otherAsa.OneKgAfrAc;
                    OneKgAmrAc = otherAsa.OneKgAmrAc;
                    OneKgEurAc = otherAsa.OneKgEurAc;
                    OneKgEasAc = otherAsa.OneKgEasAc;
                    OneKgSasAc = otherAsa.OneKgSasAc;

                }
                else
                {
                    HasMultipleOneKgenEntries = true;
                    ClearOnekGenFields();
                }

            }

            public void ClearOnekGenFields()
            {
                // clear all fields
                AncestralAllele = null;

                OneKgAllAn = null;
                OneKgAfrAn = null;
                OneKgAmrAn = null;
                OneKgEurAn = null;
                OneKgEasAn = null;
                OneKgSasAn = null;

                OneKgAllAc = null;
                OneKgAfrAc = null;
                OneKgAmrAc = null;
                OneKgEurAc = null;
                OneKgEasAc = null;
                OneKgSasAc = null;

            }

            public void MergeEvsAnnotations(AlleleSpecificAnnotation otherAsa)
            {
                // from now on, we are discarding entries that have conflicting data

                if (otherAsa.NumEvsSamples == null) return;

                if (NumEvsSamples == null)
                {
                    EvsAll = otherAsa.EvsAll;
                    EvsAfr = otherAsa.EvsAfr;
                    EvsEur = otherAsa.EvsEur;

                    EvsCoverage = otherAsa.EvsCoverage;
                    NumEvsSamples = otherAsa.NumEvsSamples;
                }
                else
                {
                    HasMultipleEvsEntries = true;
                    ClearEvsData();
                }

            }

            public void ClearEvsData()
            {
                EvsAll = null;
                EvsAfr = null;
                EvsEur = null;

                EvsCoverage = null;
                NumEvsSamples = null;
            }

            public void CombineAnnotation(AbstractAnnotationRecord annotationRecord)
            {
                // check string list values: the default behavior is to aggregate values from the supplied annotation record
                if (annotationRecord == null) return;

                switch (annotationRecord.DataType)
                {
                    case AnnotationRecordDataType.Int64List:
                        var int64ListRecord = annotationRecord as Int64ListRecord;
                        CombineInt64ListRecords(int64ListRecord);
                        break;

                    case AnnotationRecordDataType.Int32:
                        var intRecord = annotationRecord as Int32Record;
                        SetIntRecords(intRecord);
                        break;

                    case AnnotationRecordDataType.String:
                        var stringRecord = annotationRecord as StringRecord;
                        SetStringRecords(stringRecord);
                        break;
                    default:
                        // unsupported data type
                        throw new GeneralException($"Encountered an unknown data type: {annotationRecord.DataType}");
                }


            }

            private void SetStringRecords(StringRecord stringRecord)
            {
                // sanity check: make sure we successfully recast
                if (stringRecord == null)
                    throw new InvalidCastException("Unable to cast the AbstractAnnotationRecord as a StringRecord");

                switch ((AlleleSpecificId)stringRecord.Id)
                {
                    case AlleleSpecificId.AncestralAllele:
                        AncestralAllele = stringRecord.Value;
                        break;

                    default:
                        throw new GeneralException("Unable to convert byte to an allele-specific ID.");
                }
            }

            private void SetIntRecords(Int32Record intRecord)
            {
                // sanity check: make sure we successfully recast
                if (intRecord == null)
                    throw new InvalidCastException("Unable to cast the AbstractAnnotationRecord as a int32Record");

                switch ((AlleleSpecificId)intRecord.Id)
                {
                    case AlleleSpecificId.EvsCoverage:
                        EvsCoverage = intRecord.Value.ToString(CultureInfo.InvariantCulture);
                        break;
                    case AlleleSpecificId.NumEvsSamples:
                        NumEvsSamples = intRecord.Value.ToString(CultureInfo.InvariantCulture);
                        break;
                    case AlleleSpecificId.EvsAfr:
                        EvsAfr = (intRecord.Value * 1.0 / PrecisionConst).ToString(CultureInfo.InvariantCulture);
                        break;
                    case AlleleSpecificId.EvsAll:
                        EvsAll = (intRecord.Value * 1.0 / PrecisionConst).ToString(CultureInfo.InvariantCulture);
                        break;
                    case AlleleSpecificId.EvsEur:
                        EvsEur = (intRecord.Value * 1.0 / PrecisionConst).ToString(CultureInfo.InvariantCulture);
                        break;
                    case AlleleSpecificId.ExacCoverage:
                        ExacCoverage = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacAllAn:
                        ExacAllAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacAfrAn:
                        ExacAfrAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacAmrAn:
                        ExacAmrAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacEasAn:
                        ExacEasAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacFinAn:
                        ExacFinAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacNfeAn:
                        ExacNfeAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacOthAn:
                        ExacOthAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacSasAn:
                        ExacSasAn = intRecord.Value;
                        break;

                    case AlleleSpecificId.ExacAllAc:
                        ExacAllAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacAfrAc:
                        ExacAfrAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacAmrAc:
                        ExacAmrAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacEasAc:
                        ExacEasAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacFinAc:
                        ExacFinAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacNfeAc:
                        ExacNfeAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacOthAc:
                        ExacOthAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.ExacSasAc:
                        ExacSasAc = intRecord.Value;
                        break;

                    case AlleleSpecificId.OneKgAllAn:
                        OneKgAllAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.OneKgAfrAn:
                        OneKgAfrAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.OneKgAmrAn:
                        OneKgAmrAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.OneKgEasAn:
                        OneKgEasAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.OneKgEurAn:
                        OneKgEurAn = intRecord.Value;
                        break;
                    case AlleleSpecificId.OneKgSasAn:
                        OneKgSasAn = intRecord.Value;
                        break;

                    case AlleleSpecificId.OneKgAllAc:
                        OneKgAllAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.OneKgAfrAc:
                        OneKgAfrAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.OneKgAmrAc:
                        OneKgAmrAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.OneKgEasAc:
                        OneKgEasAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.OneKgEurAc:
                        OneKgEurAc = intRecord.Value;
                        break;
                    case AlleleSpecificId.OneKgSasAc:
                        OneKgSasAc = intRecord.Value;
                        break;

                    default:
                        throw new GeneralException("int record: Unable to convert byte to an allele-specific ID.");
                }
            }

            private void CombineInt64ListRecords(Int64ListRecord int64ListRecord)
            {
                if (int64ListRecord == null)
                    throw new InvalidCastException("Unable to cast the AbstractAnnotationRecord as a int64ListRecord");

                switch ((AlleleSpecificId)int64ListRecord.Id)
                {
                    case AlleleSpecificId.DbSnp:
                        foreach (var dbSnpId in int64ListRecord.Values)
                        {
                            if (!DbSnp.Contains(dbSnpId))
                                DbSnp.Add(dbSnpId);
                        }

                        break;
                }
            }
        }

        // constructor
        public SupplementaryAnnotation(int referencePosition = 0)
        {
            ReferencePosition         = referencePosition;
            AlleleSpecificAnnotations = new Dictionary<string, AlleleSpecificAnnotation>();
            CosmicItems               = new List<CosmicItem>();
            ClinVarItems              = new List<ClinVarItem>();
            CustomItems               = new List<CustomItem>();
        }

        public static bool IsNucleotide(char c)
        {
            c = char.ToUpper(c);
            return c == 'A' || c == 'C' || c == 'G' || c == 'T' || c == 'N';
        }

        /// <summary>
        /// returns all of the positional records that are currently set
        /// </summary>
        internal List<AbstractAnnotationRecord> GetPositionalRecords()
        {
            var posRecords = new List<AbstractAnnotationRecord>();

            AddStringRecord((byte)PositionalId.GlobalMinorAllele, GlobalMinorAllele, posRecords);
            AddStringRecord((byte)PositionalId.GlobalMinorAlleleFrequency, GlobalMinorAlleleFrequency, posRecords);
            AddStringRecord((byte)PositionalId.GlobalMajorAllele, GlobalMajorAllele, posRecords);
            AddStringRecord((byte)PositionalId.GlobalMajorAlleleFrequency, GlobalMajorAlleleFrequency, posRecords);

            if (AnnotationLoader.Instance.GenomeAssembly != GenomeAssembly.GRCh38)
                AddBooleanRecord((byte)PositionalId.IsRefMinorAllele, IsRefMinorAllele, posRecords);

            return posRecords;
        }

        private static void AddStringRecord(byte id, string s, List<AbstractAnnotationRecord> posRecords)
        {
            if (!string.IsNullOrEmpty(s)) posRecords.Add(new StringRecord(id, s));
        }

        private static void AddInt64ListRecord(byte id, List<long> int64List, List<AbstractAnnotationRecord> posRecords)
        {
            if (int64List?.Count > 0) posRecords.Add(new Int64ListRecord(id, int64List));
        }

        private static void AddBooleanRecord(byte id, bool b, List<AbstractAnnotationRecord> posRecords)
        {
            if (b) posRecords.Add(new BooleanRecord(id, true));
        }

        private static void AddInt32Record(byte id, string s, List<AbstractAnnotationRecord> posRecords)
        {
            if (string.IsNullOrEmpty(s)) return;

            var i = Convert.ToInt32(s);
            posRecords.Add(new Int32Record(id, i));
        }

        private static void AddInt32Record(byte id, int? n, List<AbstractAnnotationRecord> posRecords)
        {
            if (n == null) return;
            posRecords.Add(new Int32Record(id, n.Value));
        }

        private static void AddDecimalRecord(byte id, string s, List<AbstractAnnotationRecord> posRecords)
        {
            if (string.IsNullOrEmpty(s)) return;

            var f = Convert.ToDouble(s);
            var i = Convert.ToInt32(f * PrecisionConst);
            posRecords.Add(new Int32Record(id, i));
        }

        /// <summary>
        /// returns all of the allele-specific records that are currently set and associated with the specified allele
        /// </summary>
        internal List<AbstractAnnotationRecord> GetAlleleSpecificRecords(AlleleSpecificAnnotation asa)
        {
            TotalAlleleSpecificEntryCount++;

            var asaRecords = new List<AbstractAnnotationRecord>();

            if (asa.HasMultipleEvsEntries)
                asa.ClearEvsData();
            if (asa.HasMultipleExacEntries)
                asa.ClearExacData();
            if (asa.HasMultipleOneKgenEntries)
            {
                asa.ClearOnekGenFields();
                ConflictingAlleleSpecificEntryCount++;
            }


            AddInt64ListRecord((byte)AlleleSpecificId.DbSnp, asa.DbSnp, asaRecords);
            AddStringRecord((byte)AlleleSpecificId.AncestralAllele, asa.AncestralAllele, asaRecords);

            AddInt32Record((byte)AlleleSpecificId.EvsCoverage, asa.EvsCoverage, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.NumEvsSamples, asa.NumEvsSamples, asaRecords);

            AddDecimalRecord((byte)AlleleSpecificId.EvsAfr, asa.EvsAfr, asaRecords);
            AddDecimalRecord((byte)AlleleSpecificId.EvsAll, asa.EvsAll, asaRecords);
            AddDecimalRecord((byte)AlleleSpecificId.EvsEur, asa.EvsEur, asaRecords);


            AddInt32Record((byte)AlleleSpecificId.ExacCoverage, asa.ExacCoverage, asaRecords);

            AddInt32Record((byte)AlleleSpecificId.ExacAllAn, asa.ExacAllAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacAfrAn, asa.ExacAfrAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacAmrAn, asa.ExacAmrAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacEasAn, asa.ExacEasAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacFinAn, asa.ExacFinAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacNfeAn, asa.ExacNfeAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacOthAn, asa.ExacOthAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacSasAn, asa.ExacSasAn, asaRecords);

            AddInt32Record((byte)AlleleSpecificId.ExacAllAc, asa.ExacAllAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacAfrAc, asa.ExacAfrAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacAmrAc, asa.ExacAmrAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacEasAc, asa.ExacEasAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacFinAc, asa.ExacFinAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacNfeAc, asa.ExacNfeAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacOthAc, asa.ExacOthAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.ExacSasAc, asa.ExacSasAc, asaRecords);


            AddInt32Record((byte)AlleleSpecificId.OneKgAllAn, asa.OneKgAllAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.OneKgAfrAn, asa.OneKgAfrAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.OneKgAmrAn, asa.OneKgAmrAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.OneKgEasAn, asa.OneKgEasAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.OneKgEurAn, asa.OneKgEurAn, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.OneKgSasAn, asa.OneKgSasAn, asaRecords);

            AddInt32Record((byte)AlleleSpecificId.OneKgAllAc, asa.OneKgAllAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.OneKgAfrAc, asa.OneKgAfrAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.OneKgAmrAc, asa.OneKgAmrAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.OneKgEasAc, asa.OneKgEasAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.OneKgEurAc, asa.OneKgEurAc, asaRecords);
            AddInt32Record((byte)AlleleSpecificId.OneKgSasAc, asa.OneKgSasAc, asaRecords);

            return asaRecords;
        }

        public void SetIsAlleleSpecific(string saAltAllele)
        {
            foreach (var cosmicItem in CosmicItems)
                cosmicItem.IsAlleleSpecific = cosmicItem.SaAltAllele == saAltAllele ? "true" : null;

            foreach (var clinVarEntry in ClinVarItems)
                clinVarEntry.IsAlleleSpecific = clinVarEntry.SaAltAllele == saAltAllele ? "true" : null;

            foreach (var customItem in CustomItems)
                customItem.IsAlleleSpecific = customItem.SaAltAllele == saAltAllele ? "true" : null;

        }

        /// <summary>
        /// sets the positional annotation
        /// </summary>
        public void SetPositionalAnnotation(AbstractAnnotationRecord annotationRecord)
        {
            if (annotationRecord == null) return;
            // check string values
            if (annotationRecord.DataType == AnnotationRecordDataType.String)
            {
                var stringRecord = annotationRecord as StringRecord;

                // sanity check: make sure we successfully recast
                if (stringRecord == null) throw new InvalidCastException("Unable to cast the AbstractAnnotationRecord as a StringRecord");

                var positionalId = (PositionalId)stringRecord.Id;

                switch (positionalId)
                {
                    case PositionalId.GlobalMinorAllele:
                        GlobalMinorAllele = stringRecord.Value;
                        break;
                    case PositionalId.GlobalMajorAllele:
                        GlobalMajorAllele = stringRecord.Value;
                        break;
                    case PositionalId.GlobalMinorAlleleFrequency:
                        GlobalMinorAlleleFrequency = stringRecord.Value;
                        break;
                    case PositionalId.GlobalMajorAlleleFrequency:
                        GlobalMajorAlleleFrequency = stringRecord.Value;
                        break;

                    default:
                        throw new GeneralException($"string record: Unable to convert byte ({stringRecord.Id} - {positionalId}) to positional ID.");
                }

                return;
            }


            // check boolean values
            if (annotationRecord.DataType == AnnotationRecordDataType.Boolean)
            {
                var boolRecord = annotationRecord as BooleanRecord;

                // sanity check: make sure we successfully recast
                if (boolRecord == null) throw new InvalidCastException("Unable to cast the AbstractAnnotationRecord as a BooleanRecord");

                switch ((PositionalId)boolRecord.Id)
                {
                    case PositionalId.IsRefMinorAllele:
                        IsRefMinorAllele = boolRecord.Value;
                        break;
                    default:
                        throw new GeneralException("Unable to convert byte to positional ID.");
                }

                return;
            }

            // unsupported data type
            throw new GeneralException($"Encountered an unknown data type: {annotationRecord.DataType}");
        }

        private AbstractAnnotationRecord GetPositionalAnnotation(PositionalId id)
        {
            // handle boolean variables
            if (id == PositionalId.IsRefMinorAllele)
            {
                var b = GetPositionalBoolean(id);
                return b ? new BooleanRecord((byte)id, true) : null;
            }

            // handle strings
            var s = GetPositionalString(id);
            return string.IsNullOrEmpty(s) ? null : new StringRecord((byte)id, s);
        }

        /// <summary>
        /// returns an positional boolean if it exists, null otherwise
        /// </summary>
        private bool GetPositionalBoolean(PositionalId id)
        {
            switch (id)
            {
                case PositionalId.IsRefMinorAllele:
                    return IsRefMinorAllele;
                default:
                    throw new InvalidCastException("Unhandled positional ID found: " + id);
            }
        }

        /// <summary>
        /// returns an positional string if it exists, null otherwise
        /// </summary>
        private string GetPositionalString(PositionalId id)
        {
            switch (id)
            {
                case PositionalId.GlobalMinorAllele:
                    return GlobalMinorAllele;

                case PositionalId.GlobalMinorAlleleFrequency:
                    return GlobalMinorAlleleFrequency;

                case PositionalId.GlobalMajorAllele:
                    return GlobalMajorAllele;

                case PositionalId.GlobalMajorAlleleFrequency:
                    return GlobalMajorAlleleFrequency;

                default:
                    throw new InvalidCastException("Unhandled positional ID found: " + id);
            }
        }

        public void MergeAnnotations(SupplementaryAnnotation other)
        {
            // sanity check: merge not possible as the annotations are for different positions
            if ((ReferencePosition != other.ReferencePosition) || (RefSeqName != other.RefSeqName)) return;

            // first the allele specific annotations are merged
            MergeAlleleSpecificAnnotations(other);
			
            CheckReferenceMinor(other);//should be silenced ofr GRCh38 till we find a reliable data source.

            // merge positional annotations
            MergePositionalAnnotations(other);

			// merging custom annotations: they cannot be categorized into positional or otherwise since that is only known from each customItem's IsAlleleSpecificFlag
            CustomItems.AddRange(other.CustomItems);
        }

	    public void FinalizePositionalAnnotations()
	    {
		    var count = 0;
		    if (!RefAlleleFreq.Equals(double.MinValue)) count++;
		    count += AlleleSpecificAnnotations.Count(asa => !asa.Value.AltAlleleFreq.Equals(double.MinValue));

		    if (count == 0) return;
		    
			//certain values like the GMAF and GMA can be computed only after all alt alleles have been seen
		    var alleleFreqDict = new Dictionary<string, double>(count);

		    if (!RefAlleleFreq.Equals(double.MinValue)) alleleFreqDict[RefAllele] = RefAlleleFreq;

			foreach (var asa in AlleleSpecificAnnotations)
		    {
			    alleleFreqDict[asa.Key] = asa.Value.AltAlleleFreq;
		    }

		    GlobalMajorAllele = GetMostFrequentAllele(alleleFreqDict, RefAllele);
		    if (GlobalMajorAllele != null)
			    GlobalMajorAlleleFrequency = alleleFreqDict[GlobalMajorAllele].ToString(CultureInfo.InvariantCulture);
		    else return;//no global alleles available

		    alleleFreqDict.Remove(GlobalMajorAllele);
		    
		    GlobalMinorAllele = GetMostFrequentAllele(alleleFreqDict, RefAllele, false);
			if (GlobalMinorAllele!=null)
				GlobalMinorAlleleFrequency = alleleFreqDict[GlobalMinorAllele].ToString(CultureInfo.InvariantCulture);
			
		}

		static string GetMostFrequentAllele(Dictionary<string, double> alleleFreqDict, string refAllele, bool isRefPreferred = true)
		{
			if (alleleFreqDict.Count == 0) return null;

			// find all alleles that have max frequency.
			var maxFreq = alleleFreqDict.Values.Max();
			if (Math.Abs(maxFreq - double.MinValue) < double.Epsilon) return null;

			var maxFreqAlleles = (from pair in alleleFreqDict where Math.Abs(pair.Value - maxFreq) < double.Epsilon select pair.Key).ToList();


			// if there is only one with max frequency, return it
			if (maxFreqAlleles.Count == 1)
				return maxFreqAlleles[0];

			// if ref is preferred (as in global major) it is returned
			if (isRefPreferred && maxFreqAlleles.Contains(refAllele))
				return refAllele;

			// else refAllele is removed and the first of the remaining allele is returned (arbitrary selection)
			maxFreqAlleles.Remove(refAllele);
			return maxFreqAlleles[0];

		}

		private void MergePositionalAnnotations(SupplementaryAnnotation other)
		{
			if (!other.RefAlleleFreq.Equals(double.MinValue))
			{
				RefAllele = other.RefAllele;
				RefAlleleFreq = other.RefAlleleFreq;
			}

            foreach (PositionalId positionalId in Enum.GetValues(typeof(PositionalId)))
            {
                var positionalAnnotationRecord = other.GetPositionalAnnotation(positionalId);

                SetPositionalAnnotation(positionalAnnotationRecord);

            }

            // a cosmic id may have multiple entries each for a study. So this needs special handling
            foreach (var cosmicItem in other.CosmicItems)
                AddCosmic(cosmicItem);
            ClinVarItems.AddRange(other.ClinVarItems);
        }

        // sums up all SNV frequencies for a position and checks if this is a ref minor site
        private void CheckReferenceMinor(SupplementaryAnnotation other)
        {
            TotalMinorAlleleFreq = 0;
            IsRefMinorAllele = false;
            other.IsRefMinorAllele = false;

            // we have to check if the total minor allele freq has crossed the threshold to be tagged as a ref minor
            // Note that for now only SNVs are considered.
            TotalMinorAlleleFreq = AlleleSpecificAnnotations.Where(asa => IsSnv(asa.Key) && asa.Value.OneKgAllAn != null && asa.Value.OneKgAllAc != null && asa.Value.OneKgAllAn.Value > 0).Sum(asa => (double)asa.Value.OneKgAllAc / (double)asa.Value.OneKgAllAn);
            IsRefMinorAllele = TotalMinorAlleleFreq >= RmaFreqThreshold;

            if (!(TotalMinorAlleleFreq > RmaFreqThreshold)) return;

            IsRefMinorAllele = true;
            other.IsRefMinorAllele = true;
        }

        private void MergeAlleleSpecificAnnotations(SupplementaryAnnotation other)
        {
            foreach (var otherAlleleAnnotation in other.AlleleSpecificAnnotations)
            {
                var otherAsa = otherAlleleAnnotation.Value;

                AlleleSpecificAnnotation asa;
                if (AlleleSpecificAnnotations.TryGetValue(otherAlleleAnnotation.Key, out asa))
                {
                    // merging dbsnp entries and Ancestral allele
                    asa.DbSnp.AddRange(otherAsa.DbSnp.Where(x => !asa.DbSnp.Contains(x)));

                    if (asa.AncestralAllele == null)
                        asa.AncestralAllele = otherAsa.AncestralAllele;
	                if (asa.AltAlleleFreq.Equals(double.MinValue))
		                asa.AltAlleleFreq = otherAsa.AltAlleleFreq;

                    // Note: the reason i keep exac 1000k and evs separate is because their arbitration strategies may vary

                    asa.MergeExacAnnotations(otherAsa);

                    asa.MergeEvsAnnotations(otherAsa);

                    asa.MergeOneKGenAnnotations(otherAsa);

                }
                else
                {
                    // this is for a new alternate allele
                    AlleleSpecificAnnotations[otherAlleleAnnotation.Key] = otherAsa;
                }
            }


        }


        public static bool IsSnv(string allele)
        {
            if (allele.Length != 1) return false;

            allele = allele.ToUpper();

            if (allele == "A" || allele == "C" || allele == "G" || allele == "T") return true;

            return false;
        }

        public bool NotEmpty()
        {
            bool notEmpty = AlleleSpecificAnnotations.Count > 0;

            if (notEmpty) return true;

            if (Enum.GetValues(typeof(PositionalId)).Cast<PositionalId>().Any(positionalId => GetPositionalAnnotation(positionalId) != null))
            {
                return true;
            }
            if (CosmicItems.Count > 0) return true;
            if (ClinVarItems.Count > 0) return true;
            if (CustomItems.Count > 0) return true;

            return false;
        }

        /// <summary>
        /// sets the allele-specific annotation
        /// </summary>
        public static void SetAlleleSpecificAnnotation(AlleleSpecificAnnotation asa, AbstractAnnotationRecord annotationRecord)
        {
            // check string list values: the default behavior is to aggregate values from the supplied annotation record
            if (annotationRecord == null) return;

            asa.CombineAnnotation(annotationRecord);

        }


        /// <summary>
        /// Returns a regular alternate allele when a provided with one have SA format.
        /// In case of long insertions or InsDel, where the saAltAllele contains an MD5 hash, the hash is returned.
        /// </summary>
        /// <param name="saAltAllele"> supplementary annotation alternate allele</param>
        /// <param name="emptyAllele">The way the calling function wants to represent an empty allele</param>
        /// <returns>regular alternate allele</returns>
        public static string ReverseSaReducedAllele(string saAltAllele, string emptyAllele = "-")
        {
            if (saAltAllele == null) return null;
            if (saAltAllele.All(char.IsDigit)) return emptyAllele; // this was a deletion

            int firstBaseIndex;
            for (firstBaseIndex = 0; firstBaseIndex < saAltAllele.Length; firstBaseIndex++)
            {
				if (saAltAllele[firstBaseIndex] != 'i' && saAltAllele[firstBaseIndex] != '<' &&
                    !char.IsDigit(saAltAllele[firstBaseIndex]))
                    break;
            }

            if (saAltAllele.Substring(firstBaseIndex) == "") return emptyAllele;

            return firstBaseIndex > 0 && firstBaseIndex < saAltAllele.Length ? saAltAllele.Substring(firstBaseIndex) : saAltAllele;
        }

        public static bool ValidateRefAllele(string refAllele, string refBases)
        {
            if (refBases == null) return true;
            if (refAllele == ".") return true;//ref base is unknown
            if (refBases.All(x => x == 'N')) return true;

            if (refAllele.Length < refBases.Length)
                return refBases.StartsWith(refAllele);

            // in rare cases the refAllele will be too large for our refBases string that is limited in length
            return refAllele.StartsWith(refBases);
        }

        /// <summary>
        /// Given a ref and alt allele string, return their trimmed version. 
        /// This method should be decommissioned once VariantAlternateAlleles are used in SA.
        /// </summary>
        /// <param name="refAllele"> The reference allele string </param>
        /// <param name="altAllele">The alternate allele string</param>
        /// <param name="newStart">new start position after trimming reference</param>
        /// <returns>Trimmed reference allele and reduced alternate allele as expected by SA</returns>
        public static Tuple<string, string> GetReducedAlleles(string refAllele, string altAllele, ref int newStart)
        {
            if (string.IsNullOrEmpty(altAllele))
            {
                // we have a deletion
                return Tuple.Create(refAllele, refAllele.Length.ToString(CultureInfo.InvariantCulture));

            }

            // when we have a supplementary annotation for the ref allele (as in clinVar sometimes), we should not apply any trimming or modification to the alleles.
            if (refAllele == altAllele)
                return Tuple.Create(refAllele, altAllele);

            // When we have a item that is derived from an entry, the alt alleles may have already been processed. We can detect the inserts and deletions and just return without any further processing. For MNVs, we have no way of detecting
            // we should also avoid any modifications for symbolic allele
            if (altAllele[0] == 'i' || altAllele[0] == '<' || char.IsDigit(altAllele[0]))
                return Tuple.Create(refAllele, altAllele);

            // trimming at the start
            int i = 0;
            while (i < refAllele.Length && i < altAllele.Length && refAllele[i] == altAllele[i])
                i++;

            string newAltAllele = altAllele;
            string newRefAllele = refAllele;

            if (i > 0)
            {
                newStart += i;
                newAltAllele = altAllele.Substring(i);
                newRefAllele = refAllele.Substring(i);
            }

            // trimming at the end
            int j = 0;
            while (j < newRefAllele.Length && j < newAltAllele.Length && newRefAllele[newRefAllele.Length - j - 1] == newAltAllele[newAltAllele.Length - j - 1])
                j++;

            if (j > 0)
            {
                newAltAllele = newAltAllele.Substring(0, newAltAllele.Length - j);
                newRefAllele = newRefAllele.Substring(0, newRefAllele.Length - j);
            }

            if (string.IsNullOrEmpty(newAltAllele))
            {
                // we have a deletion
                return Tuple.Create(newRefAllele, newRefAllele.Length.ToString(CultureInfo.InvariantCulture));

            }

            if (string.IsNullOrEmpty(newRefAllele))
            {
                // we have an insertion and we indicate that with an i 
                newAltAllele = 'i' + newAltAllele;
                return Tuple.Create(newRefAllele, newAltAllele);
            }

            if (newRefAllele.Length == newAltAllele.Length) //SNV or CNV
                return Tuple.Create(newRefAllele, newAltAllele);

            // its a delins 
            newAltAllele = newRefAllele.Length.ToString(CultureInfo.InvariantCulture) + newAltAllele;

            return Tuple.Create(newRefAllele, newAltAllele);
        }
        /// <summary>
        /// returns the MD5 checksum of the specified string
        /// </summary>
        public static string GetMd5HashString(string input)
        {
            var data = MD5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            MD5Builder.Clear();
            foreach (var b in data) MD5Builder.Append(b.ToString("x2"));
            return MD5Builder.ToString();
        }
    }
}
