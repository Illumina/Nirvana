using System;
using System.Collections.Generic;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public sealed class OneKGenItem : SupplementaryDataItem
    {
        #region members

        private string Id { get; }
        private string ReferenceAllele { get; }
        public string AncestralAllele { get; }
        private string AlternateAllele { get; }

        private string AfrFreq { get; }
        private string AllFreq { get; }
        private string AmrFreq { get; }
        private string EasFreq { get; }
        private string EurFreq { get; }
        private string SasFreq { get; }

        public int? AllAlleleNumber { get; }
        public int? AfrAlleleNumber { get; }
        public int? AmrAlleleNumber { get; }
        public int? EurAlleleNumber { get; }
        public int? EasAlleleNumber { get; }
        public int? SasAlleleNumber { get; }

        public int? AllAlleleCount { get; }
        public int? AfrAlleleCount { get; }
        public int? AmrAlleleCount { get; }
        public int? EurAlleleCount { get; }
        public int? EasAlleleCount { get; }
        public int? SasAlleleCount { get; }

        private string SvType { get; }
        private int SvEnd { get; }
        private int ObservedGains { get; }
        private int ObservedLosses { get; }
        private Tuple<int, int> CiPos { get; }
        private Tuple<int, int> CiEnd { get; }

        #endregion

        public OneKGenItem(string chromosome,
            int position,
            string id,
            string refAllele,
            string alternateAllele,
            string ancestralAllele,
            int? allAlleleCount,
            int? afrAlleleCount,
            int? amrAlleleCount,
            int? eurAlleleCount,
            int? easAlleleCount,
            int? sasAlleleCount,
            int? allAlleleNumber,
            int? afrAlleleNumber,
            int? amrAlleleNumber,
            int? eurAlleleNumber,
            int? easAlleleNumber,
            int? sasAlleleNumber,
            string svType,
            int svEnd,
            Tuple<int, int> ciPos,
            Tuple<int, int> ciEnd
            )
        {
            Chromosome = chromosome;
            Start = position;
            Id = id;
            ReferenceAllele = refAllele;
            AlternateAllele = alternateAllele;
            AncestralAllele = ancestralAllele;


            AllAlleleCount = allAlleleCount;
            AfrAlleleCount = afrAlleleCount;
            AmrAlleleCount = amrAlleleCount;
            EurAlleleCount = eurAlleleCount;
            EasAlleleCount = easAlleleCount;
            SasAlleleCount = sasAlleleCount;

            AllAlleleNumber = allAlleleNumber;
            AfrAlleleNumber = afrAlleleNumber;
            AmrAlleleNumber = amrAlleleNumber;
            EurAlleleNumber = eurAlleleNumber;
            EasAlleleNumber = easAlleleNumber;
            SasAlleleNumber = sasAlleleNumber;

            SvType = svType;
            SvEnd = svEnd;
            CiPos = ciPos;
            CiEnd = ciEnd;

            IsInterval = svType != null;
        }

        public OneKGenItem(string chromosome,
            int position,
            string id,
            string refAllele,
            string alternateAllele,
            string ancestralAllele,
            string afrFreq,
            string allFreq,
            string amrFreq,
            string easFreq,
            string eurFreq,
            string sasFreq,
            int? allAlleleCount,
            int? afrAlleleCount,
            int? amrAlleleCount,
            int? eurAlleleCount,
            int? easAlleleCount,
            int? sasAlleleCount,
            int? allAlleleNumber,
            int? afrAlleleNumber,
            int? amrAlleleNumber,
            int? eurAlleleNumber,
            int? easAlleleNumber,
            int? sasAlleleNumber,
            string svType,
            int svEnd,
            Tuple<int, int> ciPos,
            Tuple<int, int> ciEnd, int observedGains, int observedLosses)
            : this(
                chromosome, position, id, refAllele, alternateAllele, ancestralAllele, allAlleleCount, afrAlleleCount, amrAlleleCount, eurAlleleCount, easAlleleCount, sasAlleleCount,
                allAlleleNumber, afrAlleleNumber, amrAlleleNumber, eurAlleleNumber, easAlleleNumber, sasAlleleNumber, svType,
                svEnd, ciPos, ciEnd)
        {
            ObservedGains = observedGains;
            ObservedLosses = observedLosses;

            AfrFreq = afrFreq;
            AllFreq = allFreq;
            AmrFreq = amrFreq;
            EasFreq = easFreq;
            EurFreq = eurFreq;
            SasFreq = sasFreq;
        }

        public override SupplementaryInterval GetSupplementaryInterval(ChromosomeRenamer renamer)
        {
            if (!IsInterval) return null;

            var seqAltType = SequenceAlterationUtilities.GetSequenceAlteration(SvType, ObservedGains, ObservedLosses);

            var intValues    = new Dictionary<string, int>();
            var doubleValues = new Dictionary<string, double>();
            var freqValues   = new Dictionary<string, double>();
            var stringValues = new Dictionary<string, string>();
            var boolValues   = new List<string>();

            var suppInterval = new SupplementaryInterval(Start, SvEnd, Chromosome, AlternateAllele, seqAltType,
                "1000 Genomes Project", renamer, intValues, doubleValues, freqValues, stringValues, boolValues);

            if (Id != null) suppInterval.AddStringValue("id", Id);
            if (AfrFreq != null) suppInterval.AddFrequencyValue("variantFreqAfr", Convert.ToDouble(AfrFreq));
            if (AllFreq != null) suppInterval.AddFrequencyValue("variantFreqAll", Convert.ToDouble(AllFreq));
            if (AmrFreq != null) suppInterval.AddFrequencyValue("variantFreqAmr", Convert.ToDouble(AmrFreq));
            if (EasFreq != null) suppInterval.AddFrequencyValue("variantFreqEas", Convert.ToDouble(EasFreq));
            if (EurFreq != null) suppInterval.AddFrequencyValue("variantFreqEur", Convert.ToDouble(EurFreq));
            if (SasFreq != null) suppInterval.AddFrequencyValue("variantFreqSas", Convert.ToDouble(SasFreq));

            if (AllAlleleNumber != null && AllAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSize", AllAlleleNumber.Value);
            if (AfrAlleleNumber != null && AfrAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSizeAfr", AfrAlleleNumber.Value);
            if (AmrAlleleNumber != null && AmrAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSizeAmr", AmrAlleleNumber.Value);
            if (EasAlleleNumber != null && EasAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSizeEas", EasAlleleNumber.Value);
            if (EurAlleleNumber != null && EurAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSizeEur", EurAlleleNumber.Value);
            if (SasAlleleNumber != null && SasAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSizeSas", SasAlleleNumber.Value);

            if (ObservedGains != 0) suppInterval.AddIntValue("observedGains", ObservedGains);
            if (ObservedLosses != 0) suppInterval.AddIntValue("observedLosses", ObservedLosses);

            return suppInterval;
        }

        public override SupplementaryDataItem SetSupplementaryAnnotations(SupplementaryPositionCreator sa, string refBases = null)
        {
            // check if the ref allele matches the refBases as a prefix
            if (!SupplementaryAnnotationUtilities.ValidateRefAllele(ReferenceAllele, refBases))
                return null; //the ref allele for this entry did not match the reference bases.

            // if this is a SV, 
            if (IsInterval) return null;

            var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(Start, ReferenceAllele, AlternateAllele);

            var newStart     = newAlleles.Item1;
            var newRefAllele = newAlleles.Item2;
            var newAltAllele = newAlleles.Item3;

            if (newRefAllele != ReferenceAllele)
            {
                return new OneKGenItem(Chromosome, newStart, Id, newRefAllele, newAltAllele, AncestralAllele,
                    AllAlleleCount, AfrAlleleCount, AmrAlleleCount, EurAlleleCount, EasAlleleCount, SasAlleleCount,
                    AllAlleleNumber, AfrAlleleNumber, AmrAlleleNumber, EurAlleleNumber, EasAlleleNumber, SasAlleleNumber,
                    SvType, SvEnd, CiPos, CiEnd);
            }

            SetSaFields(sa, newAltAllele);

            return null;
        }

        private void SetSaFields(SupplementaryPositionCreator saCreator, string altAllele)
        {
            var annotation = new OneKGenAnnotation
            {
                AncestralAllele = AncestralAllele,

                OneKgAllAn = AllAlleleNumber,
                OneKgAfrAn = AfrAlleleNumber,
                OneKgAmrAn = AmrAlleleNumber,
                OneKgEurAn = EurAlleleNumber,
                OneKgEasAn = EasAlleleNumber,
                OneKgSasAn = SasAlleleNumber,

                OneKgAllAc = AllAlleleCount,
                OneKgAfrAc = AfrAlleleCount,
                OneKgAmrAc = AmrAlleleCount,
                OneKgEurAc = EurAlleleCount,
                OneKgEasAc = EasAlleleCount,
                OneKgSasAc = SasAlleleCount

            };

            saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.OneKg, altAllele, annotation);
        }

        public override bool Equals(object other)
        {
            // If parameter is null return false.

            // if other cannot be cast into OneKGenItem, return false
            var otherItem = other as OneKGenItem;
            if (otherItem == null) return false;

            // Return true if the fields match:
            return string.Equals(Chromosome, otherItem.Chromosome)
                && Start == otherItem.Start
                && AlternateAllele.Equals(otherItem.AlternateAllele)
                ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (ReferenceAllele?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (AlternateAllele?.GetHashCode() ?? 0);

                return hashCode;
            }
        }
    }
}
