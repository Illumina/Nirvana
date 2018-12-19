using System.Collections.Generic;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using Variants;

namespace SAUtils.DataStructures
{
    public sealed class CustomInterval : ISuppIntervalItem
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }
        public VariantType VariantType { get; }

        private Dictionary<string, string> StringValues { get; }
        private Dictionary<string, bool> BoolValues { get; }
        private Dictionary<string, double> NumValues { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public CustomInterval(IChromosome chromosome, int start, int end, Dictionary<string, string> stringValues, Dictionary<string, bool> boolValues, Dictionary<string, double> numValues)
        {
            Chromosome      = chromosome;
            Start           = start;
            End             = end;
            VariantType     = VariantType.structural_alteration;
            StringValues    = stringValues;
            BoolValues = boolValues;
            NumValues = numValues;
        }

        public string GetJsonString()
		{
			var sb = StringBuilderCache.Acquire();
			var jsonObject = new JsonObject(sb);

			jsonObject.AddIntValue("start", Start);
			jsonObject.AddIntValue("end", End);

		    if (StringValues != null && StringValues.Count > 0)
		        foreach ((string key, string value) in StringValues)
		        {
		            jsonObject.AddStringValue(key, value);
		        }

		    if (NumValues != null && NumValues.Count > 0)
		        foreach ((string key, double value) in NumValues)
		        {
		            var intValue = (int)value;
		            if (value.Equals(intValue))
		                jsonObject.AddIntValue(key, intValue);
		            else
		                jsonObject.AddDoubleValue(key, value, "0.######");
                }

		    if (BoolValues != null && BoolValues.Count > 0)
		        foreach ((string key, bool value) in BoolValues)
		        {
		            jsonObject.AddBoolValue(key, value, true);
		        }

      //      var jsonString = StringBuilderCache.GetStringAndRelease(sb);
		    //Console.WriteLine("{"+jsonString+"}");
		    //return jsonString;

            return StringBuilderCache.GetStringAndRelease(sb);
        }
	}
}
