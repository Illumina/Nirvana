using System.Linq;
using VariantAnnotation.FileHandling.VCF;

namespace VariantAnnotation.DataStructures.VCF
{
    internal sealed class IntermediateSampleFields : IIntermediateSampleFields
    {
        public FormatIndices FormatIndices { get; }
        public string[] SampleColumns { get; }

        public int? Tir { get; private set; }
        public int? Tar { get; private set; }
        public int? TotalAlleleCount { get; private set; }
        public string VcfRefAllele { get; }
        public int? ACount { get; private set; }
        public int? CCount { get; private set; }
        public int? GCount { get; private set; }
        public int? TCount { get; private set; }
		public int? MajorChromosomeCount { get; }
	    public int? CopyNumber { get; }
        public int? NR { get; private set; }
        public int? NV { get; private set; }
        public string[] AltAlleles { get; }
		public string RepeatNumber { get; }
        public string RepeatNumberSpan { get; }
        public int? DenovoQuality { get; }

        // constructor
        public IntermediateSampleFields(string[] vcfColumns, FormatIndices formatIndices, string[] sampleCols, bool fixGatkGenomeVcf = false)
        {
            VcfRefAllele  = vcfColumns[VcfCommon.RefIndex];
            FormatIndices = formatIndices;
            SampleColumns = sampleCols;

	        if (formatIndices.MCC != null)
	        {
				if (sampleCols[formatIndices.MCC.Value] != ".")
					MajorChromosomeCount = int.Parse(sampleCols[formatIndices.MCC.Value]);
	        }

	        if (formatIndices.CN != null)
	        {
		        if (vcfColumns[VcfCommon.AltIndex].Contains("STR"))
		        {
			        RepeatNumber = sampleCols[formatIndices.CN.Value];
			        CopyNumber = null;
		        }
				else CopyNumber = int.Parse(sampleCols[formatIndices.CN.Value]);
			}
            if (formatIndices.CI != null)
            {
                if (sampleCols[formatIndices.CI.Value] != ".")
                {
                    RepeatNumberSpan = sampleCols[formatIndices.CI.Value];
                }
            }

            if (fixGatkGenomeVcf && formatIndices.AD !=null)
	        {
				sampleCols[formatIndices.AD.Value] = CorrectAdInGatkGenomeVcf(sampleCols[formatIndices.AD.Value]);
	        }
	        if (formatIndices.DQ != null)
	        {
		        DenovoQuality = GetDenovoQuality(sampleCols[formatIndices.DQ.Value]);
	        }

            AltAlleles = vcfColumns[VcfCommon.AltIndex].Split(',');

            CalculateTirTar();
            CalculateRawAlleleCounts();
            GetPlatypusCounts();
        }

	    private static int? GetDenovoQuality(string sampleDqCol)
	    {
		    int denovoQuality;
		    var parse = int.TryParse(sampleDqCol, out denovoQuality);

		    if (!parse) return null;

		    return denovoQuality;
		    
	    }

	    private static string CorrectAdInGatkGenomeVcf(string adString)
        {
            var ads = adString.Split(',');
            if (ads.Length < 3) return adString;

            ads = ads.Take(ads.Length - 1).ToArray();
            return string.Join(",", ads);
        }

        /// <summary>
        /// calculates TIR and TAR tier 1 values
        /// </summary>
        private void CalculateTirTar()
        {
            if (FormatIndices.TAR == null || FormatIndices.TIR == null) return;

            var tarString = SampleColumns[FormatIndices.TAR.Value];
            var tirString = SampleColumns[FormatIndices.TIR.Value];

            if (tarString == "." || tirString == ".") return;

            Tar = int.Parse(tarString.Split(',')[0]);
            Tir = int.Parse(tirString.Split(',')[0]);
        }

        /// <summary>
        /// calculates allele counts from tier 1
        /// </summary>
        private void CalculateRawAlleleCounts()
        {
            if (FormatIndices.AU == null || FormatIndices.CU == null || FormatIndices.GU == null ||
                FormatIndices.TU == null)
                return;

            ACount = GetTier1RawCount(FormatIndices.AU.Value);
            CCount = GetTier1RawCount(FormatIndices.CU.Value);
            GCount = GetTier1RawCount(FormatIndices.GU.Value);
            TCount = GetTier1RawCount(FormatIndices.TU.Value);

            if (ACount == null || CCount == null || GCount == null || TCount == null) return;

            TotalAlleleCount = ACount + CCount + GCount + TCount;
        }

        /// <summary>
        /// grabs the Platypus NR & NV values
        /// </summary>
        private void GetPlatypusCounts()
        {
            if (FormatIndices.NR == null || FormatIndices.NV == null) return;

            var nrString = SampleColumns[FormatIndices.NR.Value];
            var nvString = SampleColumns[FormatIndices.NV.Value];

            if (nrString == "." || nvString == ".") return;

            NR = int.Parse(nrString);
            NV = int.Parse(nvString);
        }

        /// <summary>
        /// returns a raw count given a genotype field index
        /// </summary>
        private int? GetTier1RawCount(int index)
        {
            var countString = SampleColumns[index].Split(',')[0];
            if (countString == ".") return null;

            int count;
            if (!int.TryParse(countString, out count)) return null;
            return count;
        }
    }
}
