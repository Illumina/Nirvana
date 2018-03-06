using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.DataStructures
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
        private string Id { get; }
        private int End { get; }
        private VariantType VariantType { get; }
        private ClinicalInterpretation ClinicalInterpretation { get; }
        private IEnumerable<string> Phenotypes => _phenotypes;
	    private readonly HashSet<string> _phenotypes;
        private IEnumerable<string> PhenotypeIds => _phenotypeIds;
	    private readonly HashSet<string> _phenotypeIds;
        private int ObservedGains { get; }
        private int ObservedLosses { get; }
        private bool Validated { get; }

        public ClinGenItem(string id, IChromosome chromosome, int start, int end, VariantType variantType, int observedGains, int observedLosses,
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



        public override SupplementaryIntervalItem GetSupplementaryInterval()
        {
            if (!IsInterval) return null;

            var intValues    = new Dictionary<string, int>();
            var doubleValues = new Dictionary<string, double>();
            var freqValues   = new Dictionary<string, double>();
            var stringValues = new Dictionary<string, string>();
            var boolValues   = new List<string>();
            var stringLists  = new Dictionary<string, IEnumerable<string>>();

            var suppInterval = new SupplementaryIntervalItem(Chromosome,Start, End, null, VariantType,
                "ClinGen", intValues, doubleValues, freqValues, stringValues, boolValues, stringLists);

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
    }
}
