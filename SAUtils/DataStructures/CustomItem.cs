using System.Collections.Generic;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class CustomItem : ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        private Dictionary<string, string> StringValues { get; }
        private Dictionary<string, bool> BoolValues { get; }
        private Dictionary<string, double> NumValues { get; }
        

        public CustomItem(IChromosome chromosome, int start, string refAllele, string altAllele, Dictionary<string, string> stringValues, Dictionary<string, bool> boolValues, Dictionary<string, double> numValues)
        {
            Chromosome   = chromosome;
            Position     = start;
            RefAllele    = refAllele;
            AltAllele    = altAllele;
            StringValues = stringValues;
            BoolValues   = boolValues;
            NumValues    = numValues;
        }

        public string GetJsonString()
	    {
			var sb = StringBuilderCache.Acquire();
			var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("refAllele", RefAllele);
            jsonObject.AddStringValue("altAllele", AltAllele);

            if(StringValues!=null && StringValues.Count > 0 )
			    foreach ((string key, string value)  in StringValues)
			    {
			        jsonObject.AddStringValue(key, value);
			    }

	        if (NumValues != null && NumValues.Count > 0)
	            foreach ((string key, double value) in NumValues)
	            {
	                var intValue = (int) value;
                    if(value.Equals(intValue)) 
                        jsonObject.AddIntValue(key, intValue);
	                else
                        jsonObject.AddDoubleValue(key, value, "0.######");
	            }

            if (BoolValues != null && BoolValues.Count > 0)
	            foreach ((string key, bool value) in BoolValues)
	            {
	                jsonObject.AddBoolValue(key, value, true);
	            }

         //   var jsonString = StringBuilderCache.GetStringAndRelease(sb);
	        //Console.WriteLine("{" + jsonString + "}");
	        //return jsonString;

            return StringBuilderCache.GetStringAndRelease(sb);
        }

	}
}
