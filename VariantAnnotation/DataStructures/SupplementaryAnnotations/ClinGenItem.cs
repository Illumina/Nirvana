using System;
using System.Collections.Generic;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public enum ClinicalInterpretation
    {
        // ReSharper disable InconsistentNaming
        pathogenic             = 5,
        likely_pathogenic      = 4,
        benign                 = 3,
        likely_benign          = 2,
        uncertain_significance = 1,
        unknown                = 0
        // ReSharper restore InconsistentNaming
    }

    public sealed class ClinGenItem : SupplementaryDataItem
    {
        public string Id { get; }
        public int End { get; }
        private VariantType VariantType { get; }
        public ClinicalInterpretation ClinicalInterpretation { get; private set; }
        public IEnumerable<string> Phenotypes => _phenotypes;
        private HashSet<string> _phenotypes { get; }
        public IEnumerable<string> PhenotypeIds => _phenotypeIds;
        private HashSet<string> _phenotypeIds { get; }
        public int ObservedGains { get; private set; }
        public int ObservedLosses { get; private set; }
        public bool Validated { get; private set; }

        public ClinGenItem(string id, string chromosome, int start, int end, VariantType variantType, int observedGains, int observedLosses,
            ClinicalInterpretation clinicalInterpretation, bool validated, HashSet<string> phenotypes = null, HashSet<string> phenotypeIds = null)
        {
            Id                     = id;
            Chromosome             = chromosome;
            Start                  = start;
            End                    = end;
            VariantType            = variantType;
            ClinicalInterpretation = clinicalInterpretation;
            _phenotypes            = phenotypes ?? new HashSet<string>();
            _phenotypeIds          = phenotypeIds ?? new HashSet<string>();
            ObservedGains          = observedGains;
            ObservedLosses         = observedLosses;
            Validated              = validated;
            IsInterval             = true;
        }

        public override SupplementaryDataItem SetSupplementaryAnnotations(SupplementaryPositionCreator sa, string refBases = null)
        {
            return null;
        }

        public override SupplementaryInterval GetSupplementaryInterval(ChromosomeRenamer renamer)
        {
            if (!IsInterval) return null;

            var intValues    = new Dictionary<string, int>();
            var doubleValues = new Dictionary<string, double>();
            var freqValues   = new Dictionary<string, double>();
            var stringValues = new Dictionary<string, string>();
            var boolValues   = new List<string>();
            var stringLists  = new Dictionary<string, IEnumerable<string>>();

            var suppInterval = new SupplementaryInterval(Start, End, Chromosome, null, VariantType,
                "ClinGen", renamer, intValues, doubleValues, freqValues, stringValues, boolValues, stringLists);

            if (Id                     != null) suppInterval.AddStringValue("id", Id);
            if (ClinicalInterpretation != ClinicalInterpretation.unknown) suppInterval.AddStringValue("clinicalInterpretation", GetClinicalDescription(ClinicalInterpretation));
            if (Phenotypes             != null) suppInterval.AddStringList("phenotypes", Phenotypes);
            if (PhenotypeIds           != null) suppInterval.AddStringList("phenotypeIds", PhenotypeIds);
            if (ObservedGains          != 0) suppInterval.AddIntValue("observedGains", ObservedGains);
            if (ObservedLosses         != 0) suppInterval.AddIntValue("observedLosses", ObservedLosses);
            if (Validated) suppInterval.AddBoolValue("validated");

            return suppInterval;
        }

        private static string GetClinicalDescription(ClinicalInterpretation clinicalInterpretation)
        {
            switch (clinicalInterpretation)
            {
                case ClinicalInterpretation.uncertain_significance:
                    return "uncertain significance";
                case ClinicalInterpretation.likely_benign:
                    return "likely benign";
                case ClinicalInterpretation.likely_pathogenic:
                    return "likely pathogenic";
                default:
                    return clinicalInterpretation.ToString();
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Chromosome?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Start.GetHashCode();
                hashCode = (hashCode * 397) ^ End.GetHashCode();
                hashCode = (hashCode * 397) ^ VariantType.GetHashCode();
                hashCode = (hashCode * 397) ^ Validated.GetHashCode();
                hashCode = (hashCode * 397) ^ ObservedGains.GetHashCode();
                hashCode = (hashCode * 397) ^ ClinicalInterpretation.GetHashCode();
                hashCode = (hashCode * 397) ^ ObservedLosses.GetHashCode();

                return hashCode;
            }
        }

        public void MergeItem(ClinGenItem other)
        {
            //sanity check 
            if (!Id.Equals(other.Id) || !Chromosome.Equals(other.Chromosome) || Start != other.Start || End != other.End)
            {
                throw new Exception($"different region with same parent ID {Id}\n");
            }

            //check if the validate status and clinical interpretation are consistent
            if (Validated || other.Validated)
            {
                Validated = true;
            }

            if (!ClinicalInterpretation.Equals(other.ClinicalInterpretation))
            {
                if (ClinicalInterpretation < other.ClinicalInterpretation)
                    ClinicalInterpretation = other.ClinicalInterpretation;
            }

            if (other.VariantType == VariantType.copy_number_gain)
            {
                ObservedGains++;
            }
            else if (other.VariantType == VariantType.copy_number_loss)
            {
                ObservedLosses++;
            }
            foreach (var phenotype in other.Phenotypes)
            {
                _phenotypes.Add(phenotype);
            }

            foreach (var phenotypeId in other.PhenotypeIds)
            {
                _phenotypeIds.Add(phenotypeId);
            }
        }
    }
}
