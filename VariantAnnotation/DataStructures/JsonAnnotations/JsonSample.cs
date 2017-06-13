using System;
using System.Collections.Generic;
using System.Text;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.JsonAnnotations
{
    public sealed class JsonSample : IAnnotatedSample
    {
		#region members

		public string Genotype { get; set; }         // 0/1 (GT)
		public string VariantFrequency { get; set; } // 1.00 (AF)
		public string TotalDepth { get; set; }       // 10 (DP)
		public bool FailedFilter { get; set; }       // F (FT)
		public string[] AlleleDepths { get; set; }   // 92,21 (AD)
		public string GenotypeQuality { get; set; }  // 790 (GQX)
		public string CopyNumber { get; set; }//CN in CANVAs
		public bool IsLossOfHeterozygosity { get; set; }
		public string DenovoQuality { get; set; }
		public string RepeatNumber { get; set; }
        public string RepeatNumberSpan { get; set; }
        public string[] SplitReadCounts { get; set; }
		public string[] PairEndReadCounts { get; set; }

		public bool IsEmpty { get; set; }
        public List<String> RecomposedGenotype { get; set; }
        #endregion



        public bool IsNull()
        {
            return Genotype == null
                   && VariantFrequency == null
                   && TotalDepth == null
                   && FailedFilter == false
                   && AlleleDepths == null
                   && GenotypeQuality == null
                   && CopyNumber == null
				   && SplitReadCounts ==null
				   && PairEndReadCounts == null;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            // data section
            sb.Append(JsonObject.OpenBrace);
			 
			jsonObject.AddStringValue("variantFreq", VariantFrequency,false);
			jsonObject.AddStringValue("totalDepth", TotalDepth,false);
			jsonObject.AddStringValue("genotypeQuality", GenotypeQuality, false);
			jsonObject.AddStringValue("copyNumber", CopyNumber, false);
	        
			jsonObject.AddStringValue("repeatNumbers", RepeatNumber);
			jsonObject.AddStringValue("repeatNumberSpans",RepeatNumberSpan);

            jsonObject.AddStringValues("alleleDepths", AlleleDepths, false);
	        jsonObject.AddStringValue("genotype", Genotype);

			jsonObject.AddBoolValue("failedFilter", FailedFilter, true, "true");

            jsonObject.AddStringValues("splitReadCounts",SplitReadCounts,false);
			jsonObject.AddStringValues("pairedEndReadCounts",PairEndReadCounts,false);

			jsonObject.AddBoolValue("isEmpty", IsEmpty, true, "true");
			jsonObject.AddBoolValue("lossOfHeterozygosity", IsLossOfHeterozygosity, true, "true");

			jsonObject.AddStringValue("deNovoQuality", DenovoQuality, false);
            jsonObject.AddStringValues("recomposedGenotype",RecomposedGenotype);

			sb.Append(JsonObject.CloseBrace);
            return sb.ToString();
        }

       
    }
}
