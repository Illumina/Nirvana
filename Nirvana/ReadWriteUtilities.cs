using System;
using System.Collections.Generic;
using System.IO;
using Compression.Utilities;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using Vcf;

namespace Nirvana
{
	public static class ReadWriteUtilities
	{
		public static StreamWriter GetOutputWriter(string outputPath)
		{
		    return ConfigurationSettings.OutputFileName == "-"
		        ? new StreamWriter(Console.OpenStandardOutput())
		        : GZipUtilities.GetStreamWriter(outputPath+".json.gz");

        }

	    internal static IVcfReader GetVcfReader(string vcfPath, IDictionary<string, IChromosome> chromosomeDictionary,IRefMinorProvider refMinorProvider,bool verboseTranscript)
		{
			return new VcfReader(GZipUtilities.GetAppropriateReadStream(vcfPath), chromosomeDictionary, refMinorProvider,verboseTranscript);
		}

	    public static StreamWriter GetVcfOutputWriter(string outputPath)
	    {
	        return ConfigurationSettings.OutputFileName == "-"
	            ? new StreamWriter(Console.OpenStandardOutput())
	            : GZipUtilities.GetStreamWriter(outputPath + ".vcf.gz");

	    }
	    public static StreamWriter GetGvcfOutputWriter(string outputPath)
	    {
	        return ConfigurationSettings.OutputFileName == "-"
	            ? new StreamWriter(Console.OpenStandardOutput())
	            : GZipUtilities.GetStreamWriter(outputPath + ".genome.vcf.gz");

	    }
    }
}