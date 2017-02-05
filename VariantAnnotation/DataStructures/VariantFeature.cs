using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.DataStructures.VCF;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures
{
    public sealed class VariantFeature : IVariantFeature, IEquatable<VariantFeature>
    {
        #region members

        // basic variant information
        public string ReferenceName { get; private set; }
        public string UcscReferenceName { get; private set; }
        private string EnsemblReferenceName { get; set; }
        public ushort ReferenceIndex { get; private set; }
        public int VcfReferenceBegin { get; private set; }
        public int VcfReferenceEnd { get; private set; }
        private string VcfRefAllele { get; set; }
        private string VcfVariantId { get; set; }

        // defines the interval that should be used when looking for overlapping annotations
        public int OverlapReferenceBegin { get; private set; }
        public int OverlapReferenceEnd { get; private set; }

        public string CytogeneticBand { get; set; }

        // info fields
        public double? StrandBias { get; private set; }
        public int? JointSomaticNormalQuality { get; private set; }
        public double? RecalibratedQuality { get; private set; }
        public int? CopyNumber { get; private set; } // SENECA
        public VariantType InternalCopyNumberType { get; private set; }
        public string[] CiPos { get; private set; }
        public string[] CiEnd { get; private set; }
        private int? Depth { get; set; } // Pisces
        public int? SvLength { get; private set; }
        public bool ColocalizedWithCnv { get; private set; }

        // variant flags
        public bool IsReference { get; private set; }
        public bool IsStructuralVariant { get; private set; }
        public bool IsRefNoCall { get; internal set; }
        public bool IsRefMinor { get; private set; }
        public bool IsSingletonRefSite { get; private set; }

        // member data
        public string[] VcfColumns { get; private set; }
        public List<VariantAlternateAllele> AlternateAlleles { get; }

        public IAllele FirstAlternateAllele => AlternateAlleles[0];

        public string SiftScore;
        public string SiftPrediction;
        public string PolyPhenScore;
        public string PolyPhenPrediction;

        public string ConservationScore;
        private bool _fixGatkGenomeVcf;

        private const string CsqPrefixTag = "CSQ";
        private const string CsqRPrefixTag = "CSQR";
        private const string CsqTPrefixTag = "CSQT";
        private const string OneKGenTag = "AF1000G";
        private const string AncestralAlleleTag = "AA";
        private const string CosmicTag = "cosmic";
        private const string ClinVarTag = "clinvar";
        private const string EvsTag = "EVS";
        private const string GmafTag = "GMAF";
        private const string PhylopTag = "phyloP";
        private const string RefMinorTag = "RefMinor";

        private static readonly HashSet<string> TagsToRemove = new HashSet<string>
            {
                CsqPrefixTag,
                CsqTPrefixTag,
                CsqRPrefixTag,
                OneKGenTag,
                AncestralAlleleTag,
                CosmicTag,
                ClinVarTag,
                EvsTag,
                PhylopTag,
                GmafTag,
                RefMinorTag
            };

        // used for reference sites
        public ISupplementaryAnnotationPosition SupplementaryAnnotationPosition;
        private List<ISupplementaryInterval> _overlappingSupplementaryIntervals;
        private Dictionary<string, string> _infoKeyValue;

        private static readonly IAlleleTrimmer AlleleTrimmer = new BiDirectionalTrimmer();
        private readonly ChromosomeRenamer _renamer;
        private readonly VID _vid;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public VariantFeature(VcfVariant variant, ChromosomeRenamer renamer, VID vid)
        {
            _renamer = renamer;
            _vid     = vid;

            AlternateAlleles = new List<VariantAlternateAllele>();
            if (variant.IsGatkGenomeVcf) EnableGatkGenomeVcfFix();
            ParseVcfLine(variant.Fields);
        }

        /// <summary>
        /// sets all of the entries in the genomic duplicates list to true if the corresponding alternate allele
        /// causes a genomic duplicate.
        /// </summary>
        public void CheckForGenomicDuplicates(ICompressedSequence compressedSequence)
        {
            foreach (var altAllele in AlternateAlleles)
            {
                altAllele.CheckForDuplicationForAltAllele(compressedSequence);
            }
        }

        /// <summary>
        /// gets the variant type given the ref and alt columns. This method is used for all
        /// methods except for those in the HGVS class.
        /// </summary>
        private static VariantType GetVariantType(int referenceAlleleLen, int alternateAlleleLen)
        {
            VariantType variantType;

            if (alternateAlleleLen != referenceAlleleLen)
            {
                variantType = alternateAlleleLen > referenceAlleleLen ? VariantType.insertion : VariantType.deletion;
            }
            else
            {
                variantType = alternateAlleleLen == 1 ? VariantType.SNV : VariantType.MNV;
            }

            return variantType;
        }

        private static VariantType NirvanaVariantType(int referenceAlleleLen, int alternateAlleleLen)
        {
            if (alternateAlleleLen != referenceAlleleLen)
            {
                if (alternateAlleleLen == 0 && referenceAlleleLen > 0) return VariantType.deletion;
                if (alternateAlleleLen > 0 && referenceAlleleLen == 0) return VariantType.insertion;

                return VariantType.indel;
            }

            var variantType = alternateAlleleLen == 1 ? VariantType.SNV : VariantType.MNV;

            return variantType;
        }

        /// <summary>
        /// parses the INFO fields for SV-specific information
        /// </summary>
        private StructuralVariant ParseSvFields()
        {
            // update each alternate allele
            var sv = new StructuralVariant(VcfColumns, VcfReferenceBegin, VcfReferenceEnd, _renamer, _vid);
            foreach (var altAllele in AlternateAlleles) sv.AssignVariantType(altAllele, AlleleTrimmer);

            return sv;
        }

        public void Clear()
        {
            SupplementaryAnnotationPosition = null;
            ReferenceName = null;
            UcscReferenceName = null;
            EnsemblReferenceName = null;
            VcfRefAllele = null;
            VcfColumns = null;
            ConservationScore = null;
            CytogeneticBand = null;
            VcfVariantId = null;

            IsRefMinor = false;
            IsSingletonRefSite = false;
            IsReference = false;
            IsRefNoCall = false;
            IsStructuralVariant = false;
            VcfReferenceBegin = 0;
            VcfReferenceEnd = 0;
            OverlapReferenceBegin = 0;
            OverlapReferenceEnd = 0;
            StrandBias = null;
            JointSomaticNormalQuality = null;
            RecalibratedQuality = null;
            CiPos = null;
            CiEnd = null;
            CopyNumber = null;
            Depth = null;
            InternalCopyNumberType = VariantType.unknown;
            SvLength = null;
            ColocalizedWithCnv = false;

            AlternateAlleles?.Clear();
            _overlappingSupplementaryIntervals?.Clear();
        }

        /// <summary>
        /// takes a vcf line and populates the variant object
        /// </summary>
        private void ParseVcfLine(string[] vcfColumns)
        {
            Clear();

            VcfColumns = vcfColumns;
            CheckVcfColumnCount();

            // fix GATK genome vcf entries
            if (_fixGatkGenomeVcf) RemoveGatkNonRefAllele();

            AssignVcfFields();
            ParseInfoField(VcfColumns[VcfCommon.InfoIndex]);

            IsStructuralVariant = VcfColumns[VcfCommon.InfoIndex].Contains(VcfCommon.StructuralVariantTag);
        }

        public void AssignAlternateAlleles()
        {
            // sanity check: skip reference sites 
            if (IsReference)
            {
                var infoEnd = ExtractInfoEnd();
                if (infoEnd != null) VcfReferenceEnd = (int)infoEnd;
                IsSingletonRefSite = VcfReferenceEnd == VcfReferenceBegin || VcfReferenceEnd == -1;

                // basic blank allele for ref sites
                var refSiteAltAllele = new VariantAlternateAllele(VcfReferenceBegin, VcfReferenceEnd,
                    VcfColumns[VcfCommon.RefIndex], ".");

                // create alternative allele for reference no call
                if (IsRefNoCall)
                {
                    // the following are for refNoCall only.
                    refSiteAltAllele.NirvanaVariantType = VariantType.reference_no_call;
                    refSiteAltAllele.VepVariantType = VariantType.reference_no_call;
                    refSiteAltAllele.VariantId = _vid.Create(_renamer, ReferenceName, refSiteAltAllele);
                }

                AlternateAlleles.Add(refSiteAltAllele);

                if (_fixGatkGenomeVcf) RecoverGatkNonRefAllele();
                return;
            }

            // split our alternate alleles
            var altAlleles = VcfColumns[VcfCommon.AltIndex].Split(',');

            var genotypeIndex = 1;
            foreach (var altAllele in altAlleles)
            {
                AlternateAlleles.Add(new VariantAlternateAllele(VcfReferenceBegin, VcfReferenceEnd, VcfRefAllele, altAllele, genotypeIndex));
                genotypeIndex++;
            }



            if (IsStructuralVariant) FixMantaSvs(VcfColumns[VcfCommon.InfoIndex]);

            if (IsStructuralVariant)
            {
                var infoEnd = ExtractInfoEnd();
                if (infoEnd != null) VcfReferenceEnd = (int)infoEnd;

                var sv = ParseSvFields();
                int? copyNumber = null;

                if (CopyNumber != null)
                {
                    copyNumber = CopyNumber;
                }
                else
                {
                    if (AlternateAlleles.Count == 1 && AlternateAlleles.First().CopyNumber != null)
                    {
                        int altCopyNumber;
                        if (int.TryParse(AlternateAlleles.First().CopyNumber, out altCopyNumber)) copyNumber = altCopyNumber;
                    }
                }

                EvaluateCopyNumberType(copyNumber);

                OverlapReferenceBegin = sv.MinBegin;
                OverlapReferenceEnd = sv.MaxEnd;
            }
            else
            {
                TrimAlternateAlleles();
                AssignVariantTypes();
                AssignVariantIds();
            }

            if (_fixGatkGenomeVcf) RecoverGatkNonRefAllele();
        }

        private void EvaluateCopyNumberType(int? copyNumber)
        {
            if (copyNumber == null) return;

            if (VcfVariantId.StartsWith("Canvas:"))
            {
                var canvasInfo = VcfVariantId.Split(':');
                if (canvasInfo[1].Equals("GAIN")) InternalCopyNumberType = VariantType.copy_number_gain;
                if (canvasInfo[1].Equals("LOSS")) InternalCopyNumberType = VariantType.copy_number_loss;
                if (canvasInfo[1].Equals("REF")) InternalCopyNumberType = VariantType.copy_number_variation;
                return;
            }

            var baseCopyNumber = UcscReferenceName == "chrY" ? 1 : 2;

            if (copyNumber < baseCopyNumber)
            {
                InternalCopyNumberType = VariantType.copy_number_loss;
                return;
            }

            InternalCopyNumberType = copyNumber > baseCopyNumber
                ? VariantType.copy_number_gain
                : VariantType.copy_number_variation;
        }

        private void RecoverGatkNonRefAllele()
        {
            var altAllele = VcfColumns[VcfCommon.AltIndex];
            if (altAllele.Equals("."))
            {
                VcfColumns[VcfCommon.AltIndex] = VcfCommon.GatkNonRefAllele;
                return;
            }

            VcfColumns[VcfCommon.AltIndex] = altAllele + "," + VcfCommon.GatkNonRefAllele;
        }

        /// <summary>
        /// removes the GATK NON_REF alternate allele in a way that affects the output vcfs
        /// </summary>
        private void RemoveGatkNonRefAllele()
        {
            var altAllele = VcfColumns[VcfCommon.AltIndex];

            // handle reference sites
            if (altAllele == VcfCommon.GatkNonRefAllele)
            {
                VcfColumns[VcfCommon.AltIndex] = ".";
                return;
            }

            // handle variant sites
            VcfColumns[VcfCommon.AltIndex] = altAllele.Replace("," + VcfCommon.GatkNonRefAllele, "");
        }

        private void AssignVcfFields()
        {
            ReferenceName = VcfColumns[VcfCommon.ChromIndex];
            ReferenceIndex = _renamer.GetReferenceIndex(ReferenceName);
            EnsemblReferenceName = ReferenceIndex == ushort.MaxValue? ReferenceName: _renamer.EnsemblReferenceNames[ReferenceIndex];
            UcscReferenceName = ReferenceIndex == ushort.MaxValue ? ReferenceName : _renamer.UcscReferenceNames[ReferenceIndex];

            var referenceAllele = VcfColumns[VcfCommon.RefIndex];
            VcfRefAllele = referenceAllele;
            VcfReferenceBegin = int.Parse(VcfColumns[VcfCommon.PosIndex]);
            VcfReferenceEnd = VcfReferenceBegin + referenceAllele.Length - 1;
            VcfVariantId = VcfColumns[VcfCommon.IdIndex];

            OverlapReferenceBegin = VcfReferenceBegin;
            OverlapReferenceEnd = VcfReferenceEnd;

            IsReference = VcfColumns[VcfCommon.AltIndex] == VcfCommon.NonVariant;
            IsRefNoCall = false;

            // reset the info field		
            if (VcfColumns[VcfCommon.InfoIndex] == ".") VcfColumns[VcfCommon.InfoIndex] = "";
        }

        private void CheckVcfColumnCount()
        {
            var numColumns = VcfColumns.Length;

            if (numColumns < VcfCommon.MinNumColumns)
            {
                throw new UserErrorException(
                    $"Expected at least {VcfCommon.MinNumColumns} columns in the vcf entry, but found only {numColumns} columns.");
            }
        }

        /// <summary>
        /// check structural variants to see if this is actually a normal deletion
        /// </summary>
        private void FixMantaSvs(string infoField)
        {
            if (infoField.Contains("SVTYPE=") && !infoField.Contains("SVTYPE=BND") &&
                !AlternateAlleles.Any(x => x.IsSymbolicAllele))
                IsStructuralVariant = false;
        }

        private void ParseInfoField(string infoField)
        {
            if (string.IsNullOrEmpty(infoField)) return;

            ExtractInfoFields(infoField);

            foreach (var kvp in _infoKeyValue)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                switch (key)
                {
                    case "SB":
                        StrandBias = Convert.ToDouble(value);
                        break;
                    case "QSI_NT":
                    case "SOMATICSCORE":
                    case "QSS_NT":
                        JointSomaticNormalQuality = Convert.ToInt32(value);
                        break;
                    case "VQSR":
                        RecalibratedQuality = Convert.ToDouble(value);
                        break;
                    case "CN": // SENECA
                        CopyNumber = Convert.ToInt32(value);
                        break;
                    case "DP": // Pisces
                        Depth = Convert.ToInt32(value);
                        break;
                    case "CIPOS":
                        CiPos = value.Split(',');
                        break;
                    case "CIEND":
                        CiEnd = value.Split(',');
                        break;
                    case "SVLEN":
                        SvLength = Math.Abs(Convert.ToInt32(value));
                        break;
                    case "ColocalizedCanvas":
                        ColocalizedWithCnv = true;
                        break;



                }
            }
        }

        private void ExtractInfoFields(string infoField)
        {
            _infoKeyValue = new Dictionary<string, string>();

            var infoFields = infoField.Split(';');

            var sb = new StringBuilder();

            foreach (var field in infoFields)
            {
                var keyValue = field.Split('=');

                var key = keyValue[0];
                if (TagsToRemove.Contains(key)) continue;


                sb.Append(field);
                sb.Append(';');

                if (keyValue.Length == 1) _infoKeyValue[key] = "true";
                if (keyValue.Length != 1) _infoKeyValue[key] = keyValue[1];
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1); //removing the last semi-colon

                VcfColumns[VcfCommon.InfoIndex] = sb.ToString();
            }
            else VcfColumns[VcfCommon.InfoIndex] = "";

        }


        private int? ExtractInfoEnd()
        {
            if (_infoKeyValue == null) return null;
            if (_infoKeyValue.ContainsKey("END")) return Convert.ToInt32(_infoKeyValue["END"]);
            return null;

        }

        /// <summary>
        /// returns true if this object is equal to the other object
        /// </summary>
        public bool Equals(VariantFeature other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return ReferenceName == other.ReferenceName &&
                   AlternateAlleles.SequenceEqual(other.AlternateAlleles) &&
                   VcfReferenceBegin == other.VcfReferenceBegin &&
                   VcfReferenceEnd == other.VcfReferenceEnd &&
                   VcfRefAllele == other.VcfRefAllele;
        }

        /// <summary>
        /// returns a string representation of this variant
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(new string('=', 42));
            sb.AppendFormat("reference name: {0}\n", ReferenceName);
            sb.AppendFormat("reference begin: {0}\n", VcfReferenceBegin);
            sb.AppendFormat("reference end: {0}\n", VcfReferenceEnd);
            foreach (var altAllele in AlternateAlleles) sb.Append(altAllele);
            sb.AppendLine(new string('=', 42));

            return sb.ToString();
        }

        /// <summary>
        /// trims all of the alternate alleles
        /// </summary>
        internal void TrimAlternateAlleles()
        {
            AlleleTrimmer.Trim(AlternateAlleles);
        }

        /// <summary>
        /// assigns the variant type to each alternate allele
        /// </summary>
        private void AssignVariantTypes()
        {
            foreach (var altAllele in AlternateAlleles)
            {
                if (altAllele.IsSymbolicAllele) continue;
                //change the variant type for MantaDeletion
                altAllele.VepVariantType = GetVariantType(altAllele.ReferenceAllele.Length, altAllele.AlternateAllele.Length);
                altAllele.NirvanaVariantType = NirvanaVariantType(altAllele.ReferenceAllele.Length, altAllele.AlternateAllele.Length);
            }
        }

        /// <summary>
        /// assigns a VID to each alternate allele
        /// </summary>
        private void AssignVariantIds()
        {
            foreach (var altAllele in AlternateAlleles)
            {
                altAllele.VariantId = _vid.Create(_renamer, ReferenceName, altAllele);
            }
        }

        /// <summary>
        /// sets the supplementary annotation record for each allele in this record
        /// </summary>
        public void SetSupplementaryAnnotation(ISupplementaryAnnotationReader saReader)
        {
            // for ref variants, alternate alleles will be empty. We will need to pull the SA from reference position.
            // If this is a ref minor position, I will add an alt allele with the ref as alt and GMAF as ref
            if (IsReference)
            {

                // checking the index for refMinor using the index before actually going to disk
                IsRefMinor = saReader.IsRefMinor(VcfReferenceBegin);

                if (!IsRefMinor) return;

                SupplementaryAnnotationPosition = saReader.GetAnnotation(VcfReferenceBegin);
                if (SupplementaryAnnotationPosition == null) return;


                // if we have a ref minor, we do not care about ref no call
                IsRefNoCall = false;

                AlternateAlleles[0].AlternateAllele = AlternateAlleles[0].ReferenceAllele;// ref becomes alt for ref minor

                if (SupplementaryAnnotationPosition.GlobalMajorAllele != null)
                {
                    AlternateAlleles[0].ReferenceAllele = SupplementaryAnnotationPosition.GlobalMajorAllele;
                }

                AlternateAlleles[0].NirvanaVariantType = VariantType.SNV;
                AlternateAlleles[0].VepVariantType     = VariantType.SNV;
                AlternateAlleles[0].SuppAltAllele      = AlternateAlleles[0].AlternateAllele; // for SNVs there is nothing to reduce
                AlternateAlleles[0].VariantId          = _vid.Create(_renamer, EnsemblReferenceName, AlternateAlleles[0]);
                AlternateAlleles[0].SetSupplementaryAnnotation(SupplementaryAnnotationPosition);

                return;
            }

            foreach (var altAllele in AlternateAlleles)
            {
                var sa = saReader.GetAnnotation(altAllele.Start);
                if (sa != null) altAllele.SetSupplementaryAnnotation(sa);
            }
        }

        /// <summary>
        /// retrieves the genotype indices from the genotype
        /// </summary>
        internal static void GetGenotypeIndices(string genotype, List<int> genotypeIndices)
        {
            // skip the null genotype and make sure we don't have any calls that weren't genotyped
            if (genotype == "." || genotype.IndexOf('.') != -1) return;

            // grab our genotype
            char[] delimiterChars = { '/', '|' };
            var hapGenotypes = genotype.Split(delimiterChars);

            // sanity check: do we have a higher ploidy than 2?
            if (hapGenotypes.Length > 2)
            {
                throw new GeneralException(
                    $"Expected a ploidy of 2 or less when extracting the genotype. Found: {hapGenotypes.Length} ploidy.");
            }

            foreach (var hapGenotype in hapGenotypes)
            {
                int genotypeIndex;
                if (!int.TryParse(hapGenotype, out genotypeIndex))
                {
                    genotypeIndices.Clear();
                    return;
                }

                genotypeIndices.Add(genotypeIndex);
            }
        }

        public void AddCustomAnnotation(List<ISupplementaryAnnotationReader> saReaders)
        {
            if (IsReference)
            {
                foreach (var saReader in saReaders)
                {
                    var sa = saReader.GetAnnotation(VcfReferenceBegin);
                    if (sa == null) continue;
                    if (SupplementaryAnnotationPosition != null) SupplementaryAnnotationPosition.CustomItems.AddRange(sa.CustomItems);
                    else SupplementaryAnnotationPosition = sa;
                }

                return;
            }

            foreach (var altAllele in AlternateAlleles)
            {
                foreach (var saReader in saReaders)
                {
                    var sa = saReader.GetAnnotation(altAllele.Start);

                    if (sa != null) altAllele.AddCustomAnnotation(sa);
                }
            }
        }

        /// <summary>
        /// extracts the genotype fields from the VCF file and returns a list of JSON samples
        /// </summary>
        public List<JsonSample> ExtractSampleInfo()
        {
            return new SampleFieldExtractor(VcfColumns, Depth).ExtractSamples(_fixGatkGenomeVcf);
        }

        /// <summary>
        /// enables the GATK genome vcf fix (removes the NON_REF symbolic alleles)
        /// </summary>
        private void EnableGatkGenomeVcfFix()
        {
            _fixGatkGenomeVcf = true;
        }

        public void AddSupplementaryIntervals(List<ISupplementaryInterval> overlappingSupplementaryIntervals)
        {
            _overlappingSupplementaryIntervals = overlappingSupplementaryIntervals;
        }

        public List<ISupplementaryInterval> GetSupplementaryIntervals()
        {
            return _overlappingSupplementaryIntervals;
        }

        public bool PassFilter()
        {
            var filters = VcfColumns[VcfCommon.FilterIndex];
            return filters == "PASS" || filters == ".";
        }

        public void AddCustomIntervals(List<ICustomInterval> intervals)
        {
            foreach (var altAllele in AlternateAlleles) altAllele.AddCustomIntervals(intervals);
        }
    }
}
