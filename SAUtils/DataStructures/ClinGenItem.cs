using System.Collections.Generic;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using Variants;

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

    public sealed class ClinGenItem:ISuppIntervalItem
    {
        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; }


        private string Id { get; }
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
        }



        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("chromosome", Chromosome.EnsemblName);
            jsonObject.AddIntValue("begin", Start);
            jsonObject.AddIntValue("end", End);
            jsonObject.AddStringValue("variantType", VariantType.ToString());
            jsonObject.AddStringValue("id", Id);
            jsonObject.AddStringValue("clinicalInterpretation", GetClinicalDescription(ClinicalInterpretation));
            jsonObject.AddStringValues("phenotypes", Phenotypes);
            jsonObject.AddStringValues("phenotypeIds", PhenotypeIds);
            if (ObservedGains>0) jsonObject.AddIntValue("observedGains", ObservedGains);
            if (ObservedLosses>0) jsonObject.AddIntValue("observedLosses", ObservedLosses);
            jsonObject.AddBoolValue("validated",Validated);

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        

        private static string GetClinicalDescription(ClinicalInterpretation clinicalInterpretation)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (clinicalInterpretation)
            {
                case ClinicalInterpretation.uncertain_significance:
                    return "uncertain significance";
                case ClinicalInterpretation.likely_benign:
                    return "likely benign";
                case ClinicalInterpretation.likely_pathogenic:
                    return "likely pathogenic";
                case ClinicalInterpretation.unknown:
                    return null;
                default:
                    return clinicalInterpretation.ToString();
            }
        }
        
        
    }
}
