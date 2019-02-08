using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using Variants;

namespace SAUtils.DataStructures
{
    public sealed class OnekGenSvItem: ISuppIntervalItem
    {
        public OnekGenSvItem(IChromosome chromosome, int start, int end, VariantType variantType, string id, string afrFrequency, string allFrequency, string amrFrequency, string easFrequency, string eurFrequency, string sasFrequency, int allAlleleNumber, int afrAlleleNumber, int amrAlleleNumber, int eurAlleleNumber, int easAlleleNumber, int sasAlleleNumber, int observedGains, int observedLosses)
        {
            Chromosome = chromosome;
            Start      = start;
            End        = end;
            Id         = id;
            VariantType = variantType;

            AfrFrequency    = afrFrequency;
            AllFrequency    = allFrequency;
            AmrFrequency    = amrFrequency;
            EasFrequency    = easFrequency;
            EurFrequency    = eurFrequency;
            SasFrequency    = sasFrequency;
            AllAlleleNumber = allAlleleNumber;
            AfrAlleleNumber = afrAlleleNumber;
            AmrAlleleNumber = amrAlleleNumber;
            EurAlleleNumber = eurAlleleNumber;
            EasAlleleNumber = easAlleleNumber;
            SasAlleleNumber = sasAlleleNumber;
            ObservedGains   = observedGains;
            ObservedLosses  = observedLosses;
        }

        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; }
        public VariantType VariantType { get; }

        private string Id { get; }
        private string AfrFrequency { get; }
        private string AllFrequency { get; }
        private string AmrFrequency { get; }
        private string EasFrequency { get; }
        private string EurFrequency { get; }
        private string SasFrequency { get; }

        private int? AllAlleleNumber { get; }
        private int? AfrAlleleNumber { get; }
        private int? AmrAlleleNumber { get; }
        private int? EurAlleleNumber { get; }
        private int? EasAlleleNumber { get; }
        private int? SasAlleleNumber { get; }

        private int ObservedGains { get; }
        private int ObservedLosses { get; }
        

        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("chromosome", Chromosome.EnsemblName);
            jsonObject.AddIntValue("begin", Start);
            jsonObject.AddIntValue("end", End);
            jsonObject.AddStringValue("variantType", VariantType.ToString());

            jsonObject.AddStringValue("id", Id);
            jsonObject.AddStringValue("variantFreqAll", AllFrequency, false);
            jsonObject.AddStringValue("variantFreqAfr", AfrFrequency, false);
            jsonObject.AddStringValue("variantFreqAmr", AmrFrequency, false);
            jsonObject.AddStringValue("variantFreqEas", EasFrequency, false);
            jsonObject.AddStringValue("variantFreqEur", EurFrequency, false);
            jsonObject.AddStringValue("variantFreqSas", SasFrequency, false);

            jsonObject.AddIntValue("sampleSize", AllAlleleNumber);
            jsonObject.AddIntValue("sampleSizeAfr", AfrAlleleNumber);
            jsonObject.AddIntValue("sampleSizeAmr", AmrAlleleNumber);
            jsonObject.AddIntValue("sampleSizeEas", EasAlleleNumber);
            jsonObject.AddIntValue("sampleSizeEur", EurAlleleNumber);
            jsonObject.AddIntValue("sampleSizeSas", SasAlleleNumber);

            if (ObservedGains != 0) jsonObject.AddIntValue("observedGains", ObservedGains);
            if (ObservedLosses != 0) jsonObject.AddIntValue("observedLosses", ObservedLosses);

            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}